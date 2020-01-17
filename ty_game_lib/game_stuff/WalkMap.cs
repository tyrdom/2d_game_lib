using System.Collections.Generic;
using System.Drawing;
using collision_and_rigid;

namespace game_stuff
{
    public class WalkMap
    {
        private Dictionary<BodySize, QSpace> SizeToBlocks;
        private Dictionary<BodySize, WalkBlock> SizeToEdge;

        public WalkMap(Dictionary<BodySize, QSpace> sizeToBlocks, Dictionary<BodySize, WalkBlock> sizeToEdge)
        {
            SizeToBlocks = sizeToBlocks;
            SizeToEdge = sizeToEdge;
        }
        
        
    }
}