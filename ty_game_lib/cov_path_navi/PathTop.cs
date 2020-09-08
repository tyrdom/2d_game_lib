#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using collision_and_rigid;

namespace cov_path_navi
{
    public class PathTop
    {
        private Dictionary<int, PathNodeCovPolygon> PolygonsTop { get; }


        private IQSpace AreaQSpace { get; }

        public override string ToString()
        {
            var aggregate = PolygonsTop.Aggregate("", (s, x) => s + x.Key + "===" + x.Value + "\n");

            return aggregate;
        }


        public PathTop(WalkBlock walkBlock)
        {
            if (walkBlock.QSpace == null)
            {
                throw new Exception("cant create PathTop");
            }

            var blockShapes = walkBlock.QSpace.GetAllIBlocks().ToList();
            var genFromBlocks = GenFromBlocks(blockShapes);
            var walkAreaBlocks = GenBlockUnits(genFromBlocks, walkBlock.IsBlockIn);
            var continuousWalkAreas = walkAreaBlocks.Select(x => x.GenWalkArea());
            var a = -1;
            var pathNodeCovPolygons = continuousWalkAreas.SelectMany(x => x.ToCovPolygons(ref a)).ToList();
            foreach (var pathNodeCovPolygon in pathNodeCovPolygons)
            {
                pathNodeCovPolygon.LoadLinkAndCost();
            }

            PolygonsTop = pathNodeCovPolygons.ToDictionary(x => x.ThisPathNodeId, x => x);
            var areaBoxes = pathNodeCovPolygons.Select(x => x.GenAreaBox()).ToArray();
            var areaQSpaceByAreaBox = SomeTools.CreateAreaQSpaceByAreaBox(areaBoxes, 6);
            AreaQSpace = areaQSpaceByAreaBox;
        }

        private int? InWhichPoly(TwoDPoint pt)
        {
            var pointInWhichArea = AreaQSpace.PointInWhichArea(pt);

            return pointInWhichArea?.PolyId;
        }

        public List<(int, TwoDVectorLine?)> FindAPathByPoint(TwoDPoint startPt, TwoDPoint endPt, int? startPoly,
            int? endPoly)
        {
            var start = startPoly ?? InWhichPoly(startPt);
            var end = endPoly ?? InWhichPoly(endPt);
            if (start != null && end != null)
            {
                return FindAPathById(start.Value, end.Value, startPt, endPt);
            }

            throw new Exception($"Pt{startPt.Log()} or {endPt.Log()} not in any area  ");
        }


        public static List<TwoDPoint> GetGoPts(TwoDPoint start, TwoDPoint end, List<TwoDVectorLine?> twoDVectorLines)
        {
            var twoDPoints = new List<TwoDPoint>();
            if (!twoDVectorLines.Any())
            {
                return twoDPoints;
            }

            var nowPt = start;
            var leftLines = new List<TwoDVectorLine>();

            var rightLines = new List<TwoDVectorLine>();


            for (var i = 1; i < twoDVectorLines.Count; i++)
            {
                var twoDVectorLine = twoDVectorLines[i];
                if (twoDVectorLine == null)
                {
                    continue;
                }

                var rightPt = twoDVectorLine.GetStartPt();
                var leftPt = twoDVectorLine.GetEndPt();

                //getNextPt;

                if (i > 1)
                {
                    var cutLl = 0;
                    for (var i1 = leftLines.Count - 1; i1 >= 0; i1--)
                    {
                        var nowLeftLine = leftLines[i1];
                        if (rightPt.GetPosOf(nowLeftLine) != Pt2LinePos.Left) continue;
#if DEBUG
                        Console.Out.WriteLine("right pt is over left");
#endif
                        cutLl = i1 + 1;
                        // var before = leftLines.GetRange(0, i1 + 1);
                        // var after = leftLines.GetRange(i1 + 1, leftLines.Count - i1 - 1);
                        break;
                    }

                    if (cutLl > 0)
                    {
                        var before = leftLines.GetRange(0, cutLl);
                        var after = leftLines.GetRange(cutLl, leftLines.Count - cutLl);
                        var dPoints = before.Select(x => x.GetEndPt());
                        leftLines = after;

                        twoDPoints.AddRange(dPoints);
                        var twoDPoint = twoDPoints.Last();
                        rightLines = new List<TwoDVectorLine> {new TwoDVectorLine(twoDPoint, rightPt)};
                        continue;
                    }

                    if (rightPt == rightLines.Last().GetEndPt())
                    {
                        var cutRr = -1;
                        for (var i1 = rightLines.Count - 1; i1 >= 0; i1--)
                        {
                            var rightLine = rightLines[i1];
                            if (rightPt.GetPosOf(rightLine) != Pt2LinePos.Right)
                            {
                                cutRr = i1;
                            }
                        }

                        if (cutRr >= 0)
                        {
                            var rest = rightLines.GetRange(0, cutRr);

                            var dVectorLine = rightLines[cutRr];
                            var vectorLine = new TwoDVectorLine(dVectorLine.GetStartPt(), rightPt);
                            rest.Add(vectorLine);
                            rightLines = rest;
                        }
                        else
                        {
                            var twoDPoint = rightLines.Last().GetEndPt();
                            rightLines.Add(new TwoDVectorLine(twoDPoint, rightPt));
                        }
                    }
                    //TODO LeftSide
                    
                }

                leftLines.Add(new TwoDVectorLine(start, twoDVectorLine.GetEndPt()));

                rightLines.Add(new TwoDVectorLine(start, twoDVectorLine.GetStartPt()));
            }

            twoDPoints.Add(end);
            return twoDPoints;
        }

        public List<(int, TwoDVectorLine?)> FindAPathById(int start, int end, TwoDPoint? startPt = null,
            TwoDPoint? endPt = null)
        {
            if (start == end)
            {
#if DEBUG
                Console.Out.WriteLine("in same poly just go");
#endif
                return new List<(int, TwoDVectorLine?)> {(start, null)};
            }

            var pathTreeNode = new PathTreeNode(start, 0);
            var pathTreeNodes = new List<PathTreeNode>();

            var treeNodes = new Dictionary<int, PathTreeNode> {{start, pathTreeNode}};


            pathTreeNode.GrowFromRoot(PolygonsTop, end, pathTreeNodes, treeNodes, startPt, endPt);

            if (!pathTreeNodes.Any())
            {
#if DEBUG
                Console.Out.WriteLine($"no way between {start} and {end}");
#endif

                return new List<(int, TwoDVectorLine?)>();
            }


#if DEBUG
            var aggregate = pathTreeNodes.Aggregate("", (s, x) =>
            {
                var ints = x.LogPath();

                return s + ints + $"cost:{x.GetTotalCost()}\n";
            });
            Console.Out.WriteLine($"paths are \n{aggregate}");
#endif
            pathTreeNodes.Sort((x, y) =>
            {
                var xCost = x.GetTotalCost();
                var yCost = y.GetTotalCost();
                if (xCost > yCost) return 1;
                if (xCost < yCost) return -1;
                return 0;
            });

            var firstOrDefault = pathTreeNodes.FirstOrDefault();

            var path = firstOrDefault.GetPath();
            return path;
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
                    )
                    {
                        tList.Add(blockShape);
                        if (endPt == lstPt
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