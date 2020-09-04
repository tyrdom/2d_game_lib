using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace cov_path_navi
{
    public class PathNodeCovPolygon
    {
        public List<Link> Links;

        public int ThisPathNodeId;

        public List<IBlockShape> Edges;

        public PathNodeCovPolygon(List<Link> links, int thisPathNodeId, List<IBlockShape> edges)
        {
            Links = links;
            ThisPathNodeId = thisPathNodeId;
            Edges = edges;
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

            return $"{s1} edges::\n{aggregate} links::\n{aggregate1}";
        }
    }
}