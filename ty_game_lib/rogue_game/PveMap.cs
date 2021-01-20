using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_stuff;

namespace rogue_game
{
    public class PveMap
    {
        public PveMap(PlayGround playGround, HashSet<Creep> bosses, HashSet<Creep> creeps,
            PveWinCond pveWinCond, TwoDPoint[] enterPoints, bool isClear)
        {
            PlayGround = playGround;
            Bosses = bosses;
            Creeps = creeps;
            PveWinCond = pveWinCond;
            EnterPoints = enterPoints;
            IsClear = isClear;
        }

        public bool IsClear { get; set; }
        private PlayGround PlayGround { get; }
        public HashSet<Creep> Bosses { get; }
        public HashSet<Creep> Creeps { get; }
        public PveWinCond PveWinCond { get; }

        public TwoDPoint[] EnterPoints { get; }

        public void ActiveApplyDevice()
        {
            PlayGround.ActiveApplyDevice();
        }

        public bool IsClearAndSave()
        {
            if (IsClear)
            {
                return true;
            }

            var b = PveWinCond switch
            {
                PveWinCond.AllClear => Bosses.All(x => x.CharacterBody.CharacterStatus.SurvivalStatus.IsDead()) &&
                                       Creeps.All(x => x.CharacterBody.CharacterStatus.SurvivalStatus.IsDead()),
                PveWinCond.BossClear => Bosses.All(x => x.CharacterBody.CharacterStatus.SurvivalStatus.IsDead()),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (!b) return false;
            IsClear = b;
            return true;
        }

        public PlayGroundGoTickResult PlayGroundGoATick(
            Dictionary<int, Operate> valuePairs)
        {
            return PlayGround.PlayGroundGoATick(valuePairs);
        }

        public void AddPlayers(CharacterBody[] characterBodies)
        {
            var valueTuples = new List<(CharacterBody, TwoDPoint)>();
            for (var i = 0; i < characterBodies.Length; i++)
            {
                var twoDPoint = EnterPoints[i % EnterPoints.Length];
                valueTuples.Add((characterBodies[i], twoDPoint));
            }

            PlayGround.AddBodies(valueTuples);
        }

        public int GetMId()
        {
            return PlayGround.MgId;
        }
    }

    public class Creep
    {
        public CharacterBody CharacterBody { get; }
        public WantedBonus WantedBonus { get; }
    }

    public class WantedBonus
    {
    }
}