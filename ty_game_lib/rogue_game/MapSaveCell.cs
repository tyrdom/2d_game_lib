using System.Collections.Generic;
using rogue_chapter_maker;

namespace rogue_game
{
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