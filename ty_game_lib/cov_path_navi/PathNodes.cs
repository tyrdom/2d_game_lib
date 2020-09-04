using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace cov_path_navi
{
    public class PathNodes
    {
        private Dictionary<int, PathNodeCovPolygon> PolygonsTop;

        private readonly List<AreaBox> AreaBoxes;

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