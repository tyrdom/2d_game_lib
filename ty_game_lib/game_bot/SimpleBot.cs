using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using collision_and_rigid;
using cov_path_navi;
using game_config;
using game_stuff;

namespace game_bot
{
    public enum BotStatus
    {
        OnPatrol,
        TargetApproach,
        EngageAct,
        GoMaybe
    }

    public class SimpleBot
    {
        public CharacterBody BotBody { get; }
        public Dictionary<int, ImmutableArray<(float, SkillAction)>> SkillToTrickRange { get; set; }

        public Patrol Patrol { get; }

        public BotSkillCtrl BotSkillCtrl { get; }
        private BotStatus BotStatus { get; set; }
        public List<(float maxRange, int maxAmmoUse, int weaponIndex)> RangeToWeapon { get; set; }


        private int? MyPoly { get; set; }
        public int NowWeapon { get; set; }
        private List<TwoDPoint> PathPoints { get; }
        private Random Random { get; }

        public SkillAction DelaySkillAction { get; set; }

        public int DelayTick { get; set; }
        private TwoDPoint? TargetRecordPos { get; set; }

        public SimpleBot(CharacterBody botBody, Random random, List<TwoDPoint> patrolPts, BotSkillCtrl botSkillCtrl)
        {
            var valueTuples = new List<(float, int, int)>();
            var dictionary = new Dictionary<int, ImmutableArray<(float, SkillAction)>>();
            foreach (var keyValuePair in botBody.CharacterStatus.Weapons)
            {
                var weapon = keyValuePair.Value;
                var max = weapon.BotRanges.Select(x => x.Item1).Max();
                var i = weapon.SkillGroups[botBody.GetSize()].Values
                    .Max(skills => skills.Values.Max(skill => skill.AmmoCost));
                valueTuples.Add((max, i, keyValuePair.Key));
                dictionary[keyValuePair.Key] = weapon.BotRanges;
            }

            valueTuples.Sort((x, y) => -x.Item1.CompareTo(y.Item1));

            SkillToTrickRange = dictionary;
            BotBody = botBody;
            Random = random;
            BotSkillCtrl = botSkillCtrl;
            BotStatus = BotStatus.OnPatrol;
            Patrol = new Patrol(patrolPts);
            PathPoints = new List<TwoDPoint>();
            RangeToWeapon = valueTuples;
            NowWeapon = 0;
            MyPoly = null;
            TargetRecordPos = null;
        }


        private Operate? SeeATarget(List<TwoDPoint> canBeHitPts)
        {
            var twoDPoint = NearPt(canBeHitPts);
            TargetRecordPos = twoDPoint;
            BotStatus = BotStatus.TargetApproach;
            var valueTuple = BotBody.CharacterStatus.Prop?.BotUse(bot_use_cond.EnemyOnSight,
                BotBody.CharacterStatus.NowPropPoint);
            if (valueTuple.HasValue && valueTuple.Value.canUse)
            {
                return new Operate(specialAction: SpecialAction.UseProp);
            }

            return null;
        }

        public void ReturnOnPatrol(PathTop pathTop)
        {
            BotStatus = BotStatus.OnPatrol;
            var twoDPoint = Patrol.GetNowPt();
            var twoDPoints = pathTop.FindGoPts(BotBody.GetAnchor(), twoDPoint);
            PathPoints.AddRange(twoDPoints);
        }


        public Operate? BotSimpleTick(IEnumerable<ICanBeEnemy> perceivable, PathTop pathTop)
        {
            var canBeEnemies = perceivable.ToList();
            var canBeHitPts = canBeEnemies.OfType<ICanBeHit>()
                .Where(x => x.GetTeam() != BotBody.Team)
                .Select(x => x.GetAnchor()).ToList();
            var radarSees = canBeEnemies.OfType<RadarSee>().Select(x => x.GetAnchor()).ToList();
            switch (BotStatus)
            {
                case BotStatus.OnPatrol:
                    if (canBeHitPts.Any())
                    {
                        return SeeATarget(canBeHitPts);
                    }

                    if (radarSees.Any())
                    {
                        var twoDPoint = NearPt(radarSees);
                        var twoDPoints = pathTop.FindGoPts(BotBody.GetAnchor(), twoDPoint);
                        PathPoints.Clear();
                        PathPoints.AddRange(twoDPoints);
                        BotStatus = BotStatus.GoMaybe;
                        return null;
                    }

                    DoPatrol();
                    break;
                case BotStatus.GoMaybe:
                    if (canBeHitPts.Any())
                    {
                        return SeeATarget(canBeHitPts);
                    }

                    var goPathDirection = GoPathDirection();
                    if (goPathDirection == null)
                    {
                        ReturnOnPatrol(pathTop);
                        return null;
                    }

                    break;
                case BotStatus.TargetApproach:
                    if (canBeHitPts.Any())
                    {
                        var twoDPoint = NearPt(canBeHitPts);

                        var distance = twoDPoint.GetDistance(BotBody.GetAnchor());
                        CheckWeaponAndAmmo(distance);
                    }

                    if (TargetRecordPos != null)
                    {
                    }


                    break;
                case BotStatus.EngageAct:

                    CheckAct();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private bool CheckAct()
        {
            throw new NotImplementedException();
        }

        private void CheckWeaponAndAmmo(float distance)
        {
            throw new NotImplementedException();
        }

        private TwoDPoint NearPt(List<TwoDPoint> pts)
        {
            pts.Sort((a, b) =>
                BotBody.GetAnchor().GetDistance(a)
                    .CompareTo(BotBody.GetAnchor().GetDistance(b)));
            var nearPt = pts.First();
            return nearPt;
        }

        private void DoPatrol()
        {
            throw new NotImplementedException();
        }


        private TwoDVector? GoPathDirection()
        {
            var moveVectorLine = BotBody.GetMoveVectorLine();
            while (true)
            {
                var firstOrDefault = PathPoints.FirstOrDefault();

                if (firstOrDefault == null) return null;

                if (firstOrDefault.GetDistance(moveVectorLine) >= 0.3f)
                {
                    return new TwoDVector(BotBody.GetAnchor(), firstOrDefault).GetUnit2();
                }

                PathPoints.RemoveAt(0);
            }
        }

        private SkillAction? InRangeAct(float distance)
        {
            var useWeapon = -1;
            foreach (var tuple in RangeToWeapon.Where(tuple => tuple.Item1 >= distance))
            {
                useWeapon = tuple.Item2;
                break;
            }

            if (useWeapon < 0) return null;
            if (useWeapon != NowWeapon)
            {
                return SkillAction.Switch;
            }

            if (!SkillToTrickRange.TryGetValue(useWeapon, out var tuples)) return null;
            var valueTuples = tuples.Where(x => x.Item1 >= distance).Select(x => x.Item2).ToList();
            if (valueTuples.Count <= 0) return null;
            var next = Random.Next(valueTuples.Count);
            var skillAction = valueTuples[next];
            return skillAction;
        }
    }

    public class BotSkillCtrl
    {
        public BotSkillCtrl(IEnumerable<(int weight, SkillAction skillAction)> stackWeightToSkillActions,
            int noActWeight,
            int doNotMaxTick,
            int doNotMinTick, int showDelayMax)
        {
            var (weightOverList, i) = stackWeightToSkillActions.GetWeightOverList();
            StackWeightToSkillActions = weightOverList;
            Total = noActWeight + i;
            DoNotMaxTick = doNotMaxTick;
            DoNotMinTick = doNotMinTick;
            NowThinkAction = null;
            ShowDelayNow = 0;
            ShowDelayMax = showDelayMax;
            DoNotRestTick = 0;
        }

        private ImmutableArray<(int, SkillAction)> StackWeightToSkillActions { get; }

        private int Total { get; }

        public int DoNotRestTick { get; set; }
        private int DoNotMaxTick { get; }
        private int DoNotMinTick { get; }
        public SkillAction? NowThinkAction { get; set; }
        private int ShowDelayNow { get; set; }
        private int ShowDelayMax { get; }

        private SkillAction? GetAction()
        {
            return NowThinkAction;//todo
        }

        private void ThinkAAct(Random random)
        {
            var next = random.Next(Total);
        }

        private bool CanShowThinkAction()
        {
            return ShowDelayNow > ShowDelayMax;
        }

        private void GoATick()
        {
        }
    }

    public class Patrol
    {
        public TwoDPoint[] Points { get; }
        public int NowToPt { get; set; }

        public Patrol(List<TwoDPoint> rawPoints)
        {
            var twoDPoints = rawPoints.GetRange(1, rawPoints.Count - 2);
            twoDPoints.Reverse();
            rawPoints.AddRange(twoDPoints);
            Points = rawPoints.ToArray();
            NowToPt = 0;
        }

        public TwoDPoint GetNowPt()
        {
            return Points[NowToPt];
        }

        public TwoDPoint NextPt()
        {
            NowToPt = (1 + NowToPt) % Points.Length;
            return Points[NowToPt];
        }
    }
}