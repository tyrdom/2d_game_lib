using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace rogue_chapter_maker
{
    public class ChapterMapTop
    {
        public ChapterMapTop()
        {
            PointMaps = new HashSet<PointMap>();
            CanLinks = new HashSet<Link>();
        }

        public override string ToString()
        {
            var valueTuples = PointMaps.Select(x => (x.To3Strings(), x.Slot)).ToArray();
            var max = valueTuples.Select(tuple => tuple.Slot.x).Max();
            var min = valueTuples.Select(tuple => tuple.Slot.x).Min();
            var enumerable = Enumerable.Range(min, max - min + 1);
            var groupBy = valueTuples.GroupBy(t => t.Slot.y).Select(g =>
                {
                    var argKey = g.Key;
                    var ints = g.Select(p => p.Slot.x).ToArray();
                    foreach (var i in ints)
                    {
                        Console.Out.WriteLine($"x:{i},y{argKey}");
                    }

                    var dictionary = g.ToDictionary(p => p.Slot.x, p => p.Item1);
                    var valueTuple = enumerable.Aggregate(("", "", ""),
                        (s, x)
                            =>
                        {
                            var (item1, item2, item3) = s;
                            if (dictionary.TryGetValue(x, out var ss))
                            {
                                return (item1 + ss.t,
                                    item2 + ss.c,
                                    item3 + ss.b);
                            }

                            return (item1 + "   ",
                                item2 + "   ",
                                item3 + "   ");
                        });
                    return (argKey, valueTuple);
                }
            ).ToList();
            groupBy.Sort((x, y) =>
                -x.argKey.CompareTo(y.argKey));
            var array = groupBy.SelectMany(x => $"{x.valueTuple.Item1}\n{x.valueTuple.Item2}\n{x.valueTuple.Item3}\n"
            ).ToArray();
            return new string(array);
        }

        private HashSet<Link> CanLinks { get; }
        public HashSet<PointMap> PointMaps { get; }

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

        public static ChapterMapTop GenAChapterMap(int big, int small)
        {
            var enumerable = Enumerable.Range(1, big);
            var nowSlot = (0, 0);
            var enumerable2 = Enumerable.Range(1, small);
            var genAChapterMap = new ChapterMapTop();
            foreach (var _ in enumerable)
            {
                var pointMap = new PointMap(MapType.Big, 1, 1, 1, 1, nowSlot);
                genAChapterMap.AddMapPoint(pointMap);
                nowSlot = genAChapterMap.GenANewSlot();
            }

            var e = new PointMap(MapType.In, 1, 1, 1, 1, nowSlot);
            genAChapterMap.AddMapPoint(e);
            nowSlot = genAChapterMap.GenANewSlot();
            genAChapterMap.SetFinishSlot(e);

            foreach (var _ in enumerable2)
            {
                var pointMap = new PointMap(MapType.Small, 1, 1, 1, 1, nowSlot);
                genAChapterMap.AddMapPoint(pointMap);
                nowSlot = genAChapterMap.GenANewSlot();
            }

            return genAChapterMap;
        }

        private void SetFinishSlot(PointMap pointMap)
        {
            (int distance, PointMap? farMap) func = (0, null);
            var (distance, farMap) = PointMaps.Aggregate(func, (a, map) =>
            {
                var intPtr = map.GetDistance(pointMap);
                var (dis, fMap) = a;
                return intPtr > dis ? (intPtr, map) : (dis, fMap);
            });
            if (farMap == null) throw new ArgumentNullException($"not big enough map {distance}");
            CanLinks.ExceptWith(farMap.Links);
            farMap.SetFinish();
        }

        private void AddMapPoint(PointMap pointMap)
        {
            var any = PointMaps.Any();
            var pointMapLinks = new HashSet<Link>();

            pointMapLinks.UnionWith(pointMap.Links);
            if (any)
            {
                var isNearAndGetLinksList = PointMaps.Select(map => map.IsNearAndGetLinks(pointMap))
                    .Where(isNearAndGetLinks => isNearAndGetLinks != null).ToList();
                isNearAndGetLinksList.Shuffle();
                var count = isNearAndGetLinksList.Count;
                var i = MakerTools.Random.Next(count);
                Console.Out.WriteLine($"{i + 1} of {count}");
                for (var i1 = 0; i1 <= i; i1++)
                {
                    var isNearAndGetLinks = isNearAndGetLinksList[i1];
                    if (isNearAndGetLinks == null) continue;
                    var (thisLink, thatLink, thisMap, thatMap) = isNearAndGetLinks.Value;
                    PointMap.SetNearLinks(thisLink, thatLink,
                        thisMap, thatMap);
                }

                foreach (var isNearAndGetLinks in isNearAndGetLinksList)
                {
                    CanLinks.ExceptWith(isNearAndGetLinks?.thisLink ?? Array.Empty<Link>());
                    pointMapLinks.ExceptWith(isNearAndGetLinks?.thatLink ?? Array.Empty<Link>());
                }
            }

            PointMaps.Add(pointMap);
            CanLinks.UnionWith(pointMapLinks);
        }

        private (int x, int y) GenANewSlot()
        {
            var next = MakerTools.Random.Next(CanLinks.Count);
            var canLink = CanLinks.ToList()[next];
            var (x, y) = canLink.InPointMap.Slot;

            return canLink.Side switch
            {
                Side.East => (x + 1, y),
                Side.West => (x - 1, y),
                Side.North => (x, y + 1),
                Side.South => (x, y - 1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}