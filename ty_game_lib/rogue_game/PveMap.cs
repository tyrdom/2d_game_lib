using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public bool IsClear { get; private set; }
        private PlayGround PlayGround { get; }
        public HashSet<Creep> Bosses { get; }
        public HashSet<Creep> Creeps { get; }
        public PveWinCond PveWinCond { get; }

        public TwoDPoint[] EnterPoints { get; }

        public void ActiveApplyDevice()
        {
            PlayGround.ActiveApplyDevice();
        }

        public void KillCreep(ImmutableDictionary<int, ImmutableHashSet<HitResult>> playerBeHit)
        {
            bool Predicate(Creep creep)
            {
                return playerBeHit.ContainsKey(creep.CharacterBody.GetId()) &&
                       creep.CharacterBody.CharacterStatus.SurvivalStatus.IsDead();
            }

            switch (PveWinCond)
            {
                case PveWinCond.AllClear:
                    var removeWhere = Bosses.RemoveWhere(Predicate);
                    var i = Creeps.RemoveWhere(Predicate);
                    if (removeWhere > 0 || i > 0)
                    {
                        IsClear = !(Bosses.Any() || Creeps.Any());
                    }


                    return;
                case PveWinCond.BossClear:
                    var ii = Bosses.RemoveWhere(Predicate);
                    if (ii > 0)
                    {
                        IsClear = !Bosses.Any();
                    }

                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public PlayGroundGoTickResult PlayGroundGoATick(
            Dictionary<int, Operate> valuePairs)
        {
            var playGroundGoTickResult = PlayGround.PlayGroundGoATick(valuePairs);


            return playGroundGoTickResult;
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