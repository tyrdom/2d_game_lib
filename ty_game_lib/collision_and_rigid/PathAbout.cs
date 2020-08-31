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
        public ContinuousWalkArea(List<IBlockShape> area, List<Link> links
        )
        {
            Links = links;

            Area = area;
        }

        private List<IBlockShape> Area { get; }

        private List<Link> Links { get; }

        public override string ToString()
        {
            var aggregate = Area.Aggregate("", (s, x) => s + x.Log() + "\n");
            var aggregate2 = Links.Aggregate("", (s, x) => s + x.LinkId + "::" + x.GoThrough.Log() + "\n");

            return $"Area::\n{aggregate} Cuts::\n{aggregate2}";
        }

        public void ToCovPolygons()

        {
            var twoDVectorLines = Area.Select(x => x.AsTwoDVectorLine());

            var simpleBlocks = new SimpleBlocks(twoDVectorLines);

            var blockShape = Area.First();
            //check right
            var shape = Area.Last();
            var b = blockShape.GetStartPt() == shape.GetEndPt();
            var alink = Links.FirstOrDefault(x => x.GoThrough == blockShape);
            if (alink != null)
            {
                Console.Out.WriteLine
                    ($"cuts {alink.LinkId} ::{alink.GoThrough.Log()}");
            }


            if (!b)
            {
                throw new Exception("no good Continuous Area :: not end to start");
            }
#if DEBUG

            Console.Out.WriteLine(" check ok ");
#endif

            var valueTuples = new List<Link>();


            if (alink != null)
            {
                valueTuples.Add(alink);
            }

            TwoDPoint? fEndPt = null;
            (TwoDVectorLine line1, TwoDVectorLine line2)? valueTuple = null;
            //向前搜索
            var covEndAt = -1;
            for (var i = 0; i < Area.Count; i++)
            {
                var blockShape1 = Area[i];
                var twoDVectorLine = blockShape1.AsTwoDVectorLine();

                var shape1 = Area[(i + 1) % Area.Count];
                var endPt = shape1.GetEndPt();
                var asTwoDVectorLine2 = shape1.AsTwoDVectorLine();
                var pt2LinePos = endPt.GetPosOf(twoDVectorLine);

                if (pt2LinePos != Pt2LinePos.Right) continue;
#if DEBUG
                Console.Out.WriteLine("meet no cov edge;");
#endif

                fEndPt = blockShape1.GetEndPt();

                covEndAt = i;

                valueTuple = (twoDVectorLine, asTwoDVectorLine2);

                break;
            }

            if (fEndPt == null || valueTuple == null)
            {
#if DEBUG
                Console.Out.WriteLine("just a cov ploy");
#endif
                return;
            }
#if DEBUG
            Console.Out.WriteLine("not a cov ploy");
#endif

            //向后搜索, 分割凹点
            for (var i = covEndAt + 1; i < Area.Count; i++)
            {
                var startPt = Area[i].GetStartPt();

                var endPt = Area[i].GetEndPt();

                var p1 = endPt.GetPosOf(valueTuple.Value.line1);
                var p2 = endPt.GetPosOf(valueTuple.Value.line2);


                if (p1 == Pt2LinePos.Left && p2 == Pt2LinePos.Left)
                {
                    var twoDVectorLine = new TwoDVectorLine(fEndPt, endPt);
                    var lineCross = simpleBlocks.LineCross(twoDVectorLine);
                    if (!lineCross)
                    {
#if DEBUG
                        Console.Out.WriteLine($"find new cut at {covEndAt} {i} ll {twoDVectorLine.Log()}");
#endif
                        break;
                    }

                    Console.Out.WriteLine(
                        $"fail to find new cut at {covEndAt} {i} ll {twoDVectorLine.Log()}");
                }


                if (p1 == Pt2LinePos.On)
                {
                    var twoDVectorLine = new TwoDVectorLine(fEndPt, endPt);
                    var lineCross = simpleBlocks.LineCross(twoDVectorLine);
                    if (!lineCross)
                    {
#if DEBUG
                        Console.Out.WriteLine($"find new cut at {covEndAt} {i} on {twoDVectorLine.Log()}");
#endif
                        break;
                    }

                    Console.Out.WriteLine(
                        $"fail to find new cut at {covEndAt} {i} ll {twoDVectorLine.Log()}");
                    
                }
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

            var links = new List<Link>();
            if (!ShellRaw.Any())
            {
                throw new Exception("must have some shells");
            }

            if (ChildrenRaw.Count <= 0)
            {
                return new ContinuousWalkArea(ShellRaw, links);
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
                    links.Add(new Link(0, cutMirror));
                    links.Add(new Link(0, cut));

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

            return new ContinuousWalkArea(ShellRaw, links);
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