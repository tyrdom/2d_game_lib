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
        public BotRadio BotRadio { get; }
        public PathTop NaviMap { get; }
        private int? MyPoly { get; set; }
        public int NowWeapon { get; set; }
        private List<TwoDPoint> PathPoints { get; }
        private Random Random { get; }
        private TwoDPoint NowPos { get; set; }
        private IEnemyMsg? EnemySavedMsg { get; set; }

        private bool RadioListenOn { get; set; }

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
            NowPos = charInitMsg.Pos;
            SkillToTrickRange = dictionary;
            BotRadio = teamRadio;
            NaviMap = naviMap;
            MyTeamGid = myTeamGid;
            OtherTeamGid = otherTeamGid;
            MyGid = myGid;
            Random = random;
            PathPoints = new List<TwoDPoint>();
            RangeToWeapon = valueTuples;
            NowWeapon = 0;
            MyPoly = null;
            EnemySavedMsg = null;
            RadioListenOn = true;
            BotRadio.EnemyEvent += ListenRadio;
        }

        private void SaveEMsg(CharTickMsg charTickMsg)
        {
            var twoDPoint = charTickMsg.Pos;
            var inWhichPoly = NaviMap.InWhichPoly(twoDPoint);
            if (inWhichPoly == null)
            {
                return;
            }

            var enemyFound = new EnemyFound(inWhichPoly, twoDPoint);
            EnemySavedMsg = enemyFound;
        }

        public Operate? BotSimpleTick(IEnumerable<CharTickMsg> charTickMsgs)
        {
            //
            var tickMsgs = charTickMsgs.ToList();

            var enemy = tickMsgs.FirstOrDefault(x => OtherTeamGid.Contains(x.Gid));
            var myMsg = tickMsgs.FirstOrDefault(x => x.Gid == MyGid);
            if (myMsg == null) return null;
            //update status
            NowPos = myMsg.Pos;
            var b = myMsg.SkillLaunch == SkillAction.Switch;
            if (b)
            {
                NowWeapon = (NowWeapon + 1) % RangeToWeapon.Count;
            }

            if (enemy == null)
            {
                MyPoly = NaviMap.InWhichPoly(myMsg.Pos);
                var twoDVector = GoPath(myMsg.Pos);
                if (twoDVector != null)
                {
                    RadioListenOn = false;
                    return new Operate(null, null, twoDVector);
                }

                RadioListenOn = true;
                return null;
            }

            RadioListenOn = false;
            SaveEMsg(enemy);
            var distance = enemy.Pos.GetDistance(myMsg.Pos);
            var inRangeAct = InRangeAct(distance);

            if (inRangeAct != null)
            {
                var twoDVector = new TwoDVector(myMsg.Pos, enemy.Pos);
                var dVector = twoDVector.GetUnit2();
                var operate = new Operate(dVector, inRangeAct, null);
                return operate;
            }

            var twoDVector2 = new TwoDVector(myMsg.Pos, enemy.Pos);
            PathPoints.Clear();
            PathPoints.Add(enemy.Pos);
            var operate2 = new Operate(null, null, twoDVector2);
            return operate2;
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

        private void ListenRadio(IEnemyMsg charTickMsg)
        {
            if (!RadioListenOn)
            {
                return;
            }

            var findAPathByPoint = NaviMap.FindAPathByPoint(NowPos, charTickMsg.Pos, MyPoly, charTickMsg.NowPolyId);
            var twoDPoints = PathTop.GetGoPts(NowPos, charTickMsg.Pos, findAPathByPoint.Select(x => x.Item2).ToList());
            PathPoints.Clear();
            PathPoints.AddRange(twoDPoints);
            RadioListenOn = false;
        }

        public void BroadcastRadio()
        {
            if (EnemySavedMsg != null) BotRadio.OnEnemyEvent(EnemySavedMsg);
        }
    }
}