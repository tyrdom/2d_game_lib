using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace rogue_chapter_maker
{
    public enum MapType
    {
        Enter,
        Finish,
        Small,
        Big
    }

    public class PointMap
    {
        private char ShortChar()
        {
            return MapSize switch
            {
                MapType.Small => 'S',
                MapType.Big => 'B',
                MapType.Enter => 'E',
                MapType.Finish => 'F',
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public (string t, string c, string b ) To3Strings()
        {
            var enumerable = Links.Where(x => x.LinkTo != null).ToArray();
            var nn = enumerable.Count(a => a.Side == Side.North);
            var en = enumerable.Count(a => a.Side == Side.East);
            var wn = enumerable.Count(a => a.Side == Side.West);
            var sn = enumerable.Count(a => a.Side == Side.South);

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

        public PointMap(MapType mapSize, int n, int s, int e, int w, (int x, int y) slot)
        {
            MapSize = mapSize;
            Slot = slot;
            var enumerable = Enumerable.Range(1, n).Select(x => new Link(Side.North, this)).ToList();
            var links = Enumerable.Range(1, s).Select(x => new Link(Side.South, this)).ToList();
            var list = Enumerable.Range(1, e).Select(x => new Link(Side.East, this)).ToList();
            var links1 = Enumerable.Range(1, w).Select(x => new Link(Side.West, this)).ToList();
            enumerable.AddRange(links);
            enumerable.AddRange(list);
            enumerable.AddRange(links1);
            Links = enumerable;
        }


        public (int x, int y) Slot { get; }
        public MapType MapSize { get; }

        public List<Link> Links { get; }

        public static void SetNearLinks(IEnumerable<Link> thisLinks, IEnumerable<Link> thatLinks, PointMap thisMap, PointMap thatMap)
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
                var thisLink = Links.Where(x => x.Side == Side.East);
                var thatLink = toAnotherOne.Links.Where(x => x.Side == Side.West);
                return (thisLink, thatLink, this, toAnotherOne);
            }

            if (w)
            {
                var thisLink = Links.Where(x => x.Side == Side.West);
                var thatLink = toAnotherOne.Links.Where(x => x.Side == Side.East);

                return (thisLink, thatLink, this, toAnotherOne);
            }

            if (n)
            {
                var thisLink = Links.Where(x => x.Side == Side.North);
                var thatLink = toAnotherOne.Links.Where(x => x.Side == Side.South);
                return (thisLink, thatLink, this, toAnotherOne);
            }

            if (s)
            {
                var thisLink = Links.Where(x => x.Side == Side.South);
                var thatLink = toAnotherOne.Links.Where(x => x.Side == Side.North);

                return (thisLink, thatLink, this, toAnotherOne);
            }

            return null;
        }

        public int GetDistance(PointMap pointMap)
        {
            var slotX = Slot.x - pointMap.Slot.x;
            var slotY = Slot.y - pointMap.Slot.y;
            return slotX * slotX +
                   slotY * slotY;
        }
    }
}