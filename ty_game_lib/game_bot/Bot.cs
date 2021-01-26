using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using collision_and_rigid;
using cov_path_navi;
using game_stuff;

namespace game_bot
{
    public class Bot
    {
        public CharacterBody BotBody { get; }
        public Dictionary<int, ImmutableArray<(float, SkillAction)>> SkillToTrickRange { get; }
        public int[] MyTeamGid { get; }
        public int[] OtherTeamGid { get; }
        public List<(float, int)> RangeToWeapon;

        public float PropRange { get; }
        public PathTop NaviMap { get; }
        private int? MyPoly { get; set; }
        public int NowWeapon { get; set; }
        private List<TwoDPoint> PathPoints { get; }
        private Random Random { get; }


        // private Bot(Dictionary<int, ImmutableArray<(float, SkillAction)>> skillToTrickRange, BotRadio botRadio,
        //     List<(float, int)> rangeToWeapon, PathTop naviMap, int[] myTeamGid, int[] otherTeamGid, int myGid,
        //     Random random)
        // {
        //     SkillToTrickRange = skillToTrickRange;
        //     BotRadio = botRadio;
        //     RangeToWeapon = rangeToWeapon;
        //     NaviMap = naviMap;
        //     MyTeamGid = myTeamGid;
        //     OtherTeamGid = otherTeamGid;
        //     MyGid = myGid;
        //     Random = random;
        //     NowWeapon = 0;
        //     PathPoints = new List<TwoDPoint>();
        //     MyPoly = null;
        //     EnemySavedMsg = null;
        //     RadioListenOn = true;
        // }

        public Bot(CharInitMsg charInitMsg, PathTop naviMap, int[] myTeamGid,
            int[] otherTeamGid, CharacterBody botBody, Random random)
        {
            var valueTuples = new List<(float, int)>();
            var dictionary = new Dictionary<int, ImmutableArray<(float, SkillAction)>>();
            foreach (var keyValuePair in charInitMsg.WeaponConfigs)
            {
                var max = keyValuePair.Value.Ranges.Select(x => x.Item1).Max();
                valueTuples.Add((max, keyValuePair.Key));
                dictionary[keyValuePair.Key] = keyValuePair.Value.Ranges;
            }

            valueTuples.Sort((x, y) => -x.Item1.CompareTo(y.Item1));

            SkillToTrickRange = dictionary;
            NaviMap = naviMap;
            MyTeamGid = myTeamGid;
            OtherTeamGid = otherTeamGid;
            BotBody = botBody;
            Random = random;
            PathPoints = new List<TwoDPoint>();
            RangeToWeapon = valueTuples;
            NowWeapon = 0;
            MyPoly = null;
        }


        public Operate? BotSimpleTick(IRelationMsg relationMsg, ISeeTickMsg seeTickMsg)
        {
            MyPoly = NaviMap.InWhichPoly(BotBody.GetAnchor());


            return null;
        }

        private TwoDVector? GoPath(TwoDPoint myPos)
        {
            var firstOrDefault = PathPoints.FirstOrDefault();
            if (firstOrDefault == null) return null;
            if (myPos.GetDistance(firstOrDefault) < 0.5f)
            {
                PathPoints.RemoveAt(0);
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
}