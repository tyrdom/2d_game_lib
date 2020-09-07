using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace cov_path_navi
{
    public class PathTreeNode
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
                if (nodeCovPolygon.LinkAndCost == null ||
                    !nodeCovPolygon.LinkAndCost.TryGetValue(Father.Id, out var tuples))
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
                        if (pathTreeNode.Father == pathTreeNode2.Father)
                        {
#if DEBUG
                            Console.Out.WriteLine(
                                $"pathNode\n {pathTreeNode.LogPath()}\n is reach\n {pathTreeNode2.LogPath()} \n in same Father {pathTreeNode2.Father?.Id}");
#endif
                            continue;
                        }

                        foreach (var aTreeNode in pathTreeNode2.ToList)
                        {
                            if (!polygonsTop.TryGetValue(pathTreeNode.Id, out var polygon) ||
                                polygon.LinkAndCost == null ||
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
                            if (!b)
                            {
                                Console.Out.WriteLine
                                    ($"find a new way but not short than before {pathTreeNode2.LogPath()}");
                                continue;
                            }

                            pathTreeNode2.Father = pathTreeNode.Father;
                            pathTreeNode2.Cost = pathTreeNode.Cost;
                            aTreeNode.Cost = cost;

#if DEBUG
                            Console.Out.WriteLine(
                                $"find a new short cut {pathTreeNode.LogPath()}");
#endif

                            pathTreeNode.GrowTree(polygonsTop, end, collector, haveReached);
                        }


                        continue;
                    }

                    ToList.Add(pathTreeNode);
                    haveReached[pathTreeNode.Id] = pathTreeNode;
#if DEBUG
                    Console.Out.WriteLine(
                        $"new reach area {pathTreeNode.Id}");
#endif
                    pathTreeNode.GrowTree(polygonsTop, end, collector, haveReached);
                }
            }
        }
    }
}