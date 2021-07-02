using System;
using System.Collections.Generic;
using System.Linq;
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


        public static ChapterSave Save(Chapter chapter)
        {
            var chapterMapOnYSlotArray = chapter.MapOnYSlotArray;
            var enumerable = chapter.MGidToMap.Where(x => x.Value.IsClear && x.Value.IsReached).Select(x => x.Key)
                .ToArray();
            var ints = chapterMapOnYSlotArray
                .Where(x => x.Key.MapType == MapType.Vendor || x.Key.MapType == MapType.Hangar).Select(x => x.Value);

            var pveMaps = ints.Select(x => chapter.MGidToMap[x]);
            var playGroundSaveDataS = pveMaps.Select(x => PlayGroundSaveData.GroundSaveData(x.PlayGround)).ToArray();

            var chapterSave =
                new ChapterSave(playGroundSaveDataS, chapterMapOnYSlotArray, enumerable, chapter.ChapterId);
            return chapterSave;
        }

        public Chapter Load(Random random)
        {
            var configsRogueGameChapter = CommonConfig.Configs.rogue_game_chapters[ChapterId];
            var genBySave = Chapter.GenBySave(MapOnYSlotArray, configsRogueGameChapter, random, PlayGroundSaveDataS,
                ClearMapIds);
            return genBySave;
        }
    }
}