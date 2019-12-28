#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;


namespace ty_game_lib
{
    public class Block : IShape
    {
        private float R;
        public QSpace QSpace;


        public Block(float r, QSpace qSpace)
        {
            R = r;
            QSpace = qSpace;
        }

        public bool InBlock(TwoDPoint p)
        {
            var (item1, aabbBoxShape) = QSpace.TouchWithARightShootPoint(p);
            
//            Console.Out.WriteLine("num::" + item1);

            return (item1 % 2) != 0;
        }

        public TwoDPoint PushOutToPt(TwoDPoint lastP, TwoDPoint nowP)
        {
            var inLine = new TwoDVectorLine(lastP, nowP);
            var covToAabbPackBox = inLine.CovToAabbPackBox();
            var apt = QSpace.GetSlidePoint(covToAabbPackBox);
            return apt ?? lastP;
        }

        public AabbBoxShape CovToAabbPackBox()
        {
            var foo = QSpace.Zone;

            return new AabbBoxShape(foo, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            var touchWithARightShootPoint = QSpace.TouchWithARightShootPoint(p);
            return touchWithARightShootPoint.Item1;
        }
    }
}