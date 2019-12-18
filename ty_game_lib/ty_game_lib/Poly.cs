using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Xml.Schema;


namespace ty_game_lib
{
    public class Poly
    {
        public Poly(TwoDPoint[] pts)
        {
            if (pts.Length >= 3)

            {
                Pts = pts;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public TwoDPoint[] Pts { get; }


        public Poly ToNotCrossPoly()
        {
            var twoDPoints = Pts;
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


        public int GetACovPointNum()
        {
            var n = 0;
            var pt = Pts[0];
            var f = pt.X;
            foreach (var i in Enumerable.Range(0, Pts.Length))
            {
                var x = Pts[i].X;
                if (x < f) continue;
                n = i;
                f = x;
            }

            return n;
        }

        public bool IsFlush()
        {
            var n = GetACovPointNum();
            var ptsLength = Pts.Length;
            var m = (n - 1) % ptsLength;
            var o = (n + 1) % ptsLength;
            var twoDVectorLine1 = new TwoDVectorLine(Pts[m], Pts[o]);
            var pt2LinePos = Pts[n].GetposOnLine(twoDVectorLine1);
            return pt2LinePos switch
            {
                Pt2LinePos.Right => true,
                Pt2LinePos.On => throw new ArgumentOutOfRangeException(),
                Pt2LinePos.Left => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public Poly startWithACovAndFlush()
        {
            Poly poly;
            if (!IsFlush())
            {
                var twoDPoints = Pts.Reverse();
                 poly = new Poly(twoDPoints.ToArray()).ToNotCrossPoly();
            }
            else
            {
                poly = this.ToNotCrossPoly();
            }
            
            var n = poly.GetACovPointNum();
            var ptsLength = Pts.Length;

            return new Poly(Enumerable.Range(0, ptsLength).Select(i => (n + i) % ptsLength).Select(p => Pts[p]).ToArray());
        }
    }
}