using System;

namespace ty_game_lib
{
    public class ClockwiseBalanceAngle
    {
        public TwoDPoint A;
        public TwoDPoint O;
        public TwoDPoint B;

        public ClockwiseBalanceAngle(TwoDPoint a, TwoDPoint o, TwoDPoint b)
        {
            A = a;
            O = o;
            B = b;
        }

        public bool CheckTuring()
        {
            var twoDVectorLine = new TwoDVectorLine(A, O);
            var getposOnLine = B.GetposOnLine(twoDVectorLine);

            return getposOnLine != Pt2LinePos.Right;
        }
    }
}