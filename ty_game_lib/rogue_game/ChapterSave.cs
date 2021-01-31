using System.Linq;
using rogue_chapter_maker;

namespace rogue_game
{
    public class ChapterSave
    {
        private MapSaveCell[] MapSaveCells { get; }


        public void GenChapter()
        {
            // var pointMaps =
            //     MapSaveCells.ToDictionary(x => x.GetSlot(),
            //         x => (x.GetLinkSlots(), x.GenRawPointMap(), x.IsMapClear, x.MapResId));
            //
            // foreach (var (valueTuples, pointMap, _, _) in pointMaps.Values)
            // {
            //     var enumerable = valueTuples.Select(x => new Link(x.Key, pointMaps[x.Value].Item2));
            //
            //     pointMap.Links.AddRange(enumerable);
            // }
            
            
        }
    }
}