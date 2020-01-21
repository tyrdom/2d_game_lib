#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;


namespace collision_and_rigid
{
    public class WalkBlock : IShape
    {
//        private float R;
        public bool IsBlockIn;
        public QSpace? QSpace;


        public WalkBlock(bool isBlockIn, QSpace? qSpace)
        {
            IsBlockIn = isBlockIn;
            QSpace = qSpace;
        }

        public bool CoverPoint(TwoDPoint p)
        {
            if (QSpace == null)
            {
                return true;
            }

            var (item1, aabbBoxShape) = QSpace.TouchWithARightShootPoint(p);

//            Console.Out.WriteLine("num::" + item1);

//            Console.Out.WriteLine("box::" + SomeTools.ZoneLog(aabbBoxShape.Zone));
            if (item1 >= 0)
            {
                var inBlock = (item1 % 2) != 0;
                return IsBlockIn ? inBlock : !inBlock;
            }

            return item1 == -1;
        }

        public TwoDPoint PullInToPt(TwoDPoint lastP, TwoDPoint nowP)
        {
            var inLine = new TwoDVectorLine(nowP, lastP);
            var apt = QSpace.GetSlidePoint(inLine, false);
            return apt ?? lastP;
        }

        public TwoDPoint PushOutToPt(TwoDPoint lastP, TwoDPoint nowP, bool safe = true)
        {
            var inLine = new TwoDVectorLine(lastP, nowP);
            var apt = QSpace.GetSlidePoint(inLine, true, safe);

            if (safe)
            {
                return apt ?? lastP;
            }

            if (CoverPoint(apt))
            {
                return PushOutToPt(lastP, apt, false);
            }

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

//
//        public TryToUnityAnotherBlock(WalkBlock anotherWalkBlock)
//        {
//            
//        }
    }
}