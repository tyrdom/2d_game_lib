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
        private AabbBoxShape[] BlockElements;
        

        public Block(float r, AabbBoxShape[] blockElements)
        {
            R = r;
            BlockElements = blockElements;
        }

        bool inBlock(TwoDPoint p)
        {
            
            return false;
        }

        

        public AabbBoxShape CovToAabbPackBox()
        {
            var foo = BlockElements[0].Zone;
            foreach (var i in Enumerable.Range(1, BlockElements.Length))
            {
                foo = foo.Join(BlockElements[i].Zone);
            }

            return new AabbBoxShape(foo, this);
        }
    }
}