using System;
using System.Collections.Generic;
using System.Linq;
using game_config;


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

        public static ChapterMapTop GenAChapterTopByConfig(rogue_game_chapter gameChapter)
        {
            return GenAChapterMap(gameChapter.BigMap, gameChapter.SmallMap, gameChapter.VendorMap,
                gameChapter.HangarMap,
                gameChapter.StartWithBig, gameChapter.EndWithBig,
                gameChapter.VendorMapStart, gameChapter.VendorMapRange, gameChapter.HangarMapStart,
                gameChapter.HangarMapRange
            );
        }

        public static ChapterMapTop GenAChapterMap(int big, int small, int vendor, int hangar, bool startB,
            bool finishB, int vStart, int vRangeCount, int hStart, int hRangeCount)
        {
            var enumerable = Enumerable.Range(1, big + hangar);
            var nowSlot = (0, 0);
            var enumerable2 = Enumerable.Range(1, small + vendor);
            var genAChapterMap = new ChapterMapTop();
            foreach (var _ in enumerable)
            {
                var pointMap = new PointMap(MapType.Big, 1, 1, 1, 1, nowSlot);
                genAChapterMap.AddMapPoint(pointMap);
                nowSlot = genAChapterMap.GenANewSlot();
            }

            PointMap? startP = null;
            if (startB)
            {
                startP = new PointMap(MapType.BigStart, 1, 1, 1, 1, nowSlot);
                genAChapterMap.AddMapPoint(startP);
                nowSlot = genAChapterMap.GenANewSlot();
            }


            foreach (var _ in enumerable2)
            {
                var pointMap = new PointMap(MapType.Small, 1, 1, 1, 1, nowSlot);
                genAChapterMap.AddMapPoint(pointMap);
                nowSlot = genAChapterMap.GenANewSlot();
            }

            if (!startB)
            {
                startP = new PointMap(MapType.SmallStart, 1, 1, 1, 1, nowSlot);
                genAChapterMap.AddMapPoint(startP);
            }

            if (startP == null)
            {
                throw new Exception("have not set a start!");
            }

            genAChapterMap.SetFinishSlot(startP, finishB ? MapType.Big : MapType.Small);
            if (hangar > 0)
            {
                genAChapterMap.SetHangars(startP, hStart, hRangeCount, hangar);
            }

            if (vendor > 0)
            {
                genAChapterMap.SetVendors(startP, vStart, vRangeCount, vendor);
            }

            foreach (var pointMap in genAChapterMap.PointMaps)
            {
                var where = pointMap.Links.Where(x => x.LinkTo != null);
                pointMap.Links = where.ToList();
            }

            return genAChapterMap;
        }

        private void SetVendors(PointMap start, int rate, int rangeCount, int num)
        {
            var findFarMap = FindFarMaps(start, MapType.Small, rate, rangeCount, num);

            foreach (var pointMap in findFarMap)
            {
                pointMap.SetVendor();
            }
        }

        private void SetHangars(PointMap start, int rate, int rangeCount, int num)
        {
            var findFarMap = FindFarMaps(start, MapType.Big, rate, rangeCount, num);

            foreach (var pointMap in findFarMap)
            {
                pointMap.SetHangar();
            }
        }

        private List<PointMap> FindFarMaps(PointMap pointMap, MapType needType, int startIndex, int countRange, int num)
        {
            var valueTuples = PointMaps.Where(m => m.MapType == needType)
                .Select(map => (map.GetDistance(pointMap), map))
                .ToList();
            valueTuples.Sort((tuple, valueTuple) => tuple.Item1.CompareTo(valueTuple.Item1));
            var tuples = valueTuples.GetRange(startIndex - 1, countRange);
            var pointMaps = tuples.Select(x => x.map).ToList();
            pointMaps.Shuffle();
            if (pointMaps.Count < num)
            {
                throw new Exception("not big enough map to set shop");
            }

            var range = pointMaps.GetRange(0, num);

            return range;
        }

        private PointMap? FindFarMap(PointMap pointMap, MapType needType)
        {
            var valueTuples = PointMaps.Where(m => m.MapType == needType).Select(x => (x.GetDistance(pointMap), x))
                .ToList();
            valueTuples.Sort((tuple, valueTuple) => -tuple.Item1.CompareTo(valueTuple.Item1));
            var (_, pointMap1) = valueTuples.FirstOrDefault();
            return pointMap == pointMap1 ? null : pointMap1;
            (int distance, PointMap? farMap) func = (0, null);
            var (_, farMap) = PointMaps.Aggregate(func, (a, map) =>
            {
                var intPtr = map.GetDistance(pointMap);
                var b = map.MapType == needType;
                var (dis, fMap) = a;
                return intPtr > dis && b ? (intPtr, map) : (dis, fMap);
            });
            return farMap;
        }

        private void SetFinishSlot(PointMap pointMap, MapType needType)
        {
            var findFarMap = FindFarMap(pointMap, needType);
            if (findFarMap == null) throw new ArgumentNullException($"not big enough map ");
            CanLinks.ExceptWith(findFarMap.Links);
            findFarMap.SetFinish();
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
                direction.East => (x + 1, y),
                direction.West => (x - 1, y),
                direction.North => (x, y + 1),
                direction.South => (x, y - 1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}