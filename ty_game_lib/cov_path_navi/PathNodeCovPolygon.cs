using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;

namespace cov_path_navi
{
    public class PathNodeCovPolygon
    {
        public List<Link> Links { get; }

        public int ThisPathNodeId { get; }

        private List<IBlockShape> Edges { get; }

        public ImmutableDictionary<int, List<(int id, float cost, TwoDVectorLine GoTrough)>>? LinkAndCost
        {
            get;
            private set;
        }

        // public WayFlag WayFlag;
        public PathNodeCovPolygon(List<Link> links, int thisPathNodeId, List<IBlockShape> edges)
        {
            Links = links;
            ThisPathNodeId = thisPathNodeId;
            Edges = edges;
        }

        public TwoDPoint GetCenterPt()
        {
            var twoDPoints = Edges.Select(x => x.GetEndPt()).ToList();
            var sum = twoDPoints.Sum(p => p.X);
            var f = twoDPoints.Sum(p => p.Y);
            var count = twoDPoints.Count;
            return new TwoDPoint(sum / count, f / count);
        }


        public void LoadLinkAndCost()
        {
            LinkAndCost = GenCost(Links);
        }


        private static ImmutableDictionary<int, List<(int id, float cost, TwoDVectorLine GoTrough)>>? GenCost(
            IReadOnlyCollection<Link> list)
        {
            var res = new Dictionary<int, Dictionary<int, (float cost, TwoDVectorLine goThrough)>>();
            foreach (var link in list)
            {
                var thisId = link.LinkToPathNodeId;
                var pt1 = link.GoThrough.GetMid();
                var enumerable = list.Where(x => x.LinkToPathNodeId != thisId).ToList();
#if DEBUG
                var aggregate = enumerable.Aggregate("", (s, x) => s + x.LinkToPathNodeId + "_");
                Console.Out.WriteLine($"{thisId}=>>{aggregate}");
#endif
                foreach (var anotherLink in enumerable)
                {
                    var anotherLinkId = anotherLink.LinkToPathNodeId;
                    var ptAnother = anotherLink.GoThrough.GetMid();
                    var distance = pt1.GetDistance(ptAnother);
                    AddADistance(thisId, anotherLinkId, distance, anotherLink.GoThrough, res);
                    AddADistance(anotherLinkId, thisId, distance, link.GoThrough, res);

                    static void AddADistance(int thisId, int anotherId, float distance, TwoDVectorLine go,
                        IDictionary<int, Dictionary<int, (float cost, TwoDVectorLine goThrough)>> res)
                    {
                        if (res.TryGetValue(thisId, out var aDictionary))
                        {
                            if (!aDictionary.TryGetValue(anotherId, out _))
                            {
                                aDictionary[anotherId] = (distance, go);
                            }
                        }
                        else
                        {
                            var floats = new Dictionary<int, (float cost, TwoDVectorLine goThrough)>
                                {{anotherId, (distance, go)}};
                            res[thisId] = floats;
                        }
                    }
                }
            }

            ImmutableDictionary<int, List<(int id, float cost, TwoDVectorLine GoTrough)>> dictionary = res
                .ToDictionary(
                    x => x.Key,
                    x =>
                        x.Value.Select(xx => (xx.Key, xx.Value.cost, xx.Value.goThrough)).ToList()
                )
                .ToImmutableDictionary();
            return dictionary;
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
            var aggregate = Edges.Aggregate("", (s, x) => s + x.GetEndPt() + "\n");
            var aggregate1 = Links.Aggregate("", (s, x) => s + x + "\n");
            if (LinkAndCost == null) return $"{s1} edges::\n{aggregate} links::\n{aggregate1}";
            {
                var aggregate2 = LinkAndCost.Aggregate("",
                    (s, x) => s + $"\nfrom{x.Key}"
                                + x.Value.Aggregate("", (ss, xx)
                                    => $"\n>{xx.id}==Cost:{xx.cost}==Line{xx.GoTrough}"));
                return $"{s1} edges::\n{aggregate} links::\n{aggregate1} cost::\n{aggregate2}";
            }
        }
    }
}