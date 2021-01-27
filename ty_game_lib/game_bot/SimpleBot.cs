using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using collision_and_rigid;
using game_config;
using game_stuff;

namespace game_bot
{
    public enum BotStatus
    {
        OnPatrol,
        GoPath,
        Engage
    }

    public class SimpleBot
    {
        public CharacterBody BotBody { get; }
        public Dictionary<int, ImmutableArray<(float, SkillAction)>> SkillToTrickRange { get; set; }

        public Patrol Patrol { get; }

        private BotStatus BotStatus { get; set; }
        public List<(float, int)> RangeToWeapon { get; set; }


        private int? MyPoly { get; set; }
        public int NowWeapon { get; set; }
        private Queue<TwoDPoint> PathPoints { get; }
        private Random Random { get; }


        public SimpleBot(CharacterBody botBody, Random random, List<TwoDPoint> patrolPts)
        {
            var valueTuples = new List<(float, int)>();
            var dictionary = new Dictionary<int, ImmutableArray<(float, SkillAction)>>();
            foreach (var keyValuePair in botBody.CharacterStatus.Weapons)
            {
                var weapon = keyValuePair.Value;
                var max = weapon.BotRanges.Select(x => x.Item1).Max();
                valueTuples.Add((max, keyValuePair.Key));
                dictionary[keyValuePair.Key] = weapon.BotRanges;
            }

            valueTuples.Sort((x, y) => -x.Item1.CompareTo(y.Item1));

            SkillToTrickRange = dictionary;
            BotBody = botBody;
            Random = random;
            BotStatus = BotStatus.OnPatrol;
            Patrol = new Patrol(patrolPts);
            PathPoints = new Queue<TwoDPoint>();
            RangeToWeapon = valueTuples;
            NowWeapon = 0;
            MyPoly = null;
        }


        public Operate? BotSimpleTick(IPerceivable perceivable)
        {
            switch (BotStatus)
            {
                case BotStatus.OnPatrol:
                    DoPatrol();
                    CheckSee();
                    break;
                case BotStatus.GoPath:
                    CheckSee();
                    CheckRange();
                    break;
                case BotStatus.Engage:
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private void CheckRange()
        {
            throw new NotImplementedException();
        }

        private void CheckSee()
        {
            throw new NotImplementedException();
        }

        private void DoPatrol()
        {
            throw new NotImplementedException();
        }

        private TwoDVector? GoPath(TwoDPoint myPos)
        {
            var firstOrDefault = PathPoints.FirstOrDefault();
            if (firstOrDefault == null) return null;
            if (myPos.GetDistance(firstOrDefault) < 0.5f)
            {
                var twoDPoint = PathPoints.FirstOrDefault();
                if (twoDPoint != null)
                {
                    return new TwoDVector(myPos, twoDPoint).GetUnit2();
                }
            }
            else
            {
                return new TwoDVector(myPos, firstOrDefault).GetUnit2();
            }

            return null;
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