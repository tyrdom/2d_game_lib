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
    public class LocalBehaviorTreeBot
    {
        private IBehaviorTreeNode StartNode { get; }


        public PatrolCtrl PatrolCtrl { get; }
        public ComboCtrl ComboCtrl { get; }
        public FirstSkillCtrl FirstSkillCtrl { get; }
        private CharacterBody BotBody { get; }

        private ICanBeEnemy? TempTarget { get; set; }

        public List<(float range, int maxAmmoUse, int weaponIndex)> RangeToWeapon { get; set; }

        private TwoDPoint? TracePt { get; set; }

        private TwoDVector? TraceAim { get; set; }

        private uint NowTraceTick { get; set; }

        private HashSet<Trap> TrapsRecords { get; }

        private List<TwoDPoint> TempPath { get; set; }

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
            var startPt = BotBody.GetAnchor();
            var rangeToWeapon = new BodyStatus(RangeToWeapon, BotBody);
            agentStatusList.Add(rangeToWeapon);


            // is direct hit sth
            var b = immutableHashSet.OfType<BulletHit>().Any();
            if (b)
            {
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

            var beAndNeedHit = canBeHits.Aggregate((ICanBeAndNeedHit?) null,
                Func);
            var nearestTarget = beAndNeedHit ?? TrapsRecords.Aggregate((ICanBeAndNeedHit?) null, Func);
            if (nearestTarget == null) //没有目标的时候
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
                    NowTraceTick += CommonConfig.OtherConfig.BotAimTraceDefaultTime;
                    var twoDPoint = TempTarget.GetAnchor();
                    TracePt = twoDPoint;
                    if (TempTarget is CharacterBody characterBody)
                    {
                        var twoDVector = characterBody.GetAim();
                        TraceAim = twoDVector;
                    }
                }
            }
            else
            {
                NowTraceTick = 0;
                var targetMsg = new TargetMsg(nearestTarget);
                agentStatusList.Add(targetMsg);
                TracePt = null;
                TraceAim = null;
            }


            if (NowTraceTick > 0)
            {
                if (TracePt == null || TracePt.GetDistance(startPt) < BotLocalConfig.CloseEnoughDistance)
                {
                    if (TraceAim != null)
                    {
                        var traceToAimMsg = new TraceToAimMsg(TraceAim);
                        agentStatusList.Add(traceToAimMsg);
                    }

                    NowTraceTick--;
                }
                else
                {
                    traceMsg = new TraceToPtMsg(TracePt);
                }
            }

            TempTarget = nearestTarget;
            var ctrlPoints = PatrolCtrl.Points;
            if (TempTarget == null && NowTraceTick <= 0)
            {
                var radarSees = canBeEnemies.OfType<RadarSee>();
                var enumerable = radarSees as RadarSee[] ?? radarSees.ToArray();
                var any = enumerable.Any();
                if (any)
                {
                    var canBeEnemy = enumerable.Aggregate((ICanBeEnemy?) null, Nearest);
                    if (canBeEnemy != null)
                    {
                        var twoDPoint = canBeEnemy.GetAnchor();
                        var twoDPoints = pathTop?.FindGoPts(startPt, twoDPoint) ?? new[] {twoDPoint};
                        TempPath = twoDPoints.ToList();
                    }
                }
                else
                {
                    var twoDPoint = ctrlPoints.Aggregate((TwoDPoint?) null, Nearest);
                    if (twoDPoint != null)
                    {
                        var twoDPoints = pathTop?.FindGoPts(startPt, twoDPoint) ?? new[] {twoDPoint};
                        TempPath = twoDPoints.ToList();
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
                    traceMsg = new TraceToPtMsg(twoDPoint);
                }
            }
            else
            {
                var p = -1;
                for (var i = 0; i < ctrlPoints.Length; i++)
                {
                    var twoDPoint = ctrlPoints[i];
                    var b1 = twoDPoint.GetDistance(startPt) < BotLocalConfig.CloseEnoughDistance;
                    if (!b1) continue;
                    p = i;
                    break;
                }

                if (p >= 0)
                {
                    PatrolCtrl.SetNowPt(p);

                    var ptNum = PatrolCtrl.NextPt(10).ToList();
                    TempPath = ptNum;
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
        //
        // public Operate? GoATick()
        // {
        //     StartNode.DoNode()
        // }
    }

    public readonly struct BodyStatus : IAgentStatus
    {
        public List<(float range, int maxAmmoUse, int weaponIndex)> NowRangeToWeapon { get; }

        public CharacterBody CharacterBody { get; }


        public BodyStatus(List<(float range, int maxAmmoUse, int weaponIndex)> nowRangeToWeapon,
            CharacterBody characterBody)
        {
            NowRangeToWeapon = nowRangeToWeapon;

            CharacterBody = characterBody;
        }
    }

    public readonly struct TargetMsg : IAgentStatus
    {
        public ICanBeAndNeedHit Target { get; }

        public TargetMsg(ICanBeAndNeedHit target)
        {
            Target = target;
        }
    }

    public record TraceToPtMsg : IAgentStatus
    {
        public TwoDPoint TracePt { get; }


        public TraceToPtMsg(TwoDPoint tracePt)
        {
            TracePt = tracePt;
        }
    }

    public record TraceToAimMsg : IAgentStatus
    {
        public TraceToAimMsg(TwoDVector aim)
        {
            Aim = aim;
        }

        public TwoDVector Aim { get; }
    }

    public record HitSth : IAgentStatus
    {
    }
}