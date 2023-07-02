using System;

#nullable enable
namespace collision_and_rigid
{
    public class WalkBlock
    {
//        private float R;
        public bool IsBlockIn { get; }
        public IQSpace QSpace { get; }


        public WalkBlock(bool isBlockIn, IQSpace qSpace)
        {
            IsBlockIn = isBlockIn;
            QSpace = qSpace;
        }

        public bool RealCoverPoint(TwoDPoint p)
        {
            var (item1, aabbBoxShape) = QSpace.TouchWithARightShootPoint(p);
#if DEBUG
            // Console.Out.WriteLine("num::" + item1);
            // var logSide = aabbBoxShape == null ? "" : aabbBoxShape.Zone.LogSide();
            // Console.Out.WriteLine($"{p}  box::{logSide}");
#endif
            if (item1 >= 0)
            {
                var inBlock = item1 % 2 != 0;

                return IsBlockIn ? inBlock : !inBlock;
            }

//-1 真包含，在边缘不计，-2 不压线在外面，-3在线上
            return item1 == -1;
        }

        public (bool isHitWall, TwoDPoint pt)
            PushOutToPt(TwoDPoint lastP, TwoDPoint nowP, out TwoDVector vector) //null表示不需要被动移动
        {
            var inLine = new TwoDVectorLine(lastP, nowP);
            var apt = QSpace.GetSlidePoint(inLine);
            vector = new TwoDVector(lastP, nowP);
            // if (safe)
            return (apt != null, apt ?? nowP);


            // if (apt != null)
            // {
            //     return RealCoverPoint(apt) ? PushOutToPt(lastP, apt, false) : (true, apt);
            // }
            //
            // return (false, nowP);
        }
    }
}