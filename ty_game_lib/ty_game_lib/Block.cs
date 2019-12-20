#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;


namespace ty_game_lib
{
    public class Block : Shape
    {
        private float R;
        private QSpace qSpace;
        private AabbBoxShape[] BlockElements;


        public Block(float r, AabbBoxShape[] blockElements)
        {
            R = r;
            BlockElements = blockElements;
        }

        bool inBlock(TwoDPoint p)
        {
            int crossCount = 0;
            foreach (var aabbBoxShape in BlockElements)
            {
                var zone = aabbBoxShape.Zone;
            }

            return false;
        }


        public AabbBoxShape CovToAabbPackBox()
        {
            var foo = SomeTools.JoinAabbZone(BlockElements);

            return new AabbBoxShape(foo, this);
        }
    }
}