using System;
using System.Collections.Generic;
using System.Linq;
using game_config;

namespace rogue_chapter_maker
{
    public enum MapType
    {
        BigStart,
        BigEnd,
        Small,
        Big,
        SmallStart,
        SmallEnd,
        Vendor,
        Hangar,
        Nothing
    }

    public class PointMap
    {
        private char ShortChar()
        {
            return MapType switch
            {
                MapType.Small => 'x',
                MapType.Big => 'X',
                MapType.BigStart => 'S',
                MapType.BigEnd => 'E',
                MapType.SmallStart => 's',
                MapType.SmallEnd => 'e',
                MapType.Vendor => 'v',
                MapType.Hangar => 'H',
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public (string t, string c, string b ) To3Strings()
        {
            var enumerable = Links.Where(x => x.LinkTo != null).ToArray();
            var nn = enumerable.Count(a => a.Side == direction.North);
            var en = enumerable.Count(a => a.Side == direction.East);
            var wn = enumerable.Count(a => a.Side == direction.West);
            var sn = enumerable.Count(a => a.Side == direction.South);

            var t = $" {nn} ";
            var c = $"{wn}{ShortChar()}{en}";
            var b = $" {sn} ";
            return (t, c, b);
        }

        public override string ToString()
        {
            var (t, c, b) = To3Strings();
            return $"{t}\n{c}\n{b}\n";
        }

        public PointMap((int x, int y) slot, MapType mapType, List<Link> links)
        {
            Slot = slot;
            MapType = mapType;
            Links = links;
        }

        public PointMap(MapType mapType, int n, int s, int e, int w, (int x, int y) slot)
        {
            MapType = mapType;
            Slot = slot;
            var enumerable = Enumerable.Range(1, n).Select(x => new Link(direction.North, this)).ToList();
            var links = Enumerable.Range(1, s).Select(x => new Link(direction.South, this)).ToList();
            var list = Enumerable.Range(1, e).Select(x => new Link(direction.East, this)).ToList();
            var links1 = Enumerable.Range(1, w).Select(x => new Link(direction.West, this)).ToList();
            enumerable.AddRange(links);
            enumerable.AddRange(list);
            enumerable.AddRange(links1);
            Links = enumerable;
        }

        public void SetFinish()
        {
            if (MapType == MapType.Big)
            {
                MapType = MapType.BigEnd;
            }

            if (MapType == MapType.Small)
            {
                MapType = MapType.SmallEnd;
            }
        }

        public (int x, int y) Slot { get; }
        public MapType MapType { get; private set; }

        public List<Link> Links { get; set; }

        public static void SetNearLinks(IEnumerable<Link> thisLinks, IEnumerable<Link> thatLinks, PointMap thisMap,
            PointMap thatMap)
        {
            foreach (var link in thisLinks)
            {
                link.SetLink(thatMap);
            }

            foreach (var link in thatLinks)
            {
                link.SetLink(thisMap);
            }
        }

        public (IEnumerable<Link> thisLink, IEnumerable<Link> thatLink, PointMap thisMap, PointMap thatMap)?
            IsNearAndGetLinks(PointMap toAnotherOne)
        {
            var b1 = Slot.y == toAnotherOne.Slot.y;
            var b2 = Slot.x == toAnotherOne.Slot.x;
            var e = -Slot.x + toAnotherOne.Slot.x == 1 && b1;
            var w = -Slot.x + toAnotherOne.Slot.x == -1 && b1;
            var n = -Slot.y + toAnotherOne.Slot.y == 1 && b2;
            var s = -Slot.y + toAnotherOne.Slot.y == -1 && b2;


            if (e)
            {
                var thisLink = Links.Where(x => x.Side == direction.East);
                var thatLink = toAnotherOne.Links.Where(x => x.Side == direction.West);
                return (thisLink, thatLink, this, toAnotherOne);
            }

            if (w)
            {
                var thisLink = Links.Where(x => x.Side == direction.West);
                var thatLink = toAnotherOne.Links.Where(x => x.Side == direction.East);

                return (thisLink, thatLink, this, toAnotherOne);
            }

            if (n)
            {
                var thisLink = Links.Where(x => x.Side == direction.North);
                var thatLink = toAnotherOne.Links.Where(x => x.Side == direction.South);
                return (thisLink, thatLink, this, toAnotherOne);
            }

            if (s)
            {
                var thisLink = Links.Where(x => x.Side == direction.South);
                var thatLink = toAnotherOne.Links.Where(x => x.Side == direction.North);

                return (thisLink, thatLink, this, toAnotherOne);
            }

            return null;
        }

        public int GetDistance(PointMap pointMap)
        {
            var slotX = Slot.x - pointMap.Slot.x;
            var slotY = Slot.y - pointMap.Slot.y;
            return Math.Abs(slotX) +
                   Math.Abs(slotY);
        }

        public void SetHangar()
        {
            MapType = MapType.Hangar;
        }

        public void SetVendor()
        {
            MapType = MapType.Vendor;
        }
    }
}