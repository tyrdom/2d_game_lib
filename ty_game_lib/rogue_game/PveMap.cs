﻿using System;
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
            PveWinCond pveWinCond, bool isClear)
        {
            PlayGround = playGround;
            Bosses = bosses;
            Creeps = creeps;
            PveWinCond = pveWinCond;
            IsClear = isClear;
        }

        public bool IsClear { get; private set; }
        private PlayGround PlayGround { get; }
        public HashSet<Creep> Bosses { get; }
        public HashSet<Creep> Creeps { get; }
        public PveWinCond PveWinCond { get; }


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

        public void AddCharacterBodiesToStart(IEnumerable<CharacterBody> characterBodies)
        {
            PlayGround.AddBodiesAtStartPts(characterBodies);
        }

        public void TeleportToThisMap(IEnumerable<(CharacterBody, TwoDPoint)> characterBodiesToPt)
        {
            PlayGround.TeleportBodiesToPts(characterBodiesToPt);
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