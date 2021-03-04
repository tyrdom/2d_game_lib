using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class WalkMap
    {
        public Dictionary<size, WalkBlock> SizeToEdge { get; }

        public WalkMap(Dictionary<size, WalkBlock> sizeToEdge)
        {
            SizeToEdge = sizeToEdge;
        }

      

        public static WalkMap CreateMapByPolys(List<(Poly, bool)> lp)
        {
            var sizeToR = CommonConfig.Configs.bodys;
            var walkBlocks = new Dictionary<size, WalkBlock>();
            foreach (var keyValuePair in sizeToR)
            {
                var genWalkBlockByPolys = SomeTools.GenWalkBlockByPolygons(lp, keyValuePair.Value.rad, 6);
                walkBlocks[keyValuePair.Key] = genWalkBlockByPolys;
            }

            return new WalkMap(walkBlocks);
        }
    }
}