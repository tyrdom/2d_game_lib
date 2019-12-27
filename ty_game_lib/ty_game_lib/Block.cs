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
        public QSpace QSpace;


        public Block(float r, QSpace qSpace)
        {
            R = r;
            QSpace = qSpace;
        }

        public bool inBlock(TwoDPoint p)
        {
                   var (item1, aabbBoxShape) = QSpace.touchWithARightShootPoint(p);
                   Console.Out.WriteLine("num::" + item1 );
                   
                   return (item1 % 2) != 0;
               }
       
              
        public AabbBoxShape CovToAabbPackBox()
        {
            var foo = QSpace.Zone;

            return new AabbBoxShape(foo, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            var touchWithARightShootPoint = QSpace.touchWithARightShootPoint(p);
            return touchWithARightShootPoint.Item1;
        }
    }
}