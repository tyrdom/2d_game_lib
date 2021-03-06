#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

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

        public int GetPolyCount()
        {
            return PolygonsTop.Count;
        }


        public List<TwoDPoint> GetPatrolPts(Random random, int polyNum)
        {
            var pathNodeCovPolygons = PolygonsTop.Values.ToArray();
            if (!pathNodeCovPolygons.Any()) return new List<TwoDPoint>();
            var pathNodeCovPolygon = pathNodeCovPolygons.ChooseRandOne(random);

            var twoDPoints = new List<TwoDPoint> {pathNodeCovPolygon.GetCenterPt()};
            for (var i = 0; i < polyNum; i++)
            {
                var any = pathNodeCovPolygon.Links.Any();
                if (any)
                {
                    var chooseRandOne = pathNodeCovPolygon.Links.ChooseRandOne(random);
                    var twoDPoint = chooseRandOne.GoThrough.GetMid();
                    twoDPoints.Add(twoDPoint);
                    pathNodeCovPolygon = PolygonsTop[chooseRandOne.LinkToPathNodeId];

                    twoDPoints.Add(pathNodeCovPolygon.GetCenterPt());
                    continue;
                }

                break;
            }

            return twoDPoints;
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
#if DEBUG
            Console.Out.WriteLine($"pathCovS ok");
#endif
            foreach (var pathNodeCovPolygon in pathNodeCovPolygons)
            {
                pathNodeCovPolygon.LoadLinkAndCost();
            }
#if DEBUG
            Console.Out.WriteLine($"pathCovS LoadLink ok");
#endif
            PolygonsTop = pathNodeCovPolygons.ToDictionary(x => x.ThisPathNodeId, x => x);
#if DEBUG
            Console.Out.WriteLine($"pathCovS Top ok");
#endif
            var areaBoxes = pathNodeCovPolygons.Select(x => x.GenAreaBox()).ToArray();
#if DEBUG
            Console.Out.WriteLine($"pathCovS Box ok");
#endif
            var areaQSpaceByAreaBox = SomeTools.CreateAreaQSpaceByAreaBox(areaBoxes, 5);

            AreaQSpace = areaQSpaceByAreaBox;

#if DEBUG
            Console.Out.WriteLine($"pathTop Ok have {areaBoxes.Length} area");
#endif
        }

        public int? InWhichPoly(TwoDPoint pt)
        {
            var pointInWhichArea = AreaQSpace.PointInWhichArea(pt);

            return pointInWhichArea?.PolyId;
        }

        public List<(int polyId, TwoDVectorLine? gothroughLine)> FindAPathByPoint(TwoDPoint startPt,
            TwoDPoint endPt,
            int? startPoly = null,
            int? endPoly = null)
        {
            var start = startPoly ?? InWhichPoly(startPt);
            var end = endPoly ?? InWhichPoly(endPt);
            if (start != null && end != null)
            {
                return FindAPathById(start.Value, end.Value, startPt, endPt);
            }

            Console.Out.WriteLine($"Pt{startPt} or {endPt} not in any area  ");
            return new List<(int, TwoDVectorLine?)>();
        }

        public IEnumerable<TwoDPoint> FindGoPts(TwoDPoint startPt, TwoDPoint endPt, int? startPoly = null,
            int? endPoly = null)
        {
            var findAPathByPoint = FindAPathByPoint(startPt, endPt, startPoly, endPoly);
            var aPathByPoint =
                findAPathByPoint.ToArray();
            if (!aPathByPoint.Any())
            {
                return new TwoDPoint[] { };
            }

            var twoDVectorLines = aPathByPoint
                .Where(x => x.gothroughLine != null)
                .Select(x => x.gothroughLine!);

            return GetGoPts(startPt, endPt, twoDVectorLines.ToArray());
        }

        public static IEnumerable<TwoDPoint> GetGoPts(TwoDPoint start, TwoDPoint end,
            TwoDVectorLine[] twoDVectorLines)
        {
            var twoDPoints = new List<TwoDPoint>();
            if (!twoDVectorLines.Any())
            {
                twoDPoints.Add(end);
                return twoDPoints;
            }

            var vectorLine = twoDVectorLines.First();
            var rightSidePt = vectorLine.GetStartPt();
            var leftSidePt = vectorLine.GetEndPt();
            var localPt = start;

            for (var i = 1; i < twoDVectorLines.Length; i++)
            {
                var twoDVectorLine = twoDVectorLines[i];

                var rightPt = twoDVectorLine.GetStartPt();
                var leftPt = twoDVectorLine.GetEndPt();

                //getNextPt;
                var lLine = new TwoDVectorLine(localPt, leftSidePt);
                var rLine = new TwoDVectorLine(localPt, rightSidePt);
                var b = rightPt.GetPosOf(lLine) == Pt2LinePos.Left;
                if (b)
                {
                    //右方点越过左射线当前点为新的起点，并加入路径点
                    twoDPoints.Add(leftSidePt);
                    localPt = leftSidePt;
                    leftSidePt = leftPt;
                    rightSidePt = rightPt;
                    continue;
                }

                var b1 = leftPt.GetPosOf(rLine) == Pt2LinePos.Right;
                if (b1)
                {
                    //左方点越过右边点
                    twoDPoints.Add(rightSidePt);
                    localPt = rightSidePt;
                    leftSidePt = leftPt;
                    rightSidePt = rightPt;
                    continue;
                }

                //到这里说明都没转弯，比较夹角有没有收起
                var b2 = leftPt.GetPosOf(lLine) != Pt2LinePos.Left;
                if (b2)
                {
                    //左点不在左线的左边，更新左点
                    leftSidePt = leftPt;
                }

                var b3 = rightPt.GetPosOf(rLine) != Pt2LinePos.Right;
                if (b3)
                {
                    //右点不在右线的右边，更新右点
                    rightSidePt = rightPt;
                }
            }

            twoDPoints.Add(end);
            return twoDPoints;
        }

        private static void CheckSameSide(TwoDPoint point, List<TwoDVectorLine> thisLinesCollector, Pt2LinePos pos)
        {
            var cutRr = -1;
            for (var i1 = thisLinesCollector.Count - 1; i1 >= 0; i1--)
            {
                var line = thisLinesCollector[i1];
                if (point.GetPosOf(line) != pos)
                {
                    cutRr = i1;
                }
            }

            if (cutRr >= 0)
            {
                var startPt = thisLinesCollector[cutRr].GetStartPt();
                thisLinesCollector.RemoveRange(cutRr, thisLinesCollector.Count - cutRr);
                thisLinesCollector.Add(new TwoDVectorLine(startPt, point));
#if DEBUG
                    var aggregate = thisLinesCollector.Aggregate("", (s, x) => s + x.ToString() + "\n");
                    Console.Out.WriteLine($"get direct link at {pos} :: \n{aggregate}");
#endif
                return;
            }

            var twoDPoint = thisLinesCollector.Last().GetEndPt();
            thisLinesCollector.Add(new TwoDVectorLine(twoDPoint, point));
        }

        private static (List<TwoDVectorLine> opLines, List<TwoDVectorLine> thisLines, IEnumerable<TwoDPoint> addPoints)?
            CheckOpSide(TwoDPoint point, List<TwoDVectorLine> rawOpLines, Pt2LinePos pos)
        {
            var cutCount = -1;
            for (var i1 = rawOpLines.Count - 1; i1 >= 0; i1--)
            {
                var a = rawOpLines[i1];
#if DEBUG
                    Console.Out.WriteLine($"  {point.GetPosOf(a)} vs_vs {pos}");
#endif
                if (point.GetPosOf(a) != pos) continue;
                cutCount = i1 + 1;
                break;
            }

            if (cutCount < 0) return null;
#if DEBUG
                Console.Out.WriteLine($" pt is over {pos},turn {pos}");
#endif
            var before = rawOpLines.GetRange(0, cutCount);
            var opLines = rawOpLines.GetRange(cutCount, rawOpLines.Count - cutCount);
            var addPoints = before.Select(x => x.GetEndPt());

            var twoDPoints = addPoints.ToList();
            var twoDPoint = twoDPoints.Last();

            var thisLines = new List<TwoDVectorLine> {new TwoDVectorLine(twoDPoint, point)};
            return (opLines, thisLines, twoDPoints);
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

            if (firstOrDefault == null) return new List<(int, TwoDVectorLine?)>();
            var path = firstOrDefault.GetPath();
            return path;
        }


        //分割为凸多边形，标记联通关系
        //合并阻挡 去除环结构
        public static IEnumerable<WalkAreaBlock> GenBlockUnits(List<List<IBlockShape>> bs, bool isBlockIn)
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

            var genFromBlocks = GenLinkLists(res, rawList);
            if (CheckSamePt(genFromBlocks))
                return genFromBlocks;
            throw new ArgumentException("there is SamePt In Blocks! Move Some Pt");

            static List<List<IBlockShape>> GenLinkLists(List<List<IBlockShape>> res, List<IBlockShape> rList)
            {
                if (rList.Count <= 0) return res;
                var (lList, nrList) = GenAListFromBlocks(rList);
                if (lList.Count > 0)
                {
                    res.Add(lList);
                    return GenLinkLists(res, nrList);
                }

                var aggregate = rList.Aggregate("", (s, x) => s + x.ToString());
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
                var s1 = tList.Aggregate("", (s, x) => s + x.ToString());
                var (stPt, edPt, lList, rList) = SepARawList(lstPt, ledPt, tList, rawList);
                if (stPt == null || edPt == null)
                {
                    return (lList, rList);
                }

                if (lList.Count > beforeCount) return SepRawList(stPt, edPt, lList, rList);

                var aggregate = lList.Aggregate("l::", (s, x) => s + x.ToString());
                var aggregate1 = rList.Aggregate("r::", (s, x) => s + x.ToString());
                throw new Exception(
                    $"can not finish a block list::num {beforeCount} vs {lList.Count} from {rList.Count} ::\n::{edPt}::\n {s1} vs {aggregate} \n from ::\n{aggregate1}");
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
                    Console.Out.WriteLine($" finding new ed  {edPt} vs {startPt}");
#endif
                    if (startPt == edPt
                    )
                    {
                        tList.Add(blockShape);
                        if (endPt == lstPt
                        )
                        {
#if DEBUG
                            Console.Out.WriteLine($" found finish {endPt} vs {lstPt}");
#endif

                            isEnd = true;
                            continue;
                        }

#if DEBUG
                        find = true;
                        Console.Out.WriteLine($" found new ed  {edPt} vs {startPt}");
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
                var aggregate = rawList.Aggregate("", (s, x) => s + x.ToString());
                Console.Out.WriteLine($" not found {edPt} in {aggregate}");
#endif
                return isEnd ? (null, null, tList, newRaw)! : (lstPt, edPt, tList, newRaw);
            }
        }

        private static bool CheckSamePt(IReadOnlyCollection<List<IBlockShape>> genFromBlocks)
        {
            return (from genFromBlock in genFromBlocks
                    let enumerable = genFromBlocks.Where(x => x != genFromBlock)
                    let twoDPoints = enumerable.SelectMany(x => x.Select(b => b.GetEndPt()))
                    select genFromBlock.Any(x => twoDPoints.Any(xx =>
                    {
                        var asTwoDVectorLine = x.AsTwoDVectorLine();
                        var scaleInPt = asTwoDVectorLine.GetScaleInPt(xx);
                        var b = scaleInPt <= 1 && scaleInPt >= 0;
                        return b && xx.GetPosOf(asTwoDVectorLine) == Pt2LinePos.On;
                    })))
                .All(any => !any);
        }
    }
}