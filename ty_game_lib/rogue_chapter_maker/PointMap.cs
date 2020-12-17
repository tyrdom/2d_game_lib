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
        public PointMap(MapSize mapSize, int n, int s, int e, int w)
        {
            MapSize = mapSize;
            var enumerable = Enumerable.Range(1, n).Select(x => new Link(Side.North)).ToList();
            var links = Enumerable.Range(1, s).Select(x => new Link(Side.South)).ToList();
            var list = Enumerable.Range(1, e).Select(x => new Link(Side.East)).ToList();
            var links1 = Enumerable.Range(1, w).Select(x => new Link(Side.West)).ToList();
            enumerable.AddRange(links);
            enumerable.AddRange(list);
            enumerable.AddRange(links1);
            Links = enumerable;
        }

        public PointMap(MapSize mapSize, List<Link> links)
        {
            MapSize = mapSize;
            Links = links;
        }

        private MapSize MapSize { get; }

        public List<Link> Links { get; }
    }
}