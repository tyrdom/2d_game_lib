using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using collision_and_rigid;

namespace cov_path_navi
{
    public class PathNodeCovPolygon
    {
        public List<Link> Links;

        public int ThisPathNodeId;

        public List<IBlockShape> Edges;

        public ImmutableDictionary<int, List<(int id, float cost)>> LinkAndCost;

        // public WayFlag WayFlag;
        public PathNodeCovPolygon(List<Link> links, int thisPathNodeId, List<IBlockShape> edges)
        {
            Links = links;
            ThisPathNodeId = thisPathNodeId;
            Edges = edges;
        }

        public void LoadLinkAndCost()
        {
            LinkAndCost = GenCost(Links);
        }


        public static ImmutableDictionary<int, List<(int Key, float Value)>> GenCost(List<Link> list)
        {
            var res = new Dictionary<int, Dictionary<int, float>>();
            foreach (var link in list)
            {
                var thisId = link.LinkToPathNodeId;
                var pt1 = link.GoThrough.GetMid();
                var enumerable = list.Where(x => x.LinkToPathNodeId != thisId).ToList();
#if DEBUG
                var aggregate = enumerable.Aggregate("", (s, x) => s + "_" + x.LinkToPathNodeId);
                Console.Out.WriteLine($"{thisId}=>>{aggregate}");
#endif
                foreach (var anotherLink in enumerable)
                {
                    var anotherLinkId = anotherLink.LinkToPathNodeId;
                    var ptAnother = anotherLink.GoThrough.GetMid();
                    var distance = pt1.GetDistance(ptAnother);
                    AddADistance(thisId, anotherLinkId, distance, res);
                    AddADistance(anotherLinkId, thisId, distance, res);

                    static void AddADistance(int thisId, int anotherId, float distance,
                        IDictionary<int, Dictionary<int, float>> res)
                    {
                        if (res.TryGetValue(thisId, out var aDictionary))
                        {
                            if (!aDictionary.TryGetValue(anotherId, out _))
                            {
                                aDictionary[anotherId] = distance;
                            }
                        }
                        else
                        {
                            var floats = new Dictionary<int, float> {{anotherId, distance}};
                            res[thisId] = floats;
                        }
                    }
                }
            }

            var dictionary = res.ToDictionary(x => x.Key,
                    x => { return x.Value.Select(keyValuePair => (keyValuePair.Key, keyValuePair.Value)).ToList(); })
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
            var aggregate = Edges.Aggregate("", (s, x) => s + x.Log() + "\n");
            var aggregate1 = Links.Aggregate("", (s, x) => s + x + "\n");
            if (LinkAndCost != null)
            {
                var aggregate2 = LinkAndCost.Aggregate("",
                    (s, x) => s + $"\nfrom{x.Key}"
                                + x.Value.Aggregate("", (ss, xx)
                                    => $"\n>{xx.id}=={xx.cost}"));
                return $"{s1} edges::\n{aggregate} links::\n{aggregate1} cost::\n{aggregate2}";
            }

            return $"{s1} edges::\n{aggregate} links::\n{aggregate1}";
        }
    }

    public class WayFlag
    {
    }
}