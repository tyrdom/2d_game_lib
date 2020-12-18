using System;
using System.Collections.Generic;
using System.Linq;

namespace rogue_chapter_maker
{
    public static class MakerTools
    {
        public static Random Random { get; } = new Random();
    }

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
                var pointMap = new PointMap(MapSize.Big, 1, 1, 1, 1, nowSlot);
                nowSlot = genAChapterMap.AddMapPointAndReturnNext(pointMap);
            }

            foreach (var _ in enumerable2)
            {
                var pointMap = new PointMap(MapSize.Small, 1, 1, 1, 1, nowSlot);
                nowSlot = genAChapterMap.AddMapPointAndReturnNext(pointMap);
            }

            return genAChapterMap;
        }

        private (int x, int y) AddMapPointAndReturnNext(PointMap pointMap)
        {
            var any = PointMaps.Any();
            var pointMapLinks = new HashSet<Link>();

            pointMapLinks.UnionWith(pointMap.Links);
            if (any)
            {
                var haveLink = false;
                foreach (var isNearAndGetLinks in PointMaps.Select(map => map.IsNearAndGetLinks(pointMap, haveLink))
                    .Where(isNearAndGetLinks => isNearAndGetLinks != null))
                {
                    CanLinks.ExceptWith(isNearAndGetLinks?.thisLink ?? Array.Empty<Link>());
                    pointMapLinks.ExceptWith(isNearAndGetLinks?.thatLink ?? Array.Empty<Link>());
                    haveLink = true;
                }
            }

            PointMaps.Add(pointMap);
            CanLinks.UnionWith(pointMapLinks);
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