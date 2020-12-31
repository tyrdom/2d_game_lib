using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    public class GameSave
    {
        private Dictionary<int, PlayerSave> PlayerSaves { get; }

        private MapSave MapSave { get; }

        private int ChapterId { get; }
    }

    internal class MapSave
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

    internal readonly struct MapSaveCell
    {
        public bool IsMapClear { get; }
        private int X { get; }
        private int Y { get; }
        public int MapResId { get; }
        private MapType Type { get; }
        private bool N { get; }
        private bool S { get; }
        private bool E { get; }
        private bool W { get; }

        public (int, int) GetSlot()
        {
            return (X, Y);
        }

   
        public MapSaveCell(bool isMapClear, int x, int y, int mapResId, MapType type, bool n, bool s, bool e, bool w)
        {
            IsMapClear = isMapClear;
            X = x;
            Y = y;
            MapResId = mapResId;
            Type = type;
            N = n;
            S = s;
            E = e;
            W = w;
        }

        public Dictionary<Side, (int, int)> GetLinkSlots()
        {
            var valueTuples = new Dictionary<Side, (int, int)>();

            if (E)
            {
                valueTuples[Side.East] = (X + 1, Y);
            }


            if (W)
            {
                valueTuples[Side.West] = (X - 1, Y);
            }

            if (N)
            {
                valueTuples[Side.North] = (X, Y + 1);
            }

            if (S)
            {
                valueTuples[Side.South] = (X, Y - 1);
            }


            return valueTuples;
        }

        public PointMap GenRawPointMap()
        {
            var pointMap = new PointMap((X, Y), Type, new List<Link>());
            return pointMap;
        }
    }
}