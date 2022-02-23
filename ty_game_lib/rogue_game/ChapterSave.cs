using System;
using System.Collections.Generic;
using game_config;
using rogue_chapter_maker;

namespace rogue_game
{
    [Serializable]
    public class ChapterSave
    {
        public ChapterSave(PlayGroundSaveData[] playGroundSaveDataS, Dictionary<PointMap, int> mapOnYSlotArray,
            int[] clearMaps, int chapterId)
        {
            PlayGroundSaveDataS = playGroundSaveDataS;
            MapOnYSlotArray = mapOnYSlotArray;
            ClearMapIds = clearMaps;
            ChapterId = chapterId;
        }

        private PlayGroundSaveData[] PlayGroundSaveDataS { get; }

        private Dictionary<PointMap, int> MapOnYSlotArray { get; }

        private int[] ClearMapIds { get; }

        private int ChapterId { get; }


        public Chapter Load(Random random)
        {
            var configsRogueGameChapter = CommonConfig.Configs.rogue_game_chapters[ChapterId];
            var genBySave = Chapter.GenBySave(MapOnYSlotArray, configsRogueGameChapter, random, PlayGroundSaveDataS,
                ClearMapIds);
            return genBySave;
        }
    }
}