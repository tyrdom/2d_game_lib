using System;

namespace ty_game_lib
{
    public static class SomeTools
    {
        public static TwoDPoint[] SwapPoints(TwoDPoint[] x, int a, int b)
        {
            var xLength = x.Length;
            if (a >= xLength || b >= xLength) return x;
            var t = x[b];
            x[b] = x[a];
            x[a] = t;
            return x;
        }
    }
}