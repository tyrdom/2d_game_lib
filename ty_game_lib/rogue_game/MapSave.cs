using System.Linq;
using rogue_chapter_maker;

namespace rogue_game
{
    public class MapSave
    {
        private MapSaveCell[] MapSaveCells { get; }


        public void GenChapter()
        {
            var pointMaps =
                MapSaveCells.ToDictionary(x => x.GetSlot(),
                    x => (x.GetLinkSlots(), x.GenRawPointMap(), x.IsMapClear, x.MapResId));

            foreach (var mapsValue in pointMaps.Values)
            {
                var enumerable = mapsValue.Item1.Select(x => new Link(x.Key, pointMaps[x.Value].Item2));

                mapsValue.Item2.Links.AddRange(enumerable);
            }
        }
    }
}