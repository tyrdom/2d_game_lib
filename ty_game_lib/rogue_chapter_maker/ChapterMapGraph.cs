using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace rogue_chapter_maker
{
    public class ChapterMapGraph
    {
        public ChapterMapGraph()
        {
            PointMaps = new List<PointMap>();
            CanLinks = new List<Link>();
        }

        private List<Link> CanLinks { get; }
        private List<PointMap> PointMaps { get; }

        public Side LinkRule(Side side)
        {
            return side switch
            {
                Side.East => Side.West,
                Side.West => Side.East,
                Side.North => Side.South,
                Side.South => Side.North,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        public static ChapterMapGraph GenAChapterMap(int big, int small)
        {
            var enumerable = Enumerable.Range(1, big);

            var enumerable2 = Enumerable.Range(1, small);
            var genAChapterMap = new ChapterMapGraph();
            foreach (var i in enumerable)
            {
                var pointMap = new PointMap(MapSize.Big, 1, 1, 1, 1);
                genAChapterMap.AddMapPoint(pointMap);
            }

            foreach (var i2 in enumerable2)
            {
                var pointMap = new PointMap(MapSize.Small, 1, 1, 1, 1);
                genAChapterMap.AddMapPoint(pointMap);
            }

            return genAChapterMap;
        }

        private void AddMapPoint(PointMap pointMap)
        {
            var any = PointMaps.Any();
            if (any)
            {
            }
            else
            {
                PointMaps.Add(pointMap);

                var pointMapLinks = pointMap.Links;
                CanLinks.AddRange(pointMapLinks);
            }

            throw new NotImplementedException();
        }
    }
}