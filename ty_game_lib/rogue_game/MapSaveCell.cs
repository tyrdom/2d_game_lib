using System;
using System.Collections.Generic;
using System.Linq;
using rogue_chapter_maker;

namespace rogue_game
{
    [Serializable]
    internal readonly struct MapSaveCell
    {
        public bool IsMapClear { get; }
        private int X { get; }
        private int Y { get; }
        private int MapId { get; }
        public int MapResId { get; }
        private MapType MapType { get; }
        private bool N { get; }
        private bool S { get; }
        private bool E { get; }
        private bool W { get; }

        public (int, int) GetSlot()
        {
            return (X, Y);
        }


        public MapSaveCell(bool isMapClear, int x, int y, int mapResId, MapType mapType, bool n, bool s, bool e, bool w,
            int mapId)
        {
            IsMapClear = isMapClear;
            X = x;
            Y = y;
            MapResId = mapResId;
            MapType = mapType;
            N = n;
            S = s;
            E = e;
            W = w;
            MapId = mapId;
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
            var pointMap = new PointMap((X, Y), MapType, new List<Link>());
            return pointMap;
        }


        public MapSaveCell(PointMap pointMap, int resId, int gid)
        {
            X = pointMap.Slot.x;
            Y = pointMap.Slot.y;
            IsMapClear = false;
            MapType = pointMap.MapType;
            E = pointMap.Links.Any(x => x.Side == Side.East);
            W = pointMap.Links.Any(x => x.Side == Side.West);
            S = pointMap.Links.Any(x => x.Side == Side.South);
            N = pointMap.Links.Any(x => x.Side == Side.North);
            MapId = gid;
            MapResId = resId;
        }

        public PveMap GenPveMapPlayGround()
        {
            
            throw new NotImplementedException();
        }
    }
}