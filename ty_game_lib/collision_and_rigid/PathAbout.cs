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


            TwoDPoint? fNoCovPt = null;


            //向前搜索 第一个凹点


            var covForwardCount = 0;

            var inALine = true;
            for (var i = 0; i < Area.Count; i++)
            {
                var blockShape1 = Area[i];
                var line1 = blockShape1.AsTwoDVectorLine();

                var shape1 = Area[(i + 1) % Area.Count];
                var endPt = shape1.GetEndPt();

                // 
                var pt2LinePos = endPt.GetPosOf(line1);

                if (pt2LinePos == Pt2LinePos.Right)
                {
                    //发现凹点

                    //准备分割消除凹点
                    var twoDVectorLines = Area.Select(x => x.AsTwoDVectorLine());
                    var simpleBlocks = new SimpleBlocks(twoDVectorLines);
                    var covPt = blockShape1.GetEndPt();
#if DEBUG
                    Console.Out.WriteLine($"meet no cov edge; point {covPt.Log()}");
#endif
                    var line2 = shape1.AsTwoDVectorLine();

                    for (var j = i + 2; j < i + 1 + Area.Count; j++)
                    {
                        var blockShape2 = Area[j % Area.Count];
                        var ept = blockShape2.GetEndPt();

                        var ep1 = ept.GetPosOf(line1);
                        var ep2 = ept.GetPosOf(line2);
                        //情况1 终点就在夹脚内（包含边缘），可以分割
                        var eCovLine = new TwoDVectorLine(ept, covPt);
                        var lineCrossE = simpleBlocks.LineCross(eCovLine);
                        if (lineCrossE)
                        {
                            var b1 = ep1 != Pt2LinePos.Right && ep2 != Pt2LinePos.Right;
                            if (b1)
                            {
#if DEBUG
                                Console.Out.WriteLine(
                                    $"line cut to 2 part {eCovLine.Log()} on scan {blockShape2.Log()}");
#endif
                                break;
                            }

                            //情况2：起点终点穿过区域，使用三角形分割
                            var spt = blockShape2.GetStartPt();
                            var sp1 = spt.GetPosOf(line1);
                            var sp2 = spt.GetPosOf(line2);
                            var covSLine = new TwoDVectorLine(covPt, spt);
                            var lineCrossS = simpleBlocks.LineCross(covSLine);

                            var b21 = sp1 == Pt2LinePos.Right && sp2 == Pt2LinePos.Left && ep1 == Pt2LinePos.Left &&
                                      ep2 == Pt2LinePos.Right;
                            if (b21 && lineCrossS)
                            {
#if DEBUG
                                Console.Out.WriteLine("line cut to 3 part with a Triangle");
#endif
                                break;
                            }
                        }
                    }

                    break;
                }

                if (pt2LinePos == Pt2LinePos.On)
                {
                    //共线
                    continue;
                }

                if (pt2LinePos != Pt2LinePos.Left) throw new ArgumentOutOfRangeException();
                //凸点
                // inALine = false;
            }

            if (fNoCovPt == null)
            {
                //全是凸点，所以是凸多边形
#if DEBUG
                Console.Out.WriteLine("just a cov ploy");
#endif
                return;
            }

#if DEBUG
            Console.Out.WriteLine("not a cov ploy shape");
#endif

            return;
            TwoDPoint? bNoCovPt = null;
            var areaBackwardCovCount = 0;
            //向后再搜索, 找到可以消除这个凹点的切割方式
            for (var i = covForwardCount + 1; i < Area.Count; i++)
            {
                var blockShape1 = Area[i];
                var shape1 = Area[(i + 1) % Area.Count];


                var endPt = shape1.GetEndPt();

                var p = endPt.GetPosOf(blockShape1.AsTwoDVectorLine());

                if (p == Pt2LinePos.Right)
                {
                    // 发现凹点, 并且形成凹型
                    bNoCovPt = shape1.GetStartPt();
                    areaBackwardCovCount = i + 1;


                    break;
                }

                if (p == Pt2LinePos.On)
                {
                    //共线
                    continue;
                }

                if (p != Pt2LinePos.Left) throw new ArgumentOutOfRangeException();
                //是凸点
                inALine = false;
            }

            if (bNoCovPt == null)
            {
                throw new Exception("no good cut");
            }

            if (fNoCovPt == bNoCovPt)

            {
                //前后点重合，说明只有一个凹点，所以直接切分为两个区域
#if DEBUG
                Console.Out.WriteLine($" shape have only 1 no cov pt {fNoCovPt.Log()} , cut to 2 part");
#endif
                var (covShape, rest) = CutATriangle(Area, covForwardCount);
            }
            else if (inALine)
            {
                //图形是一条线，需要再扩充
#if DEBUG

                Console.Out.WriteLine($"shape is in a line , need bigger area");
#endif
                var twoDVectorLines = Area.Select(x => x.AsTwoDVectorLine());

                var simpleBlocks = new SimpleBlocks(twoDVectorLines);

                var sameLine = new TwoDVectorLine(bNoCovPt, fNoCovPt);
                for (var i = covForwardCount; i < Area.Count; i++)
                {
                    var fJumpPt = Area[i].GetEndPt();
                    //    找一个跳点，在线的左边可以组成凸多边形
                    var fLink = new TwoDVectorLine(fNoCovPt, fJumpPt);
                    if (fJumpPt.GetPosOf(sameLine) != Pt2LinePos.Right &&
                        !simpleBlocks.LineCross(fLink))
                    {
#if DEBUG
                        Console.Out.WriteLine($"find a jump Pt forward {fJumpPt.Log()}");
#endif
                        for (var i1 = areaBackwardCovCount - 2; i1 >= i; i1--)
                        {
                            if (i1 == i)
                            {
#if DEBUG
                                Console.Out.WriteLine("get 2 part and a Triangle");
#endif


                                break;
                            }

                            var bJumpPt = Area[i1].GetEndPt();
                            var bLink = new TwoDVectorLine(bJumpPt, bNoCovPt);
                            if (bJumpPt.GetPosOf(sameLine) != Pt2LinePos.Right &&
                                !simpleBlocks.LineCross(bLink))
                            {
#if DEBUG
                                Console.Out.WriteLine("get 2 link cut to 3 part");
#endif


                                break;
                            }
                        }

                        break;
                    }
                }
            }

            else
            {
#if DEBUG
                Console.Out.WriteLine(
                    $" shape have more than 1 cov pt link::{fNoCovPt.Log()}=={bNoCovPt.Log()} , cut a cov out and rest");
#endif
            }
        }


        // static (List<IBlockShape> part1, List<IBlockShape> part2, List<IBlockShape> triangle) CutAInTriangle(
        //     List<IBlockShape> raw, int fCount)
        // {
        // }

        static (List<IBlockShape> covShape, List<IBlockShape> rest) CutATriangle(List<IBlockShape> raw, int fCount)
        {
            var bCount = fCount + 2;
            var blockShapes = raw.GetRange(0, fCount);
            var shapes = raw.GetRange(bCount, raw.Count - bCount);
            shapes.AddRange(blockShapes);
            var rest = raw.GetRange(fCount, 2);
            return (shapes, rest);
        }

        static (List<IBlockShape> covShape, List<IBlockShape> rest) SplitTo2Part(List<IBlockShape> raw, int fCount,
            int bCount)
        {
            var blockShapes = raw.GetRange(0, fCount);
            var shapes = raw.GetRange(bCount, raw.Count - bCount);
            shapes.AddRange(blockShapes);
            var rest = raw.GetRange(fCount, bCount - fCount);
            return (shapes, rest);
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
                    where blockUnit.ShellBlocks.IsEmpty() || blockUnit.ShellBlocks.PtInShape(twoDPoint)
                    let all = blockUnit.ChildrenBlocks.All(x => !x.PtInShape(twoDPoint))
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