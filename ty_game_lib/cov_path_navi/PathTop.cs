#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using collision_and_rigid;

namespace cov_path_navi
{
    public interface IPathTree
    {
        public int Id { get; }
    }

    public class PathTreeNode : IPathTree
    {
        private PathTreeNode? Father { get; set; }
        public int Id { get; }
        public float Cost { get; private set; }

        private List<PathTreeNode> ToList;

        public PathTreeNode(int id, float cost)
        {
            Id = id;
            Cost = cost;
            Father = null;
            ToList = new List<PathTreeNode>();
        }

        private PathTreeNode(int id, float cost, PathTreeNode? pathTreeNode)
        {
            Id = id;
            Cost = cost;
            Father = pathTreeNode;
            ToList = new List<PathTreeNode>();
        }


        public override string ToString()
        {
            var fatherId = Father == null ? "root" : Father.Id.ToString();
            var id = fatherId + "=>" + Id + "<Cost::" + Cost;
            return id;
        }

        public string LogPath()
        {
            var path = GetPath2();

            return path.Aggregate("", (x, s) => $"{x}=>{s.Item1}[Cost::{s.Item2}]");
        }

        public void GrowFromRoot
        (Dictionary<int, PathNodeCovPolygon> polygonsTop, int end, List<PathTreeNode> collector,
            Dictionary<int, PathTreeNode> haveReached, TwoDPoint? startPt = null, TwoDPoint? endPoint = null)
        {
            if (!polygonsTop.TryGetValue(Id, out var nodeCovPolygon)) throw new Exception($"not such polygon id{Id}");
            if (Father != null)
            {
                throw new Exception($"not a root node~~~ {Father.Id}");
            }

            var treeNodes = nodeCovPolygon.Links.Select(x =>
                {
                    var distance = startPt == null ? 0 : x.GoThrough.GetMid().GetDistance(startPt);

                    var pathTreeNode = new PathTreeNode(x.LinkToPathNodeId, distance, this);
                    return pathTreeNode;
                })
                .ToList();
            var okNodes = treeNodes.Where(x => x.Id == end).ToList();
#if DEBUG
            Console.Out.WriteLine($"root node is {Id}");
            var aggregate = treeNodes.Aggregate("", (s, x) => s + x + "\n");
            Console.Out.WriteLine($"grow :::\n{aggregate}");
#endif
            if (okNodes.Any())
            {
                collector.AddRange(okNodes);
                return;
            }

            foreach (var pathTreeNode in treeNodes)
            {
                haveReached[pathTreeNode.Id] = pathTreeNode;
                ToList.Add(pathTreeNode);
                pathTreeNode.GrowTree(polygonsTop, end, collector, haveReached, endPoint);
            }
        }

        public float GetTotalCost()
        {
            return GatherCost(0);
        }

        private float GatherCost(float nowCost)
        {
            return Father?.GatherCost(nowCost + Cost) ?? nowCost;
        }

        public List<(int, float)> GetPath2()
        {
            var ints = new List<(int, float)>();
            GatherPathIdsAndCost(ints);
            ints.Reverse();
            return ints;
        }

        private void GatherPathIdsAndCost(List<(int, float)> ints)
        {
            ints.Add((Id, Cost));
            Father?.GatherPathIdsAndCost(ints);
        }

        public List<int> GetPath()
        {
            var ints = new List<int>();
            GatherPathIds(ints);
            ints.Reverse();
            return ints;
        }

        private void GatherPathIds(List<int> ints)
        {
            ints.Add(Id);

            Father?.GatherPathIds(ints);
        }

        public void GrowTree(Dictionary<int, PathNodeCovPolygon> polygonsTop, int end, List<PathTreeNode> collector,
            Dictionary<int, PathTreeNode> haveReached, TwoDPoint? endPt = null)
        {
            if (!polygonsTop.TryGetValue(Id, out var nodeCovPolygon)) throw new Exception($"not such polygon id{Id}");
            if (Father == null) throw new Exception($"this a root Node:: {Id}");

            {
                if (!nodeCovPolygon.LinkAndCost.TryGetValue(Father.Id, out var tuples))
                    throw new Exception($"not such link:{Father.Id} in poly:{Id} ");

                var treeNodes = tuples.Select(x =>
                    new PathTreeNode(x.id, x.cost, this)).ToList();

                var okNodes = treeNodes.Where(x => x.Id == end).ToList();
#if DEBUG
                Console.Out.WriteLine($"this node is {Id} form {Father.Id}");
                var aggregate = treeNodes.Aggregate("", (s, x) => s + x + "\n");
                Console.Out.WriteLine($"grow :::\n{aggregate}");
#endif
                if (okNodes.Any())
                {
                    if (polygonsTop.TryGetValue(end, out var endCovPolygon) && endPt != null)
                    {
                        foreach (var pathTreeNode in okNodes)
                        {
                            var firstOrDefault = endCovPolygon.Links.FirstOrDefault(x => x.LinkToPathNodeId == Id);
                            var distance = firstOrDefault.GoThrough.GetMid().GetDistance(endPt);

                            pathTreeNode.Cost += distance;
                        }
                    }


                    collector.AddRange(okNodes);
                    return;
                }


                foreach (var pathTreeNode in treeNodes)
                {
                    if (haveReached.TryGetValue(pathTreeNode.Id, out var pathTreeNode2))
                    {
                        foreach (var aTreeNode in pathTreeNode2.ToList)
                        {
                            if (!polygonsTop.TryGetValue(pathTreeNode.Id, out var polygon) ||
                                !polygon.LinkAndCost.TryGetValue(Id, out var aTuples))
                                throw new Exception($"no poly{pathTreeNode.Id} in polyTop or no {Id} in links");
                            var tuple = aTuples.FirstOrDefault(x => x.id == aTreeNode.Id);
                            var cost = tuple.cost;
#if DEBUG
                            Console.Out.WriteLine(
                                $"pathNode\n {pathTreeNode.LogPath()}\n is reach\n {pathTreeNode2.LogPath()} \ncost : {pathTreeNode.GetTotalCost() + cost} vs {pathTreeNode2.GetTotalCost() + aTreeNode.Cost}");
#endif

                            var b = pathTreeNode.GetTotalCost() + cost <
                                    pathTreeNode2.GetTotalCost() + aTreeNode.Cost;
                            if (!b) continue;
                            pathTreeNode2.Father = pathTreeNode.Father;
                            pathTreeNode2.Cost = pathTreeNode.Cost;
                            aTreeNode.Cost = cost;
                        }


                        continue;
                    }

                    ToList.Add(pathTreeNode);
                    haveReached[pathTreeNode.Id] = pathTreeNode;
                    pathTreeNode.GrowTree(polygonsTop, end, collector, haveReached);
                }
            }
        }
    }


    public class PathTop
    {
        private readonly Dictionary<int, PathNodeCovPolygon> PolygonsTop;

        private readonly List<AreaBox> AreaBoxes;

        public override string ToString()
        {
            var aggregate = PolygonsTop.Aggregate("", (s, x) => s + x.Key + "===" + x.Value + "\n");

            return aggregate;
        }


        public PathTop(WalkBlock walkBlock)
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
                foreach (var pathNodeCovPolygon in pathNodeCovPolygons)
                {
                    pathNodeCovPolygon.LoadLinkAndCost();
                }

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


        public List<int> FindAPathById(int start, int end, TwoDPoint? startPt = null, TwoDPoint? endPt = null)
        {
            var pathTreeNode = new PathTreeNode(start, 0);
            var pathTreeNodes = new List<PathTreeNode>();
            var treeNodes = new Dictionary<int, PathTreeNode> {{start, pathTreeNode}};
            pathTreeNode.GrowFromRoot(PolygonsTop, end, pathTreeNodes, treeNodes, startPt, endPt);

            if (!pathTreeNodes.Any())
            {
#if DEBUG
                Console.Out.WriteLine($"no way between {start} and {end}");
#endif

                return new List<int>();
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