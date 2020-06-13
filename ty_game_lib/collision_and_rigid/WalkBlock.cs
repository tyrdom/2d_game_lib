#nullable enable
namespace collision_and_rigid
{
    public class WalkBlock : IShape
    {
//        private float R;
        public bool IsBlockIn;
        public IQSpace? QSpace;


        public WalkBlock(bool isBlockIn, IQSpace? qSpace)
        {
            IsBlockIn = isBlockIn;
            QSpace = qSpace;
        }

        public bool CoverPoint(TwoDPoint p)
        {
            if (QSpace == null) return true;

            var (item1, aabbBoxShape) = QSpace.TouchWithARightShootPoint(p);

//            Console.Out.WriteLine("num::" + item1);

//            Console.Out.WriteLine("box::" + SomeTools.ZoneLog(aabbBoxShape.Zone));
            if (item1 >= 0)
            {
                var inBlock = item1 % 2 != 0;
                return IsBlockIn ? inBlock : !inBlock;
            }

            return item1 == -1;
        }

        public TwoDPoint? PushOutToPt(TwoDPoint lastP, TwoDPoint nowP, bool safe = true)
        {
            var inLine = new TwoDVectorLine(lastP, nowP);
            if (QSpace == null) return null;
            var apt = QSpace.GetSlidePoint(inLine, true, safe);

            if (safe) return apt;
            if (apt != null)
            {
                return CoverPoint(apt) ? PushOutToPt(lastP, apt, false) : apt;
            }

            return null;
        }
    }
}