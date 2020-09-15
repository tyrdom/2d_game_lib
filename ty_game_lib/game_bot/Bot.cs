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
        public int MyGid;
        public Dictionary<int, ImmutableArray<(float, SkillAction)>> SkillToTrickRange;
        public int[] MyTeamGid { get; }
        public int[] OtherTeamGid { get; }
        public List<(float, int)> RangeToWeapon;
        public BotRadio BotRadio;
        public PathTop NaviMap { get; }
        private int? MyPoly;
        public int NowWeapon;
        private Queue<TwoDPoint> _pathPoints;
        private Random _random;

        public Bot(Dictionary<int, ImmutableArray<(float, SkillAction)>> skillToTrickRange, BotRadio botRadio,
            List<(float, int)> rangeToWeapon, PathTop naviMap, int[] myTeamGid, int[] otherTeamGid, int myGid,
            Random random)
        {
            SkillToTrickRange = skillToTrickRange;
            BotRadio = botRadio;
            RangeToWeapon = rangeToWeapon;
            NaviMap = naviMap;
            MyTeamGid = myTeamGid;
            OtherTeamGid = otherTeamGid;
            MyGid = myGid;
            _random = random;
            NowWeapon = 0;
            _pathPoints = new Queue<TwoDPoint>();
            MyPoly = null;
        }

        public Bot(CharInitMsg charInitMsg, BotRadio teamRadio, PathTop naviMap, int[] myTeamGid,
            int[] otherTeamGid, int myGid, Random random)
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
            BotRadio = teamRadio;
            NaviMap = naviMap;
            MyTeamGid = myTeamGid;
            OtherTeamGid = otherTeamGid;
            MyGid = myGid;
            _random = random;
            _pathPoints = new Queue<TwoDPoint>();
            RangeToWeapon = valueTuples;
            NowWeapon = 0;
            MyPoly = null;
        }

        Operate? BotSimpleAct(IEnumerable<CharTickMsg> charTickMsgs)
        {
            //

            var tickMsgs = charTickMsgs as CharTickMsg[] ?? charTickMsgs.ToArray();
            var enemy = tickMsgs.FirstOrDefault(x => OtherTeamGid.Contains(x.Gid));
            var myMsg = tickMsgs.FirstOrDefault(x => x.Gid == MyGid);
            if (myMsg == null) return null;
            {
                var b = myMsg.SkillLaunch == SkillAction.Switch;
                if (b)
                {
                    NowWeapon = (NowWeapon + 1) % RangeToWeapon.Count;
                }
            }
            if (enemy == null)
            {
                MyPoly = NaviMap.InWhichPoly(myMsg.Pos);
                return null;
            }

            {
                var distance = enemy.Pos.GetDistance(myMsg.Pos);
                var inRangeAct = InRangeAct(distance);

                if (inRangeAct != null)
                {
                    var twoDVector = new TwoDVector(myMsg.Pos, enemy.Pos);
                    var dVector = twoDVector.GetUnit2();
                    var operate = new Operate(dVector, inRangeAct, null);
                    return operate;
                }
            }
            //
            return null;
        }

        TwoDVector? FindAMoveDir(TwoDPoint myPos, TwoDPoint objPos, int? thisPoly, int? objPoly)
        {
            var firstOrDefault = _pathPoints.FirstOrDefault();
            if (firstOrDefault != null)
            {
                if (myPos.GetDistance(firstOrDefault) < 0.5f)
                {
                    _pathPoints.Dequeue();
                    var twoDPoint = _pathPoints.FirstOrDefault();
                    if (twoDPoint != null)
                    {
                        return new TwoDVector(myPos, twoDPoint).GetUnit2();
                    }
                }
            }

            var findAPathByPoint = NaviMap.FindAPathByPoint(myPos, objPos, thisPoly, objPoly);
            var twoDPoints = PathTop.GetGoPts(myPos, objPos, findAPathByPoint.Select(x => x.Item2).ToList());
            var pathPoints  = _pathPoints.Concat(twoDPoints);
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
            var next = _random.Next(valueTuples.Count);
            var skillAction = valueTuples[next];
            return skillAction;
        }

        void ListenRadio()
        {
        }

        void BroadcastRadio(IEnemyMsg enemyMsg)
        {
            BotRadio.OnEnemyEvent(enemyMsg);
        }
    }
}