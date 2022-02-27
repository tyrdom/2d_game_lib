using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace cov_path_navi
{
    public class ContinuousWalkArea
    {
        public ContinuousWalkArea(List<IBlockShape> area, List<(Link link, TwoDVectorLine mirror)> linksAndMirrorLine
        )
        {
            LinksAndMirrorLine = linksAndMirrorLine;

            Area = area;
        }

        private List<IBlockShape> Area { get; }

        private List<(Link link, TwoDVectorLine mirror)> LinksAndMirrorLine { get; }

        public override string ToString()
        {
            var aggregate = Area.Aggregate("", (s, x) => s + x.ToString() + "\n");
            var aggregate2 =
                LinksAndMirrorLine.Aggregate("",
                    (s, x) => s + x.link.LinkToPathNodeId + "::" + x.link.GoThrough.ToString() + "\n");

            return $"Area::\n{aggregate} Cuts::\n{aggregate2}";
        }

        public IEnumerable<PathNodeCovPolygon> ToCovPolygons(ref int nowOnId)
        {
            // 使用切耳法

            //没有确定连接id和未确定属于哪个id的，都在这个未完成link里
            Dictionary<TwoDVectorLine, CutIncompleteLink> incompleteLinks =
                LinksAndMirrorLine.ToDictionary(x => x.link.GoThrough,
                    x => new CutIncompleteLink(x.link, x.mirror));
#if DEBUG
            Console.Out.WriteLine(
                $"new ContinuousWalkArea about to cut {Area.Aggregate("", (s, x) => s + "=" + x.GetEndPt()).Replace('|', '=')}");
#endif
            var pathNodeCovPolygons = new List<PathNodeCovPolygon>();
            nowOnId += 1;
            GetACov(Area, 0, pathNodeCovPolygons, incompleteLinks, ref nowOnId);

#if DEBUG
            var aggregate1 = pathNodeCovPolygons.Aggregate("", (s, x) => s + x + "\n");

            Console.Out.WriteLine($"cut finish result is~~~ \n{aggregate1} \n nowId::{nowOnId}");
#endif
            return pathNodeCovPolygons;
        }

        private static void GetACov(List<IBlockShape> area, int offset, ICollection<PathNodeCovPolygon> polygons,
            IDictionary<TwoDVectorLine, CutIncompleteLink> incompleteLinks, ref int nowId)
        {
            while (true)
            {
                // 在Area中切出一个凸多边形（可能共线切不出）
                var areaSimpleBlocks = new SimpleBlocks(area.Select(x => x.AsTwoDVectorLine()));
                var inALine1 = true;
                var lfc1 = offset;
                var rfc1 = offset;

                // CheckFirstLineCross(area, lfc1, rfc1);

                GetACovInArea(ref lfc1, ref rfc1, area, true, true, ref inALine1, true, areaSimpleBlocks);

                if (lfc1 == -1)
                {
#if DEBUG
                    var aggregate3 = area.Aggregate("", (s, x) => s + x.ToString() + "\n");
                    Console.Out.WriteLine($"this just a cov shape \n{aggregate3}");

#endif
                    var list = new List<Link>();
                    CollectLinks(list, incompleteLinks, nowId, area);
                    var nodeCovPolygon = new PathNodeCovPolygon(list, nowId, area);
                    polygons.Add(nodeCovPolygon);

                    return;
                }

                if (inALine1)
                {
#if DEBUG
                    Console.Out.WriteLine($"cov is in a line , need change another start place,offset::{offset}");
#endif

                    offset += 1;

                    continue;
                }
#if DEBUG
                Console.Out.WriteLine("found a cov  , cut it off");
#endif
                var (covShape, rest, covS, restS)
                    = CutTo2Part(area, rfc1, lfc1);
                var restNewLink = new Link(nowId, restS);
                var covNewLink = new Link(-1, covS);

                var links = new List<Link> {covNewLink};
                CollectLinks(links, incompleteLinks, nowId, covShape);


                var pathNodeCovPolygon = new PathNodeCovPolygon(links, nowId, covShape);
                polygons.Add(pathNodeCovPolygon);
                var restIncomplete = new CutIncompleteLink(restNewLink, covS);
                var covSIncomplete = new CutIncompleteLink(covNewLink, restS);
                incompleteLinks[restS] = restIncomplete;
                incompleteLinks[covS] = covSIncomplete;
#if DEBUG
                var aggregate = covShape.Aggregate("", (s, x) => s + x.GetEndPt() + "=").Replace('|', '=');

                var aggregate2 = rest.Aggregate("", (s, x) => s + x.GetEndPt() + "=").Replace('|', '=');
                Console.Out.WriteLine(
                    $"cut to 2 part \n {aggregate}\n and \n{aggregate2} \n  , and go to find next cov");
#endif

                nowId += 1;
                area = rest;
                offset = 0;
            }
        }

        private static void CollectLinks(ICollection<Link> links,
            IDictionary<TwoDVectorLine, CutIncompleteLink> incompleteLinks, int nowId,
            IEnumerable<IBlockShape> covShape)
        {
            foreach (var twoDVectorLine in covShape.OfType<TwoDVectorLine>())
            {
                if (!incompleteLinks.TryGetValue(twoDVectorLine, out var cut)) continue;
                links.Add(cut.Link);

                cut.Belong = nowId;
                if (cut.IsComplete())
                {
                    incompleteLinks.Remove(twoDVectorLine);
                }

                var mirrorLine = cut.Mirror;
                if (!incompleteLinks.TryGetValue(mirrorLine, out var mCut)) continue;
                mCut.Link.LinkToPathNodeId = nowId;
                if (mCut.IsComplete())
                {
                    incompleteLinks.Remove(mirrorLine);
                }
            }
        }

        private static void GetACovInArea(ref int li, ref int ri, IReadOnlyList<IBlockShape> area, bool rCanGo,
            bool lCanGo, ref bool inALine, bool right, SimpleBlocks blocks)
        {
            while (true)
            {
                if (li == (ri + 2) % area.Count && !inALine || area.Count <= 3)
                {
#if DEBUG
                    Console.Out.WriteLine($"just a cov l{li} m r{ri}");
#endif

                    li = -1;
                    ri = -1;
                    return;
                }
#if DEBUG
                if (li < 0 || li >= area.Count)
                {
                    Console.Out.WriteLine($"{li} of {area.Count}");
                    throw new Exception($"~~~~{li}~{ri} of {area.Count}:{area.Aggregate("", (s, x) => s + "~" + x)}");
                }
#endif
                var leftLine = area[li].AsTwoDVectorLine();
                var rightLine = area[ri].AsTwoDVectorLine();

#if DEBUG
                Console.Out.WriteLine($"l::{li} r::{ri} all {area.Count}\n");
                Console.Out.WriteLine($" l is {leftLine} m r is {rightLine}\n");
#endif
                if (right && rCanGo)
                {
                    // Console.Out.WriteLine($"go right before {ri} {rCanGo}");
                    var areaCount = (ri + 1) % area.Count;
                    var nextR = area[areaCount];
                    var twoDPoint = nextR.GetEndPt();

                    var (covShape, restShape, _, _) = CutTo2Part(area.ToList(), areaCount, li);
                    var simpleBlocks = new SimpleBlocks(covShape.Select(x => x.AsTwoDVectorLine()));
                    var twoDPoints = restShape.Select(x => x.GetEndPt())
                        .ToArray();
                    var any = !twoDPoints.Any(simpleBlocks.PtRealInShape);
#if DEBUG
                    var aggregate = twoDPoints.Aggregate("rest", (s, x) => s + ":" + x);
                    Console.Out.WriteLine($"rest points {aggregate}");
#endif

#if DEBUG
                    Console.Out.WriteLine($"next pt {twoDPoint} is not in cut {any}");
#endif
                    var canGo = any && CanGo(leftLine, rightLine, twoDPoint, ref inALine, blocks, true);
                    if (canGo) ri = areaCount;

                    rCanGo = canGo;
                    right = !lCanGo;
                    // Console.Out.WriteLine($"go right {ri} {rCanGo}");
                    continue;
                }

                if (lCanGo && !right)
                {
                    // Console.Out.WriteLine($"go left before:{li} {lCanGo}");

                    var areaCount = (area.Count + li - 1) % area.Count;
                    var nextL = area[areaCount];

                    var twoDPoint = nextL.GetStartPt();

                    var (covShape, restShape, _, _) = CutTo2Part(area.ToList(), ri, areaCount);
                    var simpleBlocks = new SimpleBlocks(covShape.Select(x => x.AsTwoDVectorLine()));
                    var twoDPoints = restShape.Select(x => x.GetEndPt())
                        .ToArray();
                    var any = !twoDPoints.Any(x => simpleBlocks.PtRealInShape(x));
#if DEBUG
                    var aggregate = twoDPoints.Aggregate("rest", (s, x) => s + ":" + x);
                    Console.Out.WriteLine($"rest points {aggregate}");
#endif

#if DEBUG
                    Console.Out.WriteLine(
                        $"next pt {twoDPoint} rest point is not in cut {li} {areaCount} \n{simpleBlocks} \n re {any}");
#endif
                    var canGo = any && CanGo(rightLine, leftLine, twoDPoint, ref inALine, blocks, false);

                    if (canGo) li = areaCount;

                    lCanGo = canGo;
                    right = rCanGo;
                    // Console.Out.WriteLine($"go left {li} {lCanGo}");

                    continue;
                }

#if DEBUG
                Console.Out.WriteLine(
                    $"Cant Go Further at {li} and {ri} in {area.Count}: {area.Aggregate("", (s, x) => s + "~" + x)}");
#endif
                return;
            }
        }

        private static bool CanGo(TwoDVectorLine otherLine, TwoDVectorLine thisLine, TwoDPoint next, ref bool aLine,
            SimpleBlocks simpleBlocks, bool goRight)
        {
            var nPos = next.GetPosOf(thisLine);
            var same = next.Same(goRight ? thisLine.GetEndPt() : thisLine.GetStartPt());
            var lCov = nPos != Pt2LinePos.Right;

            var notOver = next.GetPosOf(otherLine) != Pt2LinePos.Right;

            var line = goRight
                ? new TwoDVectorLine(next, otherLine.GetStartPt())
                : new TwoDVectorLine(otherLine.GetEndPt(), next);

            var lineCross = simpleBlocks.LineCross(line);
            var twoDPoint = line.GetMid();
            var include = simpleBlocks.Include(twoDPoint);
            var canGo = lCov && notOver && !lineCross && include;
            aLine = canGo ? nPos == Pt2LinePos.On && !same && aLine : aLine;
#if DEBUG
            Console.Out.WriteLine(
                $"{simpleBlocks} \nvs{line} \n  ");
            Console.Out.WriteLine(
                $"can go {canGo}:left1 {lCov} left2 {notOver}  not cross {!lineCross} include {include}\n");
            Console.Out.WriteLine($"in a line ? {aLine} go right? {goRight}");
#endif
            return canGo;
        }


        private static (List<IBlockShape> covShape, List<IBlockShape> restShape, TwoDVectorLine covS, TwoDVectorLine
            restS)
            CutTo2Part(
                List<IBlockShape> raw, int right,
                int left)
        {
            var p1 = raw[right].GetEndPt();
            var p2 = raw[left].GetStartPt();
            var covS = new TwoDVectorLine(p1, p2);
            var restS = covS.Reverse();

            if (right < left)
            {
                var blockShapes = raw.GetRange(0, right + 1);
                var shapes = raw.GetRange(left, raw.Count - left);
                shapes.AddRange(blockShapes);
                var rest = raw.GetRange(right + 1, left - right - 1);
                var covShape = new List<IBlockShape> {covS};
                covShape.AddRange(shapes);
                var restShape = new List<IBlockShape> {restS};
                restShape.AddRange(rest);
                return (covShape, restShape, covS, restS);
            }

            if (right > left)

            {
                var shapes = raw.GetRange(left, right + 1 - left);
                var rest = raw.GetRange(right + 1, raw.Count - right - 1);
                var blockShapes = raw.GetRange(0, left);
                rest.AddRange(blockShapes);
                var covShape = new List<IBlockShape> {covS};
                covShape.AddRange(shapes);
                var restShape = new List<IBlockShape> {restS};
                restShape.AddRange(rest);
                return (covShape, restShape, covS, restS);
            }

            throw new Exception("no good cut offset same");
        }
    }
}