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
            Console.Out.WriteLine("new ContinuousWalkArea about to cut ");
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
                var rfc1 = offset + 1 >= area.Count ? 0 : offset + 1;

                // CheckFirstLineCross(area, lfc1, rfc1);

                GetACovInArea(ref lfc1, ref rfc1, area, true, true, ref inALine1, false, areaSimpleBlocks);

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
            var aggregate = covShape.Aggregate("", (s, x) => s + x.ToString() + "\n");

            var aggregate2 = rest.Aggregate("", (s, x) => s + x.ToString() + "\n");
            Console.Out.WriteLine($"cut to 2 part \n {aggregate}\n and \n{aggregate2} \n  , and go to find next cov");
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
                if (li == ri || area.Count <= 3)
                {
#if DEBUG
#endif
                    Console.Out.WriteLine($"just a cov l{li} m r{ri}");
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
                var areaCount = ri == 0 ? area.Count - 1 : ri - 1;
                var rightLine = area[areaCount].AsTwoDVectorLine();
                Console.Out.WriteLine($"l::{li} r::{ri}");
#if DEBUG
            Console.Out.WriteLine($" l is {leftLine} m r is {rightLine}");
#endif
                if (right && rCanGo)
                {
                    Console.Out.WriteLine($"go right before {ri} {rCanGo}");
                    var nextR = area[ri];
                    var canGo = CanGo(leftLine, rightLine, nextR.GetEndPt(), ref inALine, blocks, true);
                    if (canGo) ri = ri + 1 == area.Count ? 0 : ri + 1;

                    var lCanGo1 = lCanGo;
                    rCanGo = canGo;
                    right = !lCanGo1;
                    Console.Out.WriteLine($"go right {ri} {rCanGo}");
                    continue;
                }

                if (lCanGo)
                {
                    Console.Out.WriteLine($"go left before:{li} {lCanGo}");

                    var nextL = area[(area.Count + li - 1) % area.Count];

                    var canGo = CanGo(rightLine, leftLine, nextL.GetStartPt(), ref inALine, blocks, false);

                    if (canGo) li = li == 0 ? area.Count - 1 : li - 1;

                    var rCanGo1 = rCanGo;
                    lCanGo = canGo;
                    right = rCanGo1;
                    Console.Out.WriteLine($"go left {li} {lCanGo}");

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
            var leftNPos = next.GetPosOf(thisLine);
            var same = next.Same(thisLine.GetEndPt());
            var lCov = leftNPos != Pt2LinePos.Right;

            var notOver = next.GetPosOf(otherLine) != Pt2LinePos.Right;
            var twoDPoint = goRight ? otherLine.GetStartPt() : otherLine.GetEndPt();
            var twoDVectorLine = new TwoDVectorLine(twoDPoint, next);

            var lineCross = simpleBlocks.LineCross(twoDVectorLine);
#if DEBUG
                Console.Out.WriteLine($"{simpleBlocks} vs {twoDVectorLine} \n {lineCross} ");
#endif
            var ptInShape = simpleBlocks.PtInShapeIncludeSide(twoDVectorLine.GetMid());
            var canGo = lCov && notOver && !lineCross && ptInShape;
            aLine = canGo ? leftNPos == Pt2LinePos.On && !same && aLine : aLine;
            return canGo;
        }


        private static (List<IBlockShape> covShape, List<IBlockShape> restShape, TwoDVectorLine covS, TwoDVectorLine
            restS)
            CutTo2Part(
                List<IBlockShape> raw, int right,
                int left)
        {
            var p1 = raw[right].GetStartPt();
            var p2 = raw[left].GetStartPt();
            var covS = new TwoDVectorLine(p1, p2);
            var restS = covS.Reverse();

            if (right < left)
            {
                var blockShapes = raw.GetRange(0, right);
                var shapes = raw.GetRange(left, raw.Count - left);
                shapes.AddRange(blockShapes);
                var rest = raw.GetRange(right, left - right);
                var covShape = new List<IBlockShape> {covS};
                covShape.AddRange(shapes);
                var restShape = new List<IBlockShape> {restS};
                restShape.AddRange(rest);
                return (covShape, restShape, covS, restS);
            }

            if (right > left)

            {
                var shapes = raw.GetRange(left, right - left);
                var rest = raw.GetRange(right, raw.Count - right);
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