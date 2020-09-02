using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class Link
    {
        public int LinkId;

        public TwoDVectorLine GoThrough;

        public Link(int linkId, TwoDVectorLine goThrough)
        {
            LinkId = linkId;
            GoThrough = goThrough;
        }
    }

    public class PathNodeCovPoly
    {
        public List<Link> Links;

        public int PathNodeId;

        public List<IBlockShape> Edges;

        public PathNodeCovPoly(List<Link> links, int pathNodeId, List<IBlockShape> edges)
        {
            Links = links;
            PathNodeId = pathNodeId;
            Edges = edges;
        }
    }


    public class ContinuousWalkArea
    {
        public ContinuousWalkArea(List<IBlockShape> area, List<(Link link, TwoDVectorLine mirror)> links
        )
        {
            Links = links;

            Area = area;
        }

        private List<IBlockShape> Area { get; }

        private List<(Link link, TwoDVectorLine mirror)> Links { get; }

        public override string ToString()
        {
            var aggregate = Area.Aggregate("", (s, x) => s + x.Log() + "\n");
            var aggregate2 = Links.Aggregate("", (s, x) => s + x.link.LinkId + "::" + x.link.GoThrough.Log() + "\n");

            return $"Area::\n{aggregate} Cuts::\n{aggregate2}";
        }

        public void ToCovPolygons3()
        {
            //三角分割然后合并

            for (int i = 0; i < Area.Count; i++)
            {
            }
        }

        public void ToCovPolygons2()
        {
            //分割记录字典，起点是key 多个分割线按照逆时针排布队列

            var simpleBlocks = new SimpleBlocks(Area);
            //找凸点，通过切分消除凸点
            for (var i = 0; i < Area.Count; i++)
            {
                var line1 = Area[i].AsTwoDVectorLine();
                var blockShape = Area[(i + 1) % Area.Count];
                var twoDPoint = blockShape.GetEndPt();
                var pt2LinePos = twoDPoint.GetPosOf(line1);


                if (pt2LinePos == Pt2LinePos.Right)
                {
                    //发现凸点，生成切割线找落在夹脚内最短的线
                    TwoDVectorLine oCut = null!;
                    TwoDVectorLine lCut = null!;
                    TwoDVectorLine rCut = null!;
                    var oInk = -1f;
                    var rInk = -1f;
                    var lInk = -1f;

                    //line1 line2 形成夹脚，只有在夹脚内的切割才不用再分割
                    var line2 = blockShape.AsTwoDVectorLine();
                    for (var i1 = i + 2; i1 < Area.Count; i1++)
                    {
                        var obp = Area[i1].GetEndPt();
                        var twoDVectorLine = new TwoDVectorLine(twoDPoint, obp);
                        var lineCross = simpleBlocks.LineCross(twoDVectorLine); //不穿过
                        var ptRealInShape = simpleBlocks.PtRealInShape(twoDVectorLine.GetMid()); //在区域里
                        var realInShape = !lineCross && ptRealInShape;
                        if (realInShape)
                        {
                            var cutToCov = obp.CanCutToCov(line1, line2);
                            switch (cutToCov)
                            {
                                case Pt2LinePos.Right:
                                    if (rCut == null)
                                    {
                                        rCut = twoDVectorLine;
                                        rInk = twoDVectorLine.GetVector().SqNorm();
                                    }
                                    else if (twoDVectorLine.GetVector().SqNorm() < rInk)
                                    {
                                        rCut = twoDVectorLine;
                                    }

                                    break;
                                case Pt2LinePos.On:
                                    if (oCut == null)
                                    {
                                        oCut = twoDVectorLine;
                                        oInk = twoDVectorLine.GetVector().SqNorm();
                                    }
                                    else if (twoDVectorLine.GetVector().SqNorm() < oInk)
                                    {
                                        oCut = twoDVectorLine;
                                    }

                                    break;
                                case Pt2LinePos.Left:
                                    if (lCut == null)
                                    {
                                        lCut = twoDVectorLine;
                                        lInk = twoDVectorLine.GetVector().SqNorm();
                                    }
                                    else if (twoDVectorLine.GetVector().SqNorm() < lInk)
                                    {
                                        lCut = twoDVectorLine;
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
            }
        }


        static bool IsCovPt(List<IBlockShape> area, int i)
        {
            var blockShape = area[i].AsTwoDVectorLine();
            var shape = area[i + 1].GetEndPt();
            return shape.GetPosOf(blockShape) == Pt2LinePos.Left;
        }


        public void ToCovPolygons(int nowOnId)

        {
            var blockShape = Area.First();
            //check right
            var shape = Area.Last();
            var b = blockShape.GetStartPt() == shape.GetEndPt();
            var alink = Links.FirstOrDefault(x => x.link.GoThrough == blockShape);


            if (!b)
            {
                throw new Exception("no good Continuous Area :: not end to start");
            }
#if DEBUG

            Console.Out.WriteLine(" check ok ");
#endif
            //没有确定连接id和未确定属于哪个id的，都在这个未完成link里
            var incompleteLinks = Links.ToDictionary(x => x.link.GoThrough, x => x);
            var links = new List<Link>();

            if (alink.link != null)
            {
#if DEBUG
                Console.Out.WriteLine
                    ($"found start cuts {alink.link.LinkId} :: {alink.link.GoThrough.Log()}");
#endif
                links.Add(alink.link);

                if (incompleteLinks.TryGetValue(alink.mirror, out alink))
                {
#if DEBUG
                    Console.Out.WriteLine($"set mirror link to this poly {nowOnId}");
#endif
                    alink.link.LinkId = nowOnId;
                }
            }

            GetACov(Area);

            static void GetACov(List<IBlockShape> area)
            {
                // 在Area中切出一个凸多边形（可能切不出）
                var areaSimpleBlocks = new SimpleBlocks(area);

                static void GetAJumpCovInArea(int lfc, int rfc, List<IBlockShape> area, SimpleBlocks blocks)
                {
                    //先找出一个范围在直线左方，再缩小范围找凸多边形？或者直接找一个三角形/四边形

                    var pt = area[rfc - 1].GetEndPt();
                    var pt2 = area[(area.Count - lfc) % area.Count].GetStartPt();
                    var twoDVectorLine = new TwoDVectorLine(pt, pt2);
                    var ints = new List<int>();
                    for (var i = rfc + 1; i < area.Count - lfc - 1; i++)
                    {
                        //选点
                        var ptt = area[i].GetEndPt();
                        var b1 = ptt.GetPosOf(twoDVectorLine) != Pt2LinePos.Left;
                        var dVectorLine = new TwoDVectorLine(pt, ptt);
                        var vectorLine = new TwoDVectorLine(ptt, pt2);
                        var lineCross = blocks.LineCross(dVectorLine);
                        var cross = blocks.LineCross(vectorLine);
                        var b2 = b1 && !lineCross && !cross;
                        if (b2)
                        {
                            ints.Add(i);
                        }
                    }
                }

                static void GetANoJumpCovInArea(ref int lfc, ref int rfc, List<IBlockShape> area,
                    bool rCanGo,
                    bool lCanGo,
                    ref bool inALine,
                    bool right, SimpleBlocks blocks)
                {
                    if (lfc + rfc >= area.Count)
                    {
#if DEBUG
                        Console.Out.WriteLine($"just a cov l{lfc} m r{rfc}");
#endif
                        lfc = -1;
                        rfc = -1;
                        return;
                    }

                    var leftLine = area[(area.Count - lfc) % area.Count].AsTwoDVectorLine();
                    var rightLine = area[rfc - 1].AsTwoDVectorLine();

                    if (right && rCanGo)
                    {
                        var nextR = area[rfc % area.Count];
                        var canGo = CanGo(leftLine, rightLine, nextR.GetEndPt(), ref inALine, blocks);
                        if (canGo)
                            rfc += 1;

                        GetANoJumpCovInArea(ref lfc, ref rfc, area, canGo, lCanGo, ref inALine, !lCanGo, blocks);
                        return;
                    }

                    if (lCanGo)
                    {
                        var nextL = area[area.Count - lfc - 1 % area.Count];

                        var canGo = CanGo(rightLine, leftLine, nextL.GetStartPt(), ref inALine, blocks);

                        if (canGo)
                            lfc += 1;

                        GetANoJumpCovInArea(ref lfc, ref rfc, area, rCanGo, canGo, ref inALine, rCanGo, blocks);
                        return;
                    }


#if DEBUG
                    Console.Out.WriteLine($"Cant Go Further at {lfc} and {rfc}");
#endif
                    return;

                    static bool CanGo(TwoDVectorLine otherLine, TwoDVectorLine thisLine,
                        TwoDPoint next, ref bool aLine, SimpleBlocks simpleBlocks)
                    {
                        var leftNPos = next.GetPosOf(thisLine);
                        // Console.Out.WriteLine($"{next.Log()} and {thisLine.Log()} pos is {leftNPos}");
                        var lCov = leftNPos != Pt2LinePos.Right;

                        aLine = lCov ? leftNPos == Pt2LinePos.On && aLine : aLine;
                        var notOver = next.GetPosOf(otherLine) != Pt2LinePos.Right;
                        var twoDVectorLine =
                            new TwoDVectorLine(otherLine.GetEndPt(), next);
                        var lineCross = simpleBlocks.LineCross(twoDVectorLine);
                        var ptInShape = simpleBlocks.PtInShapeIncludeSide(twoDVectorLine.GetMid());
                        var canGo = lCov && notOver && !lineCross && ptInShape;
                        return canGo;
                    }
                }

                var inALine1 = true;
                var lfc1 = 0;
                var rfc1 = 1;
                GetANoJumpCovInArea(ref lfc1, ref rfc1, area, true, true, ref inALine1, false, areaSimpleBlocks);

                if (lfc1 == -1)
                {
#if DEBUG
                    Console.Out.WriteLine("this just a cov shape");
#endif
                    return;
                }

                if (inALine1)
                {
#if DEBUG
                    Console.Out.WriteLine("cov is in a line , need cut to 3 part");
#endif

                    return;
                }

                var bCount = area.Count - lfc1;
                var (covShape, rest, lineCov, _) = CutTo2Part(area, rfc1, bCount);


#if DEBUG
                var aggregate = covShape.Aggregate("", (s, x) => s + x.Log() + "\n");

                var aggregate2 = rest.Aggregate("", (s, x) => s + x.Log() + "\n");
                Console.Out.WriteLine(
                    $"cut to 2 part \n {aggregate}\n and \n{aggregate2} \n with {lineCov.Log()}, and go to find next cov");
#endif
                GetACov(rest);
            }
        }


        // static (List<IBlockShape> part1, List<IBlockShape> part2, List<IBlockShape> triangle) CutAInTriangle(
        //     List<IBlockShape> raw, int fCount)
        // {
        // }


        static (List<IBlockShape> covShape, List<IBlockShape> rest, TwoDVectorLine linkLineCov, TwoDVectorLine
            linkLineRest) CutTo2Part(
                List<IBlockShape> raw, int fCount,
                int bCount)
        {
            var p1 = raw[fCount].GetStartPt();
            var p2 = raw[bCount].GetStartPt();
            var covS = new TwoDVectorLine(p1, p2);
            var restS = covS.Reverse();

            var blockShapes = raw.GetRange(0, fCount);
            var shapes = raw.GetRange(bCount, raw.Count - bCount);
            shapes.AddRange(blockShapes);
            var rest = raw.GetRange(fCount, bCount - fCount);
            var list = new List<IBlockShape>() {covS};
            list.AddRange(shapes);
            var blockShapes1 = new List<IBlockShape>() {restS};
            blockShapes1.AddRange(rest);
            return (list, blockShapes1, covS, restS);
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

    // var (sLeft, sRight) = SomeTools.CutList(ShellRaw, i);
    // var (cLeft, cRight) = SomeTools.CutList(blockShapes, i1);
    // shapes.AddRange(sLeft);
    // shapes.Add(twoDVectorLine);
    // shapes.AddRange(cRight);
    // shapes.AddRange(cLeft);
    // shapes.Add(twoDVectorLine.Reverse());
    // shapes.AddRange(sRight);
    // break;
    public class PathMap
    {
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