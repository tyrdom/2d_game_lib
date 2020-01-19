using System.Collections.Generic;
using System.Drawing;
using System.Net;
using collision_and_rigid;

namespace game_stuff
{
    public class WalkMap
    {
        private Dictionary<BodySize, WalkBlock> SizeToEdge;

        public WalkMap(Dictionary<BodySize, WalkBlock> sizeToEdge)
        {
            SizeToEdge = sizeToEdge;
        }

        public static WalkMap CreateMapByPolys(List<(Poly, bool)> lp)
        {
            Dictionary<BodySize, float> sizeToR = TempConfig.SizeToR;
            var walkBlocks = new Dictionary<BodySize, WalkBlock>();
            foreach (KeyValuePair<BodySize, float> keyValuePair in sizeToR)
            {
                var genWalkBlockByPolys = SomeTools.GenWalkBlockByPolys(lp, keyValuePair.Value);
                walkBlocks[keyValuePair.Key] = genWalkBlockByPolys;
            }

            return new WalkMap(walkBlocks);
        }
    }
}