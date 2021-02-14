using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    public class PveMap
    {
        public PveMap(PlayGround playGround, HashSet<BattleNpc> bosses, HashSet<BattleNpc> creeps,
            PveWinCond pveWinCond, bool isClear, int[] battleNpcToSpawn, int[] bossToSpawn)
        {
            PlayGround = playGround;
            Bosses = bosses;
            Creeps = creeps;
            PveWinCond = pveWinCond;
            IsClear = isClear;
            BattleNpcToSpawn = battleNpcToSpawn;
            BossToSpawn = bossToSpawn;
        }

        private static (PveWinCond winCond, bool isClear) GetWinCond(MapType mapType)
        {
            return mapType switch
            {
                MapType.BigStart => (PveWinCond.AllClear, true),
                MapType.BigEnd => (PveWinCond.BossClear, false),
                MapType.Small => (PveWinCond.AllClear, false),
                MapType.Big => (PveWinCond.AllClear, false),
                MapType.SmallStart => (PveWinCond.AllClear, true),
                MapType.SmallEnd => (PveWinCond.BossClear, false),
                MapType.Vendor => (PveWinCond.AllClear, true),
                MapType.Hangar => (PveWinCond.AllClear, true),
                _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
            };
        }

        public static PveMap GenEmptyPveMap(PointMap pointMap, int resId, int gmMid, int[] bNpc, int[] boss)
        {
            var (winCond, isClear) = GetWinCond(pointMap.MapType);
            var genEmptyPlayGround = PlayGround.GenEmptyPlayGround(resId, gmMid);
            var pveMap = new PveMap(genEmptyPlayGround, new HashSet<BattleNpc>(), new HashSet<BattleNpc>(), winCond,
                isClear, bNpc, boss);
            return pveMap;
        }

        public bool IsClear { get; private set; }
        public PlayGround PlayGround { get; }
        public HashSet<BattleNpc> Bosses { get; set; }
        public HashSet<BattleNpc> Creeps { get; set; }
        public PveWinCond PveWinCond { get; }

        public int[] BattleNpcToSpawn { get; }

        public int[] BossToSpawn { get; }

        public IEnumerable<BattleNpc> GenBattleNpc(int[] ints, Random random)
        {
            return Enumerable.Range(0, ints.Length)
                .Select(x => BattleNpc.GenById(ints[x], PlayGround.MgId * 100 + x, 2, random));
        }

        public void SpawnNpc(Random random)
        {
        }

        public void KillCreep(ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> playerBeHit)
        {
            bool Predicate(BattleNpc creep)
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
            PlayGround.AddBodies(characterBodies);
        }

        public void TeleportToThisMap(IEnumerable<(CharacterBody, TwoDPoint)> characterBodiesToPt)
        {
            PlayGround.AddBodies(characterBodiesToPt);
        }
    }
}