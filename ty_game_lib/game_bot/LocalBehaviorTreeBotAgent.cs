using System;
using System.Collections;
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
        public PatrolCtrl PatrolCtrl { get; }
        private ComboCtrl ComboCtrl { get; }
        private FirstSkillCtrl FirstSkillCtrl { get; }
        public CharacterBody BotBody { get; }

        private ICanBeEnemy? TempTarget { get; set; }

        private List<(float range, int maxAmmoUse, int weaponIndex)> RangeToWeapon { get; set; }

        private TwoDPoint? TracePt { get; set; }

        private TwoDVector? TraceAim { get; set; }

        private uint NowTraceTick { get; set; }

        private HashSet<Trap> TrapsRecords { get; }

        private List<TwoDPoint> TempPath { get; set; }

        private bool IsSlowTempPath { get; set; }

        public static LocalBehaviorTreeBotAgent GenByConfig(battle_bot battleBot, CharacterBody body,
            PathTop? pathTop)
        {
            var polyCount = pathTop?.GetPolyCount() ?? 0;
            var random = BehaviorTreeFunc.Random;
            var next = random.Next((int)(polyCount * BotLocalConfig.PatrolMin),
                (int)(polyCount * BotLocalConfig.PatrolMax + 1));
            var twoDPoints = pathTop?.GetPatrolPts(random, next) ?? new List<TwoDPoint>();
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
            ComboCtrl = comboCtrl;
            FirstSkillCtrl = firstSkillCtrl;
            BotBody = botBody;
            TempTarget = null;
            RangeToWeapon = rangeToWeapon;
            IsSlowTempPath = true;
            TracePt = null;
            TraceAim = null;
            NowTraceTick = 0;
            TrapsRecords = new HashSet<Trap>();
            TempPath = new List<TwoDPoint>();
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
            ImmutableHashSet<IHitMsg> immutableHashSet, PathTop? pathTop)
        {
            var agentStatusList = new List<IAgentStatus>();

            //bodyStatus
            var use = new PropUse();
            var startPt = BotBody.GetAnchor();
            var rangeToWeapon = new BodyStatus(RangeToWeapon, BotBody);
            agentStatusList.Add(rangeToWeapon);

            var bodyCharacterStatus = BotBody.CharacterStatus;
            var checkAppStatusToBotPropUse =
                bodyCharacterStatus.Prop?.CheckAppStatusToBotPropUse(bodyCharacterStatus) ?? false;
            if (checkAppStatusToBotPropUse)
            {
                agentStatusList.Add(use);
            }
            //botMemory

            var botMemory = new BotMemory(ComboCtrl, FirstSkillCtrl);
            agentStatusList.Add(botMemory);

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

                NowTraceTick = 0;
                var twoDPoint = nearestTarget.GetAnchor();
                var twoDPoints = pathTop?.FindGoPts(startPt, twoDPoint) ?? new[] {twoDPoint};
                TempPath = twoDPoints.ToList();
                IsSlowTempPath = false;
                var targetMsg = new TargetMsg(nearestTarget);
                agentStatusList.Add(targetMsg);
                TracePt = null;
                TraceAim = null;
            }
            else
            {
                if (TempTarget == null)
                {
                    var tickMsg = BotBody.GenCharTickMsg();
                    var hitMarks = tickMsg.CharEvents.OfType<HitMark>();
                    var firstOrDefault = hitMarks.FirstOrDefault();

                    if (firstOrDefault != null)
                    {
                        var twoDVector = firstOrDefault.HitDirV;
                        TraceAim = twoDVector;
                        NowTraceTick += CommonConfig.OtherConfig.BotAimTraceDefaultTime;
                    }
                }

                if (TempTarget != null)
                {
                    // 丢失目标，需要记录跟踪点和方向


                    NowTraceTick = CommonConfig.OtherConfig.BotAimTraceDefaultTime;


                    var twoDPoint = TempTarget.GetAnchor();
                    TracePt = twoDPoint;
#if DEBUG
                    Console.Out.WriteLine(
                        $"{BotBody.GetId()} LossTarget Start Trace To Pt {TracePt} tick:{NowTraceTick}");
#endif
                    if (TempTarget is CharacterBody characterBody)
                    {
                        var twoDVector = characterBody.GetAim();
                        TraceAim = twoDVector;
                    }
                }
            }

            TempTarget = nearestTarget;
            var ctrlPoints = PatrolCtrl.Points;

            if (NowTraceTick > 0)
            {
                if (TracePt == null || TracePt.GetDistance(startPt) < BotLocalConfig.CloseEnoughDistance)
                {
                    TracePt = null;
                    if (TraceAim != null)
                    {
                        var traceToAimMsg = new TraceToAimMsg(TraceAim);
                        agentStatusList.Add(traceToAimMsg);
                    }

                    NowTraceTick--;
                }
                else
                {
#if DEBUG
                    Console.Out.WriteLine($"{BotBody.GetId()}  Trace MayBePt {TracePt}");
#endif
                    var twoDPoints = pathTop?.FindGoPts(startPt, TracePt) ?? new[] { TracePt };
                    TempPath = twoDPoints.ToList();
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
                        var twoDPoints = pathTop?.FindGoPts(startPt, twoDPoint) ?? new[] { twoDPoint };
                        TempPath = twoDPoints.ToList();
                        IsSlowTempPath = false;
                    }
                }
            }

            if (TempPath.Any())
            {
                var twoDPoint = TempPath.First();
                if (twoDPoint.GetDistance(startPt) < BotLocalConfig.CloseEnoughDistance)
                {
                    TempPath.RemoveAt(0);
                }
                else
                {
                    traceMsg = new TraceToPtMsg(twoDPoint, IsSlowTempPath);
                }
            }
            else if (NowTraceTick == 0 || TempTarget==null)
            {
                var p = -1;
                var l = -1f;
                for (var i = 0; i < ctrlPoints.Length; i++)
                {
                    var twoDPoint2 = ctrlPoints[i];
                    var distance = twoDPoint2.GetDistance(startPt);
                    var b2 = distance < l || l < 0;
                    if (!b2) continue;
                    p = i;
                    l = distance;
                }

                var twoDPoint = p < 0 ? null : ctrlPoints[p];

                if (twoDPoint != null)
                {
                    var b1 = twoDPoint.GetDistance(startPt) < BotLocalConfig.CloseEnoughDistance;
                    if (!b1)
                    {
                        var twoDPoints = pathTop?.FindGoPts(startPt, twoDPoint) ?? new[] { twoDPoint };
                        TempPath = twoDPoints.ToList();
                    }
                    else
                    {
                        var random = BehaviorTreeFunc.Random;
                        var next = random.Next(PatrolCtrl.GetPtNum());
                        var ptNum = PatrolCtrl.TakePt(p, next).ToList();
                        TempPath = ptNum;
                        IsSlowTempPath = true;
                        var nextDouble = (float)random.NextDouble() * MathTools.Pi();
                        TraceAim = new TwoDVector(MathTools.Cos(nextDouble), MathTools.Sin(nextDouble));
                        NowTraceTick = CommonConfig.OtherConfig.BotAimTraceDefaultTime;

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


            if (traceMsg != null) agentStatusList.Add(traceMsg);

            return agentStatusList.ToArray();
        }

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