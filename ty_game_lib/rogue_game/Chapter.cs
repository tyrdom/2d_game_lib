using System;
using System.Collections.Generic;

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

        public static Chapter GenById(int chapterId)
        {
            throw new System.NotImplementedException();
        }
    }
}