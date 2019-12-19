using System;

namespace ty_game_lib
{
    public class ClockwiseAngle
    {
        public TwoDPoint A;
        public TwoDPoint O;
        public TwoDPoint B;

        public ClockwiseAngle(TwoDPoint a, TwoDPoint o, TwoDPoint b)
        {
            A = a;
            O = o;
            B = b;
        }
    }
}