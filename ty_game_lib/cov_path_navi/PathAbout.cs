using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace cov_path_navi
{
    public class Link
    {
        public int LinkToPathNodeId;

        public TwoDVectorLine GoThrough;

        public Link(int linkToPathNodeId, TwoDVectorLine goThrough)
        {
            LinkToPathNodeId = linkToPathNodeId;
            GoThrough = goThrough;
        }

        public override string ToString()
        {
            return $"link to ::{LinkToPathNodeId} go through" + GoThrough.Log();
        }
    }

    public class CutIncompleteLink
    {
        public Link Link;
        public TwoDVectorLine Mirror;
        public int Belong;

        public CutIncompleteLink(Link link, TwoDVectorLine mirror)
        {
            Link = link;
            Mirror = mirror;
            Belong = -1;
        }

        public bool IsComplete()
        {
            return Link.LinkToPathNodeId >= 0 && Belong >= 0;
        }
    }

    public class PathNodeCovPolygon
    {
        public List<Link> Links;

        public int ThisPathNodeId;

        public List<IBlockShape> Edges;

        public PathNodeCovPolygon(List<Link> links, int thisPathNodeId, List<IBlockShape> edges)
        {
            Links = links;
            ThisPathNodeId = thisPathNodeId;
            Edges = edges;
        }

        public AreaBox GenAreaBox()
        {
            var twoDVectorLines = Edges.Select(x => x.AsTwoDVectorLine());
            var simpleBlocks = new SimpleBlocks(twoDVectorLines);
            var genZone = simpleBlocks.GenZone();
            return new AreaBox(genZone, simpleBlocks, ThisPathNodeId);
        }

        public override string ToString()
        {
            var s1 = $"Id::{ThisPathNodeId}\n";
            var aggregate = Edges.Aggregate("", (s, x) => s + x.Log() + "\n");
            var aggregate1 = Links.Aggregate("", (s, x) => s + x + "\n");

            return $"{s1} edges::\n{aggregate} links::\n{aggregate1}";
        }
    }


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
            var aggregate = Area.Aggregate("", (s, x) => s + x.Log() + "\n");
            var aggregate2 =
                LinksAndMirrorLine.Aggregate("",
                    (s, x) => s + x.link.LinkToPathNodeId + "::" + x.link.GoThrough.Log() + "\n");

            return $"Area::\n{aggregate} Cuts::\n{aggregate2}";
        }

        public List<PathNodeCovPolygon> ToCovPolygons(ref int nowOnId)
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

            static void GetACov(List<IBlockShape> area, int offset,
                List<PathNodeCovPolygon> polygons,
                Dictionary<TwoDVectorLine, CutIncompleteLink> incompleteLinks, ref int nowId
            )
            {
                // 在Area中切出一个凸多边形（可能共线切不出）
                var areaSimpleBlocks = new SimpleBlocks(area.Select(x => x.AsTwoDVectorLine()));


                static void GetACovInArea(ref int li, ref int ri, List<IBlockShape> area,
                    bool rCanGo,
                    bool lCanGo,
                    ref bool inALine,
                    bool right, SimpleBlocks blocks)
                {
                    if (li == ri)
                    {
#if DEBUG
                        Console.Out.WriteLine($"just a cov l{li} m r{ri}");
#endif
                        li = -1;
                        ri = -1;
                        return;
                    }


                    var leftLine = area[li].AsTwoDVectorLine();
                    var areaCount = ri == 0 ? area.Count - 1 : ri - 1;
                    var rightLine = area[areaCount].AsTwoDVectorLine();

                    if (right && rCanGo)
                    {
                        var nextR = area[ri];
                        var canGo = CanGo(leftLine, rightLine, nextR.GetEndPt(), ref inALine, blocks);
                        if (canGo)
                            ri = ri + 1 == area.Count ? 0 : ri + 1;

                        GetACovInArea(ref li, ref ri, area, canGo, lCanGo, ref inALine, !lCanGo, blocks);
                        return;
                    }

                    if (lCanGo)
                    {
                        var nextL = area[(area.Count + li - 1) % area.Count];

                        var canGo = CanGo(rightLine, leftLine, nextL.GetStartPt(), ref inALine, blocks);

                        if (canGo)
                            li = li == 0 ? area.Count - 1 : li - 1;

                        GetACovInArea(ref li, ref ri, area, rCanGo, canGo, ref inALine, rCanGo, blocks);
                        return;
                    }

#if DEBUG
                    Console.Out.WriteLine($"Cant Go Further at {li} and {ri} in {area.Count}");
#endif
                    return;

                    static bool CanGo(TwoDVectorLine otherLine, TwoDVectorLine thisLine,
                        TwoDPoint next, ref bool aLine, SimpleBlocks simpleBlocks)
                    {
                        var leftNPos = next.GetPosOf(thisLine);

                        var lCov = leftNPos != Pt2LinePos.Right;

                        var notOver = next.GetPosOf(otherLine) != Pt2LinePos.Right;
                        var twoDVectorLine =
                            new TwoDVectorLine(otherLine.GetEndPt(), next);
                        var lineCross = simpleBlocks.LineCross(twoDVectorLine);
                        var ptInShape = simpleBlocks.PtInShapeIncludeSide(twoDVectorLine.GetMid());
                        var canGo = lCov && notOver && !lineCross && ptInShape;
                        aLine = canGo ? leftNPos == Pt2LinePos.On && aLine : aLine;

                        return canGo;
                    }
                }

                var inALine1 = true;
                var lfc1 = offset;
                var rfc1 = offset + 1 >= area.Count ? 0 : offset + 1;
                GetACovInArea(ref lfc1, ref rfc1, area, true, true, ref inALine1, false, areaSimpleBlocks);

                if (lfc1 == -1)
                {
#if DEBUG
                    var aggregate3 = area.Aggregate("", (s, x) => s + x.Log() + "\n");
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

                    GetACov(area, offset + 1, polygons, incompleteLinks, ref nowId);

                    return;
                }
#if DEBUG
                Console.Out.WriteLine("found a cov  , cut it off");
#endif
                var (covShape, rest, covS, restS) = CutTo2Part(area, rfc1, lfc1);


                var restNewLink = new Link(nowId, restS);
                var covNewLink = new Link(-1, covS);

                var links = new List<Link> {covNewLink};
                CollectLinks(links, incompleteLinks, nowId, covShape);

                static void CollectLinks(ICollection<Link> links,
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


                var pathNodeCovPolygon = new PathNodeCovPolygon(links, nowId, covShape);
                polygons.Add(pathNodeCovPolygon);
                var restIncomplete = new CutIncompleteLink(restNewLink, covS);
                var covSIncomplete = new CutIncompleteLink(covNewLink, restS);
                incompleteLinks[restS] = restIncomplete;
                incompleteLinks[covS] = covSIncomplete;
#if DEBUG
                var aggregate = covShape.Aggregate("", (s, x) => s + x.Log() + "\n");

                var aggregate2 = rest.Aggregate("", (s, x) => s + x.Log() + "\n");
                Console.Out.WriteLine(
                    $"cut to 2 part \n {aggregate}\n and \n{aggregate2} \n with , and go to find next cov");
#endif

                nowId += 1;
                GetACov(rest, 0, polygons, incompleteLinks, ref nowId);
            }
        }


        // static (List<IBlockShape> part1, List<IBlockShape> part2, List<IBlockShape> triangle) CutAInTriangle(
        //     List<IBlockShape> raw, int fCount)
        // {
        // }


        static (List<IBlockShape> covShape, List<IBlockShape> restShape, TwoDVectorLine covS, TwoDVectorLine restS)
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
            else
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
        }
    }

    public class WalkAreaBlock
    {
        public SimpleBlocks ShellBlocks { get; }
        public List<IBlockShape> ShellRaw { get; set; }
        public readonly List<List<IBlockShape>> ChildrenRaw;

        public readonly List<SimpleBlocks> ChildrenBlocks;


        public WalkAreaBlock(List<List<IBlockShape>> children, List<IBlockShape> blockShapes)
        {
            ChildrenRaw = children;
            ShellRaw = blockShapes;
            ShellBlocks = new SimpleBlocks(blockShapes);
            ChildrenBlocks = children.Select(x => new SimpleBlocks(x)).ToList();
        }

        public override string ToString()
        {
            var shellString = ShellRaw.Aggregate("", (s, x) => s + x.Log() + "\n");
            var cStr = "";
            for (var i = 0; i < ChildrenRaw.Count; i++)
            {
                var child = ChildrenRaw[i].Aggregate("", (s, x) => s + x.Log() + "\n");

                cStr += $"child{i} : \n" + child;
            }

            return $"shell : \n {shellString} \n{cStr}";
        }

        public void AddChildren(List<IBlockShape> shapes)
        {
            ChildrenRaw.Add(shapes);
            ChildrenBlocks.Add(new SimpleBlocks(shapes));
        }

        public List<IBlockShape> GetShellBlocks()
        {
            return ShellBlocks.GetBlockShapes().ToList();
        }

        public ContinuousWalkArea GenWalkArea()
        {
            //child逐个合并到shell上

            var linksAndMirror = new List<(Link link, TwoDVectorLine mirror)>();
            if (!ShellRaw.Any())
            {
                throw new Exception("must have some shells");
            }

            if (ChildrenRaw.Count <= 0)
            {
                return new ContinuousWalkArea(ShellRaw, linksAndMirror);
            }

            foreach (var aChildBlockShapes in ChildrenRaw)
            {
                (var si, var ci, TwoDVectorLine cut) = (-1, -1, null!);
                for (var i1 = 0; i1 < aChildBlockShapes.Count; i1++)
                {
                    var blockShape = aChildBlockShapes[i1];
                    var childPoint = blockShape.GetEndPt();

                    for (var i = 0; i < ShellRaw.Count; i++)
                    {
                        var shape = ShellRaw[i];
                        var shellPt = shape.GetEndPt();
                        var twoDVectorLine = new TwoDVectorLine(shellPt, childPoint);
                        foreach (var unused in ChildrenBlocks
                            .Select(simpleBlocks => simpleBlocks.LineCross(twoDVectorLine))
                            .Where(lineCross => !lineCross))
                        {
                            if (cut == null)
                            {
                                cut = twoDVectorLine;
                                si = i;
                                ci = i1;
                            }
                            else if (twoDVectorLine.GetVector().SqNorm() < cut.GetVector().SqNorm())
                            {
                                cut = twoDVectorLine;
                                si = i;
                                ci = i1;
                            }
                        }
                    }
                }

                var (sLeft, sRight) = SomeTools.CutList(ShellRaw, si);
                var (cLeft, cRight) = SomeTools.CutList(aChildBlockShapes, ci);
                if (cut != null)
                {
                    var shapes = new List<IBlockShape> {cut};
                    shapes.AddRange(cRight);
                    shapes.AddRange(cLeft);
                    var cutMirror = cut.Reverse();
                    shapes.Add(cutMirror);
                    shapes.AddRange(sRight);
                    shapes.AddRange(sLeft);
                    linksAndMirror.Add((new Link(-1, cutMirror), cut));
                    linksAndMirror.Add((new Link(-1, cut), cutMirror));

                    ShellRaw = shapes;
                    ChildrenBlocks.RemoveAt(0);
#if DEBUG
                    Console.Out.WriteLine($"a cut ok {cut.Log()}");
                    var aggregate = shapes.Aggregate("", (s, x) => s + x.Log() + "\n");
                    Console.Out.WriteLine($"res side have {shapes.Count} side is \n{aggregate}");
#endif
                }
                else
                {
                    throw new Exception($"cant find a cut from {ToString()}");
                }
            }

            return new ContinuousWalkArea(ShellRaw, linksAndMirror);
        }
    }

    public class AreaBox
    {
        private Zone Zone;

        private SimpleBlocks SimpleBlocks;

        public AreaBox(Zone zone, SimpleBlocks simpleBlocks, int polyId)
        {
            Zone = zone;
            SimpleBlocks = simpleBlocks;
            PolyId = polyId;
        }

        private int PolyId { get; }

        public int? InPoly(TwoDPoint pt)
        {
            return Zone.IncludePt(pt) && SimpleBlocks.PtInShapeIncludeSide(pt) ? (int?) PolyId : null;
        }
    }

    public class PathNodes
    {
        private Dictionary<int, PathNodeCovPolygon> PolygonsTop;

        private List<AreaBox> AreaBoxes;

        public PathNodes(WalkBlock walkBlock)
        {
            if (walkBlock.QSpace == null)
            {
                PolygonsTop = new Dictionary<int, PathNodeCovPolygon>();
                AreaBoxes = new List<AreaBox>();
            }
            else
            {
                var blockShapes = walkBlock.QSpace.GetAllIBlocks().ToList();
                var genFromBlocks = GenFromBlocks(blockShapes);
                var walkAreaBlocks = GenBlockUnits(genFromBlocks, walkBlock.IsBlockIn);
                var continuousWalkAreas = walkAreaBlocks.Select(x => x.GenWalkArea());
                var a = -1;
                var pathNodeCovPolygons = continuousWalkAreas.SelectMany(x => x.ToCovPolygons(ref a)).ToList();
                PolygonsTop = pathNodeCovPolygons.ToDictionary(x => x.ThisPathNodeId, x => x);
                AreaBoxes = pathNodeCovPolygons.Select(x => x.GenAreaBox()).ToList();
            }
        }

        public int? InWhichPoly(TwoDPoint pt)
        {
            foreach (var inPoly in AreaBoxes
                .Select(areaBox => areaBox.InPoly(pt))
                .Where(inPoly => inPoly != null))
            {
                return inPoly;
            }

            return null;
        }

        int[] FindAPath(int start, int end)
        {
            return new[] {0}; //todo 
        }

        //分割为凸多边形，标记联通关系


        //合并阻挡 去除环结构
        public static List<WalkAreaBlock> GenBlockUnits(List<List<IBlockShape>> bs, bool isBlockIn)
        {
            var firstBlockUnit = isBlockIn
                ? new WalkAreaBlock(new List<List<IBlockShape>>(), new List<IBlockShape>())
                : new WalkAreaBlock(new List<List<IBlockShape>>(), bs.First());

            var blockUnits = new List<WalkAreaBlock> {firstBlockUnit};
            for (var i = 1; i < bs.Count; i++)
            {
                var shapes = bs[i];
                var twoDPoint = shapes.First().GetStartPt();
                var beChild = false;
                foreach (var blockUnit in from blockUnit in blockUnits
                    where blockUnit.ShellBlocks.IsEmpty() || blockUnit.ShellBlocks.PtRealInShape(twoDPoint)
                    let all = blockUnit.ChildrenBlocks.All(x => !x.PtRealInShape(twoDPoint))
                    where all
                    select blockUnit)
                {
                    blockUnit.AddChildren(shapes);
                    beChild = true;
                }

                if (beChild) continue;
                {
                    var blockUnit = new WalkAreaBlock(new List<List<IBlockShape>>(), shapes);
                    blockUnits.Add(blockUnit);
                }
            }

            return blockUnits;
        }

        //找连续阻挡，分组
        public static List<List<IBlockShape>> GenFromBlocks(List<IBlockShape> rawList)
        {
            //找连续阻挡，分组
            rawList.Sort((x, y) => x.GetStartPt().X.CompareTo(y.GetStartPt().X));
            List<List<IBlockShape>> res = new List<List<IBlockShape>>();

            return GenLinkLists(res, rawList);

            static List<List<IBlockShape>> GenLinkLists(List<List<IBlockShape>> res, List<IBlockShape> rList)
            {
                if (rList.Count <= 0) return res;
                var (lList, nrList) = GenAListFromBlocks(rList);
                if (lList.Count > 0)
                {
                    res.Add(lList);
                    return GenLinkLists(res, nrList);
                }

                var aggregate = rList.Aggregate("", (s, x) => s + x.Log());
                throw new Exception($"cant find a block list but there is rest block {aggregate}");
            }

            static ( List<IBlockShape> lList, List<IBlockShape> rList) GenAListFromBlocks(List<IBlockShape> rawList)
            {
                var first = rawList.First();

                List<IBlockShape> tempList = new List<IBlockShape> {first};
                rawList.RemoveAt(0);
                var startPt = first.GetStartPt();
                var endPt = first.GetEndPt();

                return SepRawList(startPt, endPt, tempList, rawList);
            }


            static ( List<IBlockShape> lList, List<IBlockShape> rList)
                SepRawList(TwoDPoint lstPt, TwoDPoint ledPt, List<IBlockShape> tList, List<IBlockShape> rawList)
            {
                var beforeCount = tList.Count;
                var s1 = tList.Aggregate("", (s, x) => s + x.Log());
                var (stPt, edPt, lList, rList) = SepARawList(lstPt, ledPt, tList, rawList);
                if (stPt == null || edPt == null)
                {
                    return (lList, rList);
                }

                if (lList.Count > beforeCount) return SepRawList(stPt, edPt, lList, rList);

                var aggregate = lList.Aggregate("l::", (s, x) => s + x.Log());
                var aggregate1 = rList.Aggregate("r::", (s, x) => s + x.Log());
                throw new Exception(
                    $"can not finish a block list::num {beforeCount} vs {lList.Count} from {rList.Count} ::\n::{edPt.Log()}::\n {s1} vs {aggregate} \n from ::\n{aggregate1}");
            }


            static ( TwoDPoint? stPt, TwoDPoint? edPt, List<IBlockShape> lList, List<IBlockShape> rList)
                SepARawList(TwoDPoint lstPt, TwoDPoint ledPt, List<IBlockShape> tList, List<IBlockShape> rawList)
            {
                var isEnd = false;
#if DEBUG
                var find = false;
#endif

                var edPt = ledPt;
                var newRaw = new List<IBlockShape>();
                foreach (var blockShape in rawList!)
                {
                    var startPt = blockShape.GetStartPt();
                    var endPt = blockShape.GetEndPt();
                    if (isEnd)
                    {
                        newRaw.Add(blockShape);
                        continue;
                    }


#if DEBUG
                    Console.Out.WriteLine($" finding new ed  {edPt.Log()} vs {startPt.Log()}");
#endif
                    if (startPt == edPt
                        // || startPt.Same(edPt)
                    )
                    {
                        tList.Add(blockShape);
                        if (endPt == lstPt
                            // || endPt.Same(lstPt)
                        )
                        {
#if DEBUG
                            Console.Out.WriteLine($" found finish {endPt.Log()} vs {lstPt.Log()}");
#endif

                            isEnd = true;
                            continue;
                        }

#if DEBUG
                        find = true;
                        Console.Out.WriteLine($" found new ed  {edPt.Log()} vs {startPt.Log()}");
#endif

                        edPt = blockShape.GetEndPt();
                    }
                    else
                    {
                        newRaw.Add(blockShape);
                    }
                }
#if DEBUG
                if (find) return isEnd ? (null, null, tList, newRaw)! : (lstPt, edPt, tList, newRaw);
                var aggregate = rawList.Aggregate("", (s, x) => s + x.Log());
                Console.Out.WriteLine($" not found {edPt.Log()} in {aggregate}");
#endif
                return isEnd ? (null, null, tList, newRaw)! : (lstPt, edPt, tList, newRaw);
            }
        }
    }
}