using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using cov_path_navi;
using game_config;
using game_stuff;

namespace game_bot
{
    public class LocalBehaviorTreeBotAgent
    {
        private List<TwoDPoint> TempPath1;
        private TwoDVector? TraceAim1;
        public PatrolCtrl PatrolCtrl { get; }

        private BotMemory BotMemory { get; }
        public CharacterBody BotBody { get; }
        private ICanBeEnemy? TempTarget { get; set; }
        private int NearestPathTick { get; set; }
        private List<(float range, int maxAmmoUse, int weaponIndex)> RangeToWeapon { get; }
        private TwoDPoint? TracePt { get; set; }

        private TwoDVector? TraceAim
        {
            get => TraceAim1;
            set
            {
                BotMemory.AimTraced = value == null;
                TraceAim1 = value;
            }
        }

        private bool NeedGenTracePath { get; set; }
        private int NowTraceTick { get; set; }
        private int GoEnemyTick { get; set; }
        private HashSet<Trap> TrapsRecords { get; }

        private bool TempPathChanged { get; set; }

        private List<TwoDPoint> TempPath
        {
            get => TempPath1;
            set
            {
                TempPathChanged = true;
                TempPath1 = value;
            }
        }

        private TwoDVectorLine[] GoThroughLines { get; set; }

        public bool TryToGetTempPath(out TwoDPoint[] tempPath, out TwoDVectorLine[] goThroughLines)
        {
            if (TempPathChanged)
            {
                var botBodyNowPos = BotBody.NowPos;
                TempPathChanged = false;
                var twoDPoints = new List<TwoDPoint> { botBodyNowPos };
                twoDPoints.AddRange(TempPath);
                tempPath = twoDPoints.ToArray();
                goThroughLines = GoThroughLines;
                return true;
            }

            tempPath = Array.Empty<TwoDPoint>();
            goThroughLines = Array.Empty<TwoDVectorLine>();
            return false;
        }

        private bool IsSlowTempPath { get; set; }

        public static LocalBehaviorTreeBotAgent GenByConfig(battle_bot battleBot, CharacterBody body,
            PathTop pathTop)
        {
            var polyCount = pathTop.GetPolyCount();
            var random = BehaviorTreeFunc.Random;
            var next = random.Next((int)(polyCount * BotLocalConfig.BotOtherConfig.PatrolMin),
                (int)(polyCount * BotLocalConfig.BotOtherConfig.PatrolMax + 1));
            var twoDPoints = pathTop.GetPatrolPts(random, next);
            var patrolCtrl = new PatrolCtrl(twoDPoints);
            var battleNpcActWeight = battleBot.ActWeight;
            var weight = battleNpcActWeight.FirstOrDefault(x => x.op == botOp.none)?.weight ?? 0;
            var valueTuples = battleNpcActWeight.Where(x => x.op != botOp.none)
                .Select(x => (x.weight, BehaviorTreeFunc.CovOp(x.op)));
            var firstSkillCtrl = new FirstSkillCtrl(valueTuples, weight,
                (int)(battleBot.DoNotMinMaxTime.item2),
                (int)battleBot.DoNotMinMaxTime.item1);
            var comboCtrl = new ComboCtrl(battleBot.MaxCombo);
            var valueTuples2 = GetRangeAmmoWeapon(body.CharacterStatus.GetWeapons(), body.GetSize());
            var localBehaviorTreeBotAgent =
                new LocalBehaviorTreeBotAgent(patrolCtrl, comboCtrl, firstSkillCtrl, body, valueTuples2);
            return localBehaviorTreeBotAgent;
        }

        private LocalBehaviorTreeBotAgent(PatrolCtrl patrolCtrl, ComboCtrl comboCtrl, FirstSkillCtrl firstSkillCtrl,
            CharacterBody botBody,
            List<(float range, int maxAmmoUse, int weaponIndex)> rangeToWeapon)
        {
            PatrolCtrl = patrolCtrl;
            BotMemory = new BotMemory(comboCtrl, firstSkillCtrl);
            BotBody = botBody;
            TempTarget = null;
            RangeToWeapon = rangeToWeapon;
            IsSlowTempPath = true;
            TracePt = null;
            TraceAim = null;
            NeedGenTracePath = false;
            NowTraceTick = 0;
            GoEnemyTick = 0;
            NearestPathTick = 0;
            TrapsRecords = new HashSet<Trap>();
            TempPath1 = new List<TwoDPoint>();
            TempPathChanged = false;

            GoThroughLines = Array.Empty<TwoDVectorLine>();
        }

        private static List<(float range, int maxAmmoUse, int weaponIndex)> GetRangeAmmoWeapon(
            Dictionary<int, Weapon> weapons,
            size bodySize)
        {
            var valueTuples = (from keyValuePair in weapons
                let weapon = keyValuePair.Value
                let min = weapon.BotRanges
                let i = weapon.SkillGroups[bodySize].Values
                    .Max(skills => skills.Values.Max(skill => skill.AmmoCost))
                select (min, i, keyValuePair.Key)).ToList();

            valueTuples.Sort((x, y) => -x.Item1.CompareTo(y.Item1));
            return valueTuples;
        }

        public IAgentStatus[] GenAgentStatus(PlayerTickSense playerTickSense,
            ImmutableHashSet<IHitMsg> immutableHashSet, PathTop pathTop)
        {
            var agentStatusList = new List<IAgentStatus>();

            //bodyStatus
            var use = new PropUse();
            var startPt = BotBody.GetAnchor();
            var rangeToWeapon = new BodyStatus(RangeToWeapon, BotBody);
            agentStatusList.Add(rangeToWeapon);

            var bodyCharacterStatus = BotBody.CharacterStatus;
            if (bodyCharacterStatus.IsDeadOrCantDmg())
            {
                ClearTempPath();
                return agentStatusList.ToArray();
            }

            var checkAppStatusToBotPropUse =
                bodyCharacterStatus.Prop?.CheckAppStatusToBotPropUse(bodyCharacterStatus) ?? false;
            if (checkAppStatusToBotPropUse)
            {
                agentStatusList.Add(use);
            }
            //botMemory

            agentStatusList.Add(BotMemory);

            // is direct hit sth
            var b = immutableHashSet.OfType<BulletHit>().Any();
            if (b)
            {
#if DEBUG
                Console.Out.WriteLine($"{BotBody.GetId()} hit sth ");
#endif
                var hitSth = new HitSth();
                agentStatusList.Add(hitSth);
            }

            TraceToPtMsg? traceMsg = null;
            //GenLockTarget
            var notMoveCanBeAndNeedSews = playerTickSense.AppearNotMove.OfType<Trap>()
                .Where(x => x.GetTeam() != BotBody.Team);
            var moveCanBeAndNeedSews = playerTickSense.VanishNotMove.OfType<Trap>();
            TrapsRecords.UnionWith(notMoveCanBeAndNeedSews);
            TrapsRecords.ExceptWith(moveCanBeAndNeedSews);
            var canBeEnemies = playerTickSense.OnChangingBodyAndRadarSee.ToList();

            var canBeHits = canBeEnemies.OfType<ICanBeAndNeedHit>()
                .Where(x => x.GetTeam() != BotBody.Team);

            ICanBeAndNeedHit? Func(ICanBeAndNeedHit? s, ICanBeAndNeedHit x) =>
                Nearest(s, x) is ICanBeAndNeedHit canBeAndNeedHit ? canBeAndNeedHit : null;

            var beAndNeedHit = canBeHits.Aggregate((ICanBeAndNeedHit?)null,
                Func);
            var nearestTarget = beAndNeedHit ?? TrapsRecords.Aggregate((ICanBeAndNeedHit?)null, Func);

            if (NearestPathTick > 0)
            {
                NearestPathTick--;
            }

            if (nearestTarget != null)
            {
                if (TempTarget == null)
                {
                    var botUseWhenSeeEnemy =
                        bodyCharacterStatus.Prop?.BotUseWhenSeeEnemy(bodyCharacterStatus) ?? false;
                    if (botUseWhenSeeEnemy)
                    {
                        agentStatusList.Add(use);
                    }
                }

                if (NearestPathTick <= 0) //重新定路径CD
                {
                    NearestPathTick = (int)BotLocalConfig.BotOtherConfig.LockTraceTickTime;
                    var twoDPoint = nearestTarget.GetAnchor();
                    var twoDPoints =
                        pathTop.FindGoPts(startPt, twoDPoint, BotLocalConfig.BotOtherConfig.NaviPathGoThroughMulti,
#if DEBUG
#endif
                            out var pathGoThroughLine
                        );
                    SetPath(twoDPoints, pathGoThroughLine);


#if DEBUG
                    Console.Out.WriteLine($"bot::{BotBody.GetId()} find enemy go path:{WayPtString()}");
#endif
                    IsSlowTempPath = false;
                    GoEnemyTick = (int)BotLocalConfig.BotOtherConfig.LockTraceTickTime;
                }

                NowTraceTick = 0;
                var targetMsg = new TargetMsg(nearestTarget);
                agentStatusList.Add(targetMsg);
                TracePt = null;
                TraceAim = null;
                TempTarget = nearestTarget;
            }
            else
            {
                var tickMsg = BotBody.GenCharTickMsg();
                var hitMarks = tickMsg.CharEvents.OfType<HitMark>();
                var firstOrDefault = hitMarks.FirstOrDefault();

                if (firstOrDefault != null)
                    //有某方向攻击，放弃追踪目标 向那个方向看
                {
                    ClearTempPath();
                    NeedGenTracePath = false;
                    TracePt = null;
                    var twoDVector = firstOrDefault.HitDirV;
                    TraceAim = twoDVector;
                    NowTraceTick = (int)BotLocalConfig.BotOtherConfig.BotAimTraceDefaultTime;
#if DEBUG
                    Console.Out.WriteLine($"bot::{BotBody.GetId()} be hit by {TraceAim}");
#endif
                }


                if (TempTarget != null)
                {
                    // 丢失目标一段时间，需要记录跟踪点和方向

                    NowTraceTick = (int)BotLocalConfig.BotOtherConfig.BotAimTraceDefaultTime;
                    var twoDPoint = TempTarget.GetAnchor();
                    TracePt = twoDPoint;
                    NeedGenTracePath = true;
                    TempTarget = null;
#if DEBUG
                    Console.Out.WriteLine(
                        $"bot::{BotBody.GetId()} LossTarget Start Trace To Pt {TracePt} tick:{NowTraceTick}");
#endif
                    if (TempTarget is CharacterBody characterBody)
                    {
                        var twoDVector = characterBody.GetAim();
                        TraceAim = twoDVector;
                    }
                }
            }


            var ctrlPoints = PatrolCtrl.Points;

            if (NowTraceTick > 0 && GoEnemyTick <= 0)
            {
                if (TracePt == null || TracePt.GetSqDistance(startPt) <
                    BotLocalConfig.BotOtherConfig.CloseEnoughDistance *
                    BotLocalConfig.BotOtherConfig.CloseEnoughDistance)
                {
                    NeedGenTracePath = false;
                    TracePt = null;
                    if (TraceAim != null)
                    {
                        var traceToAimMsg = new TraceToAimMsg(TraceAim);

#if DEBUG
                        Console.Out.WriteLine($"bot::{BotBody.GetId()}  Trace Aim {TraceAim}");
#endif

                        agentStatusList.Add(traceToAimMsg);
                    }

                    if (BotMemory.AimTraced)
                    {
                        NowTraceTick--;
                    }
                }
                else if (NeedGenTracePath)
                {
                    var twoDPoints =
                        pathTop.FindGoPts(startPt, TracePt, BotLocalConfig.BotOtherConfig.NaviPathGoThroughMulti,
                            out var pathGoThroughLine);
                    SetPath(twoDPoints, pathGoThroughLine);

                    NeedGenTracePath = false;
#if DEBUG
                    var aggregate = WayPtString();
                    Console.Out.WriteLine($"bot::{BotBody.GetId()}  Trace MayBePt {TracePt} way point {aggregate}");
#endif
                    IsSlowTempPath = false;
                }
            }


            else if (TempTarget == null)
            {
                var radarSees = canBeEnemies.OfType<RadarSee>();
                var enumerable = radarSees as RadarSee[] ?? radarSees.ToArray();
                var any = enumerable.Any();
                if (any)
                {
                    var canBeEnemy = enumerable.Aggregate((ICanBeEnemy?)null, Nearest);
                    if (canBeEnemy != null)
                    {
                        var twoDPoint = canBeEnemy.GetAnchor();
                        var twoDPoints =
                            pathTop.FindGoPts(startPt, twoDPoint,
                                BotLocalConfig.BotOtherConfig.NaviPathGoThroughMulti, out var pathGoThroughLine);
                        SetPath(twoDPoints, pathGoThroughLine);
#if DEBUG
                        Console.Out.WriteLine(
                            $"bot::{BotBody.GetId()} may be enemy at {twoDPoint} way point {WayPtString()}");
#endif
                        IsSlowTempPath = false;
                    }
                }
            }

            if (GoEnemyTick > 0)
            {
                GoEnemyTick--;
            }


            if (TempPath.Any())
            {
                var twoDPoint = TempPath.First();
                if (twoDPoint.GetSqDistance(startPt) <
                    BotLocalConfig.BotOtherConfig.CloseEnoughDistance *
                    BotLocalConfig.BotOtherConfig.CloseEnoughDistance)
                {
#if DEBUG
                    Console.Out.WriteLine($"bot::{BotBody.GetId()} reach first pt TempPath {WayPtString()}");
#endif
                    TempPath.RemoveAt(0);
                    TempPathChanged = true;
                }

                var firstOrDefault = TempPath.FirstOrDefault();

                if (firstOrDefault != null)
                {
#if DEBUG
                    Console.Out.WriteLine($"bot::{BotBody.GetId()} go the TempPath {WayPtString()}");
#endif
                    traceMsg = new TraceToPtMsg(twoDPoint, IsSlowTempPath);
                }
            }
            else if (NowTraceTick == 0)
            {
                if (nearestTarget != null) //这时有目标，则清除寻路CD，让下一帧去找目标路径
                {
                    NearestPathTick = 0;
                }
                else
                {
                    var p = -1;
                    var l = -1f;
                    for (var i = 0; i < ctrlPoints.Length; i++)
                    {
                        var twoDPoint2 = ctrlPoints[i];
                        var distance = twoDPoint2.GetSqDistance(startPt);
                        var b2 = distance < l || l < 0;
                        if (!b2) continue;
                        p = i;
                        l = distance;
                    }

                    var twoDPoint = p < 0 ? null : ctrlPoints[p];


                    if (twoDPoint != null)
                    {
                        var b1 = twoDPoint.GetSqDistance(startPt) <
                                 BotLocalConfig.BotOtherConfig.CloseEnoughDistance *
                                 BotLocalConfig.BotOtherConfig.CloseEnoughDistance;
                        if (!b1)
                        {
                            var twoDPoints =
                                pathTop.FindGoPts(startPt, twoDPoint,
                                    BotLocalConfig.BotOtherConfig.NaviPathGoThroughMulti, out var pathGoThroughLine);
                            SetPath(twoDPoints, pathGoThroughLine);

#if DEBUG
                        Console.Out.WriteLine(
                            $"bot::{BotBody.GetId()} return to patrol way from {startPt} to {twoDPoint} pt {WayPtString()}");
#endif
                        }
                        else
                        {
                            var random = BehaviorTreeFunc.Random;
                            var next = random.Next(PatrolCtrl.GetPtNum() - 1) + 1;
                            var ptNum = PatrolCtrl.TakePt(p, next).ToList();
                            TempPath = ptNum;
                            IsSlowTempPath = true;

#if DEBUG
                        Console.Out.WriteLine(
                            $"bot::{BotBody.GetId()} close to patrol way from {startPt} to {twoDPoint} pt {WayPtString()}");
#endif

                            var nextDouble = (float)random.NextDouble() * MathTools.Pi();
                            TraceAim = new TwoDVector(MathTools.Cos(nextDouble), MathTools.Sin(nextDouble));
                            TracePt = null;
                            NowTraceTick = (int)BotLocalConfig.BotOtherConfig.BotAimTraceDefaultTime;

                            var canUseWhenPatrol =
                                bodyCharacterStatus.Prop?.CanUseWhenPatrol(bodyCharacterStatus,
                                    BehaviorTreeFunc.Random) ?? false;
                            if (canUseWhenPatrol)
                            {
                                agentStatusList.Add(use);
                            }
                        }
                    }
                }
            }


            if (traceMsg == null) return agentStatusList.ToArray();

#if DEBUG
            Console.Out.WriteLine(
                $"bot::{BotBody.GetId()} go trace pt {traceMsg.TracePt}");
#endif
            agentStatusList.Add(traceMsg);

            return agentStatusList.ToArray();
        }

        private void SetPath(IEnumerable<TwoDPoint> twoDPoints,
            List<(int polyId, TwoDVectorLine? gothroughLine)> pathGoThroughLine)
        {
            TempPath = twoDPoints.ToList();
            GoThroughLines = pathGoThroughLine.Select(x => x.gothroughLine).Where(xx => xx != null)
                .ToArray()!;
        }

        private void ClearTempPath()
        {
            TempPath.Clear();
            TempPathChanged = true;
        }

        // #if DEBUG
        private string WayPtString()
        {
            return TempPath.Aggregate("", (s, x) => s + "=>" + x);
        }
// #endif


        private TwoDPoint Nearest(TwoDPoint? s, TwoDPoint x)
        {
            if (s == null)
            {
                return x;
            }

            var distance1 = BotBody.GetAnchor().GetDistance(s);
            var distance2 = BotBody.GetAnchor().GetDistance(x);
            var canBeHit = distance1 > distance2 ? x : s;
            return canBeHit;
        }

        private ICanBeEnemy Nearest(ICanBeEnemy? s, ICanBeEnemy x)
        {
            if (s == null)
            {
                return x;
            }

            var distance1 = BotBody.GetAnchor().GetDistance(s.GetAnchor());
            var distance2 = BotBody.GetAnchor().GetDistance(x.GetAnchor());
            var canBeHit = distance1 > distance2 ? x : s;
            return canBeHit;
        }
    }
}