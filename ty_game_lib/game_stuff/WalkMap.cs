using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class WalkMap
    {
        public Dictionary<BodySize, WalkBlock> SizeToEdge { get; }

        public WalkMap(Dictionary<BodySize, WalkBlock> sizeToEdge)
        {
            SizeToEdge = sizeToEdge;
        }

        public static WalkMap CreateMapByPolys(List<(Poly, bool)> lp)
        {
            var sizeToR = LocalConfig.SizeToR;
            var walkBlocks = new Dictionary<BodySize, WalkBlock>();
            foreach (var keyValuePair in sizeToR)
            {
                var genWalkBlockByPolys = SomeTools.GenWalkBlockByPolygons(lp, keyValuePair.Value, 6);
                walkBlocks[keyValuePair.Key] = genWalkBlockByPolys;
            }

            return new WalkMap(walkBlocks);
        }
    }
}