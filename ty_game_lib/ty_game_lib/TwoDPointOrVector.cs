using System;
using System.Collections.Immutable;
using System.Runtime.Intrinsics.X86;

namespace ty_game_lib
{
    public class TwoDPoint
    {
        public TwoDPoint(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x;
        public float y;


        Pt2LinePos getposOnLine(TwoDVectorLine aline)
        {
            TwoDVectorLine xline = new TwoDVectorLine(aline.a, this);

            float cross = xline.getVector().cross(aline.getVector());
            switch (cross)

            {
                case float cro when cro > 0:
                    return Pt2LinePos.Right;
                case float cro when cro < 0:
                    return Pt2LinePos.Left;
                default:
                    return Pt2LinePos.On;
            }
        }
    }


    public class TwoDVector
    {
        public TwoDVector(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x;
        public float y;


        public float cross(TwoDVector b)
        {
            return this.x * b.y - b.x * this.y;
        }
    }

    enum Pt2LinePos
    {
        Right,
        On,
        Left
    }
}