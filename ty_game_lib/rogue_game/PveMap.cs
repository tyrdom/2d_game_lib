using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_bot;
using game_config;
using game_stuff;
using rogue_chapter_maker;
using Random = System.Random;

namespace rogue_game
{
    [Serializable]
    public class PveMap
    {
        private PveMap(PlayGround playGround, HashSet<BattleNpc> bosses, HashSet<BattleNpc> creeps,
            PveWinCond pveWinCond, bool isClear, int[] creepIdToSpawn, int[] bossIdToSpawn, bool isReached = false)
        {
            PlayGround = playGround;
            Bosses = bosses;
            Creeps = creeps;
            PveWinCond = pveWinCond;
            IsClear = isClear;
            CreepIdToSpawn = creepIdToSpawn;
            BossIdToSpawn = bossIdToSpawn;
            IsReached = isReached;
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

        public static PveMap GenEmptyPveMap(PointMap pointMap, int resId, int gmMid, int[] bNpc, int[] boss,
            IEnumerable<ISaleUnit>? saleUnits = null, bool forceClear = false)
        {
            var (winCond, isClear) = GetWinCond(pointMap.MapType);
            var isC = forceClear || isClear;
            var b = pointMap.MapType == MapType.BigStart || pointMap.MapType == MapType.SmallStart;
            var clear = b || forceClear;
            var genEmptyPlayGround = PlayGround.GenEmptyPlayGround(resId, gmMid);

            if (pointMap.MapType == MapType.Vendor && saleUnits != null)
            {
                genEmptyPlayGround.AddSaleUnitsToMap(saleUnits, 0);
            }


            var pveMap = new PveMap(genEmptyPlayGround, new HashSet<BattleNpc>(), new HashSet<BattleNpc>(), winCond,
                isC, bNpc, boss, clear);
            return pveMap;
        }

        public bool IsReached { get; set; }
        public bool IsClear { get; private set; }
        public PlayGround PlayGround { get; }
        public HashSet<BattleNpc> Bosses { get; private set; }
        public HashSet<BattleNpc> Creeps { get; private set; }
        private PveWinCond PveWinCond { get; }
        private int[] CreepIdToSpawn { get; }
        private int[] BossIdToSpawn { get; }


        private HashSet<(SimpleBot simpleBot, BattleNpc battleNpc)> GenBattleNpcWithBot(IReadOnlyList<int> ints,
            Random random,
            BotTeam botTeam, bool isBoss, int nowChapterExtraPassiveNum)
        {
            var genBattleNpcAndBot = Enumerable.Range(0, ints.Count)
                .Select(x =>
                {
                    var (battleNpc, boId) = BattleNpc.GenById(ints[x],
                        (1 + PlayGround.MgId) * 1000 + (isBoss ? 100 : 0) + x, 100,
                        random, nowChapterExtraPassiveNum);
                    var simpleBot = SimpleBot.GenById(boId, battleNpc.CharacterBody, random,
                        botTeam.GetNaviMap(battleNpc.CharacterBody.GetSize()));


                    return (simpleBot, battleNpc);
                }).IeToHashSet();
            return genBattleNpcAndBot;
        }

        public void SpawnNpcWithBot(Random random, BotTeam botTeam, int nowChapterExtraPassiveNum)
        {
            var genBattleNpc = GenBattleNpcWithBot(CreepIdToSpawn, random, botTeam, false, nowChapterExtraPassiveNum);
            Creeps = genBattleNpc.Select(x => x.Item2).IeToHashSet();
            var battleNpc = GenBattleNpcWithBot(BossIdToSpawn, random, botTeam, true, nowChapterExtraPassiveNum);
            Bosses = battleNpc.Select(x => x.Item2).IeToHashSet();
            var valueTuples = genBattleNpc.Union(battleNpc)
                .Select(x => (x.simpleBot, x.battleNpc.CharacterBody,
                    x.simpleBot.GetStartPt() ?? PlayGround.GetEntrancePoint())).IeToHashSet();
            var battleNpcS = valueTuples.Select(x => x.simpleBot);
            botTeam.SetBots(battleNpcS);
            TeleportToThisMap(valueTuples.Select(x => (x.CharacterBody, x.Item3)));
        }

        public (ImmutableList<GameItem> all, ImmutableDictionary<int, ImmutableList<GameItem>> kill,
            ImmutableList<IMapInteractable> mapInteractables) KillCreep(
                ImmutableDictionary<int, ImmutableHashSet<IRelationMsg>> charBeHit)
        {
            var all = new List<GameItem>();
            var kill = new List<(int, GameItem[])>();
            var mapInteractableS = new List<IMapInteractable>();

            bool Predicate(BattleNpc creep)
            {
                var getValue = charBeHit.TryGetValue(creep.CharacterBody.GetId(), out var relationMsgSet);
                if (!getValue) return false;
                var damageMsg = relationMsgSet.OfType<IDamageMsg>().FirstOrDefault(x => x.DmgShow.IsKill);
                if (damageMsg == null) return false;
                var gId = damageMsg.CasterOrOwner.GId;
                var creepWantedBonus = creep.WantedBonus;
                all.AddRange(creepWantedBonus.AllBonus);
                kill.Add((gId, creepWantedBonus.KillBonus));
                var mapInteractableDrop = creepWantedBonus.MapInteractableDrop;
                if (mapInteractableDrop == null) return true;
                mapInteractableDrop.ReLocate(creep.CharacterBody.GetAnchor());
                mapInteractableS.Add(mapInteractableDrop);

                return true;
            }


            var immutableDictionary = kill.GroupBy(x => x.Item1).ToImmutableDictionary(p => p.Key,
                p => p.SelectMany(x => x.Item2).ToImmutableList());
            switch (PveWinCond)
            {
                case PveWinCond.AllClear:
                    var removeWhere = Bosses.RemoveWhere(Predicate);
                    var i = Creeps.RemoveWhere(Predicate);
                    if (removeWhere > 0 || i > 0)
                    {
                        IsClear = !(Bosses.Any() || Creeps.Any());
                    }

                    return (all.ToImmutableList(), immutableDictionary, mapInteractableS.ToImmutableList());
                case PveWinCond.BossClear:
                    var ii = Bosses.RemoveWhere(Predicate);
                    if (ii > 0)
                    {
                        IsClear = !Bosses.Any();
                    }

                    return (all.ToImmutableList(), immutableDictionary, mapInteractableS.ToImmutableList());
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
            PlayGround.AddBodiesToStart(characterBodies);
        }

        public void TelePortOut()
        {
            PlayGround.RemoveAllBodies();
        }


        public void TeleportToThisMap(IEnumerable<(CharacterBody, TwoDPoint)> characterBodiesToPt)
        {
            PlayGround.AddBodies(characterBodiesToPt);
        }
#if DEBUG
        public void ForceClear()
        {
            IsClear = true;
        }
#endif
        public bool NeedSave()
        {
            return CreepIdToSpawn.Any() || BossIdToSpawn.Any();
        }
    }
}