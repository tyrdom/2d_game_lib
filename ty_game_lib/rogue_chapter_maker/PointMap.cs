using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace rogue_chapter_maker
{
    public enum MapSize
    {
        Small,
        Big
    }

    public class PointMap
    {
        private string ShortString()
        {
            return MapSize switch
            {
                MapSize.Small => "S",
                MapSize.Big => "B",
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

            var t = " " + nn + " ";
            var c = wn + ShortString() + en;
            var b = " " + sn + " ";
            return (t, c, b);
        }

        public override string ToString()
        {
            var (t, c, b) = To3Strings();
            return $"{t}\n{c}\n{b}\n";
        }

        public PointMap(MapSize mapSize, int n, int s, int e, int w, (int x, int y) slot)
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
        public MapSize MapSize { get; }

        public List<Link> Links { get; }

        public (IEnumerable<Link> thisLink, IEnumerable<Link> thatLink)? IsNearAndGetLinks(PointMap toAnotherOne,
            bool haveLink)
        {
            var b1 = Slot.y == toAnotherOne.Slot.y;
            var b2 = Slot.x == toAnotherOne.Slot.x;
            var e = -Slot.x + toAnotherOne.Slot.x == 1 && b1;
            var w = -Slot.x + toAnotherOne.Slot.x == -1 && b1;
            var n = -Slot.y + toAnotherOne.Slot.y == 1 && b2;
            var s = -Slot.y + toAnotherOne.Slot.y == -1 && b2;

            void SetNearLinks(List<Link> thisLinks, List<Link> thatLinks, PointMap thisMap, PointMap thatMap)
            {
                if (haveLink)
                {
                    var next = MakerTools.Random.Next(2);
                    if (next < 1) return;
                }

                foreach (var link in thisLinks)
                {
                    link.SetLink(thatMap);
                }

                foreach (var link in thatLinks)
                {
                    link.SetLink(thisMap);
                }
            }

            if (e)
            {
                var thisLink = Links.Where(x => x.Side == Side.East).ToList();
                var thatLink = toAnotherOne.Links.Where(x => x.Side == Side.West).ToList();
                SetNearLinks(thisLink, thatLink, this, toAnotherOne);
                return (thisLink,
                    thatLink);
            }

            if (w)
            {
                var thisLink = Links.Where(x => x.Side == Side.West).ToList();
                var thatLink = toAnotherOne.Links.Where(x => x.Side == Side.East).ToList();
                SetNearLinks(thisLink, thatLink, this, toAnotherOne);

                return (thisLink,
                    thatLink);
            }

            if (n)
            {
                var thisLink = Links.Where(x => x.Side == Side.North).ToList();
                var thatLink = toAnotherOne.Links.Where(x => x.Side == Side.South).ToList();
                SetNearLinks(thisLink, thatLink, this, toAnotherOne);

                return (thisLink,
                    thatLink);
            }

            if (s)
            {
                var thisLink = Links.Where(x => x.Side == Side.South).ToList();
                var thatLink = toAnotherOne.Links.Where(x => x.Side == Side.North).ToList();
                SetNearLinks(thisLink, thatLink, this, toAnotherOne);

                return (thisLink,
                    thatLink);
            }

            return null;
        }
    }
}