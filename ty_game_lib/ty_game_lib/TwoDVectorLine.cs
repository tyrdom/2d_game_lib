using System;
using System.Collections;

namespace ty_game_lib
{
    public class TwoDVectorLine : Shape
    {
        public TwoDVectorLine(TwoDPoint a, TwoDPoint b)
        {
            A = a;
            B = b;
        }


        public TwoDPoint A { get; }

        public TwoDPoint B { get; }

        public TwoDVector GetVector()
        {
            return new TwoDVector(A,B);
        }

        public TwoDPoint? CrossAnotherPoint(TwoDVectorLine lineB)
        {
            var getposOnLine = A.GetposOnLine(lineB);

            return null;
        }

//        判断线段相交
        public bool IsPointOnAnother(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var getPosOnLineA = A.GetposOnLine(lineB);
            var getPosOnLineB = B.GetposOnLine(lineB);
            var getPosOnLineC = c.GetposOnLine(this);
            var getPosOnLineD = d.GetposOnLine(this);
            return getPosOnLineA switch
            {
                Pt2LinePos.On => getPosOnLineC != getPosOnLineD,
                var posA when posA != getPosOnLineB => getPosOnLineC != getPosOnLineD,
                _ => false
            };
        }

        public bool IsCrossAnother(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var getposOnLineA = A.GetposOnLine(lineB);
            var getposOnLineB = B.GetposOnLine(lineB);
            var getposOnLineC = c.GetposOnLine(this);
            var getposOnLineD = d.GetposOnLine(this);
            return getposOnLineA switch
                   {
                       Pt2LinePos.Left => getposOnLineB == Pt2LinePos.Right,
                       Pt2LinePos.Right => getposOnLineB == Pt2LinePos.Left,
                       _ => false
                   }
                   &&
                   getposOnLineC switch
                   {
                       Pt2LinePos.Left => getposOnLineD == Pt2LinePos.Right,
                       Pt2LinePos.Right => getposOnLineD == Pt2LinePos.Left,
                       _ => false
                   };
        }

        public AabbBox CovToAabbBox()
        {
            var zone = new Zone(Math.Max(A.Y, B.Y), Math.Min(A.Y, B.Y), Math.Min(A.X, B.X), Math.Max(A.X, B.X));

            return new AabbBox(zone, this);
        }

        public bool IsTouch(Round another)
        {
            var o = another.O;
            var cross = new TwoDVector(A,o).Cross(new TwoDVector(o,B));
            var aX = B.X-A.X;
            var bY = B.Y-A.Y;
            var x = aX*aX+bY*bY;
            return 4 * cross * cross / x <= another.R * another.R;
        }
    }
}