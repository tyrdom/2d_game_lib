using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;

namespace ty_game_lib
{
    public class Poly
    {
        Poly(TwoDPoint[] pts)
        {
            Pts = pts;
        }

        public TwoDPoint[] Pts { get; }
        
        
        public Poly ToNotCrossPoly(Poly aPoly)
        {
            var twoDPoints = aPoly.Pts;
            var length = twoDPoints.Length;
            foreach (var i in Enumerable.Range(0, length - 1))
            {
                var twoDVectorLineA = new TwoDVectorLine(twoDPoints[i], twoDPoints[(i + 1)]);
                foreach (var j in Enumerable.Range(i + 2, length - i - 2))
                {
                    var twoDVectorLineB = new TwoDVectorLine(twoDPoints[j], twoDPoints[(j + 1) % length]);
                    if (twoDVectorLineA.IsCrossAnother(twoDVectorLineB))
                    {
                        twoDPoints = SomeTools.SwapPoints(twoDPoints, i + 1, j);
                    }
                }
            }

            return new Poly(twoDPoints);
        }
        

    }
}