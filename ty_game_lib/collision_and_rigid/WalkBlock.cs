#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;


namespace collision_and_rigid
{
    public class WalkBlock : IShape
    {
        private float R;
        public QSpace QSpace;


        public WalkBlock(float r, QSpace qSpace)
        {
            R = r;
            QSpace = qSpace;
        }

        public bool InBlock(TwoDPoint p)
        {
            var (item1, aabbBoxShape) = QSpace.TouchWithARightShootPoint(p);

            Console.Out.WriteLine("num::" + item1);

            Console.Out.WriteLine("box::" + aabbBoxShape);
            return (item1 % 2) != 0;
        }

        public TwoDPoint PullInToPt(TwoDPoint lastP, TwoDPoint nowP)
        {
            var inLine = new TwoDVectorLine(nowP, lastP);
            var apt = QSpace.GetSlidePoint(inLine, false);
            return apt ?? lastP;
        }

        public TwoDPoint PushOutToPt(TwoDPoint lastP, TwoDPoint nowP)
        {
            var inLine = new TwoDVectorLine(lastP, nowP);
            var apt = QSpace.GetSlidePoint(inLine, true);
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

        public bool IsTouchAnother(IShape another)
        {
            throw new NotImplementedException();
        }
    }
}