using System;
using System.Collections.Generic;
using System.Linq;
using rogue_chapter_maker;

namespace rogue_game
{
    [Serializable]
    public class Chapter
    {
        public Dictionary<int, PveMap> MGidToMap { get; }
        public PveMap Entrance { get; }
        public PveMap Finish { get; }

        public Chapter(Dictionary<int, PveMap> mGidToMap, PveMap entrance, PveMap finish)
        {
            MGidToMap = mGidToMap;
            Entrance = entrance;
            Finish = finish;
        }

        public bool IsPass()
        {
            return Finish.IsClear;
        }

        public static Chapter GenById(int id)
        {
            throw new System.NotImplementedException();
        }

        public static Chapter GenByTop(ChapterMapTop chapterMapTop)
        {
            var dictionary = new Dictionary<PointMap, int>();
            var pointMaps = chapterMapTop.PointMaps.ToList();
            for (var i = 0; i < pointMaps.Count; i++)
            {
                dictionary[pointMaps[i]] = i;
            }

            // var pveMaps = pointMaps.Select(x=>new PveMap(x));
            
            


            throw new System.NotImplementedException();
        }
    }
}