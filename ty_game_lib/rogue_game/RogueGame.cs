using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using game_stuff;

namespace rogue_game
{
    public class RogueGame
    {
        private Queue<Chapter> Chapters { get; }
        public Dictionary<int, (CharacterBody Player, bool Dead)> NowGamePlayers { get; }
        public int CountDownTick { get; set; }
        public PveWinCond PveWinCond { get; }


        private static ImmutableDictionary<int, LevelUpsData> RogueLevelData()
        {
            var levelUpsData = new LevelUpsData(null, LocalConfig.RogueRebornTick, LocalConfig.RogueRebornCost,
                new int[] { });
            var levelUpsDates = new Dictionary<int, LevelUpsData> {{1, levelUpsData}};
            return levelUpsDates.ToImmutableDictionary();
        }

        public LevelUps GetLevelUp()
        {
            return new LevelUps(RogueLevelData());
        }

        public void CheckPve(PveWinCond pveWinCond,
            HashSet<CharacterBody> deadPool)
        {
        }
    }
}