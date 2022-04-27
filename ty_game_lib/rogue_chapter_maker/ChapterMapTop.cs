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
            var max = valueTuples.Select(tuple => tuple.Slot.X).Max();
            var min = valueTuples.Select(tuple => tuple.Slot.X).Min();
            var enumerable = Enumerable.Range(min, max - min + 1);
            var groupBy = valueTuples.GroupBy(t => t.Slot.Y).Select(g =>
                {
                    var argKey = g.Key;
                    var ints = g.Select(p => p.Slot.X).ToArray();
                    foreach (var i in ints)
                    {
                        Console.Out.WriteLine($"x:{i},y{argKey}");
                    }

                    var dictionary = g.ToDictionary(p => p.Slot.X, p => p.Item1);
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
            var nowSlot = new Slot(0, 0);
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

            PointMap? endP = null;
            if (finishB)
            {
                if (startP != null)
                {
                    endP = genAChapterMap.SetFarthestSlot(startP, MapType.Big, MapType.BigEnd);
                }
                else
                {
                    endP = new PointMap(MapType.BigEnd, 1, 1, 1, 1, nowSlot);
                    genAChapterMap.AddMapPoint(endP);
                    genAChapterMap.CanLinks.ExceptWith(endP.Links);
                }
            }

            foreach (var _ in enumerable2)
            {
                var pointMap = new PointMap(MapType.Small, 1, 1, 1, 1, nowSlot);
                genAChapterMap.AddMapPoint(pointMap);
                nowSlot = genAChapterMap.GenANewSlot();
            }

            if (!startB)
            {
                if (endP != null)
                {
                    startP = genAChapterMap.SetFarthestSlot(endP, MapType.Small, MapType.SmallStart);
                }
                else
                {
                    startP = new PointMap(MapType.SmallStart, 1, 1, 1, 1, nowSlot);
                    genAChapterMap.AddMapPoint(startP);
                }
            }

            if (startP == null)
            {
                throw new Exception("have not set a start!");
            }

            if (endP == null)
            {
                genAChapterMap.SetFarthestSlot(startP, MapType.Small, MapType.SmallEnd);
            }


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

        private PointMap? FindFarthestMap(PointMap pointMap, MapType needType)
        {
            var valueTuples = PointMaps.Where(m => m.MapType == needType).Select(x => (x.GetDistance(pointMap), x))
                .ToList();
            valueTuples.Sort((tuple, valueTuple) => -tuple.Item1.CompareTo(valueTuple.Item1));
            var (_, pointMap1) = valueTuples.FirstOrDefault();
            return pointMap == pointMap1 ? null : pointMap1;
        }

        private PointMap SetFarthestSlot(PointMap pointMap, MapType needType, MapType toSet)
        {
            var findFarMap = FindFarthestMap(pointMap, needType);
            if (findFarMap == null) throw new ArgumentNullException($"not big enough map ");
            var nearSlotCanUse = GetNearSlotCanUse(findFarMap);
            if (!nearSlotCanUse.Any())
            {
                throw new Exception("no good map");
            }

            var valueTuple = nearSlotCanUse.First();

            var map = new PointMap(toSet, 1, 1, 1, 1, valueTuple);
            AddMapPoint(map);
            CanLinks.ExceptWith(map.Links);
            return map;
        }

        private Slot[] GetNearSlotCanUse(PointMap findFarMap)
        {
            var slotX = findFarMap.Slot.X;
            var slotY = findFarMap.Slot.Y;
            var valueTuples = new[]
            {
                new Slot(slotX + 1, slotY), new Slot(slotX - 1, slotY), new Slot(slotX, slotY + 1),
                new Slot(slotX, slotY - 1)
            };
            var enumerable = valueTuples.Except(valueTuples.Where(x => PointMaps.Any(xx =>
            {
                var i = xx.Slot.X;
                var y = xx.Slot.Y;
                return i == x.X && y == x.Y;
            })));

            return enumerable.ToArray();
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

        private Slot GenANewSlot()
        {
            var next = MakerTools.Random.Next(CanLinks.Count);
            var canLink = CanLinks.ToList()[next];
            var x = canLink.InPointMap.Slot.X;
            var y = canLink.InPointMap.Slot.Y;
            return canLink.Side switch
            {
                direction.East => new Slot(x + 1, y),
                direction.West => new Slot(x - 1, y),
                direction.North => new Slot(x, y + 1),
                direction.South => new Slot(x, y - 1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}