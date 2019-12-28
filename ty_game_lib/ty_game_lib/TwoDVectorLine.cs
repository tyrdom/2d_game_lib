#nullable enable
using System;
using System.Collections;
using System.Numerics;

namespace ty_game_lib
{
    public class TwoDVectorLine : IShape
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
            return TwoDVector.TwoDVectorByPt(A, B);
        }

        public TwoDPoint? CrossAnotherPoint(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var sA = A.Get2S(lineB);
            var sB = B.Get2S(lineB);
            var sC = c.Get2S(this);
            var b = sA - sB;
            var sD = b + sC;

            var b1 = sC < 0 ^ sD < 0;
            if (sA < 0f)
            {
                if (sB < 0f)
                {
                    return null;
                }

                if (sB > 0f)
                {
                    var a = sA / b;
                    var ab = GetVector();
                    return A.move(new TwoDVector(a * ab.X, b * ab.Y));
                }
                else
                {
                    if (b1)
                    {
                        return B;
                    }
                }
            }
            else
            {
                if (sA > 0f)
                {
                    if (sB > 0f)
                    {
                        return null;
                    }

                    if (sB < 0f)
                    {
                        var a = sA / b;
                        var ab = GetVector();
                        return A.move(new TwoDVector(a * ab.X, b * ab.Y));
                    }
                    else
                    {
                        if (b1)
                        {
                            return B;
                        }
                    }
                }
                else
                {
                    if (b1)
                    {
                        return A;
                    }
                }
            }

            return null;
        }

        public TwoDVectorLine MoveVector(TwoDVector v)
        {
            return new TwoDVectorLine(A.move(v), B.move(v));
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

        public bool SimpleIsCross(TwoDVectorLine lineB)
        {
            return (A.GetposOnLine(lineB) == Pt2LinePos.Right) && (B.GetposOnLine(lineB) != Pt2LinePos.Right);
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

        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = new Zone(Math.Max(A.Y, B.Y), Math.Min(A.Y, B.Y), Math.Min(A.X, B.X), Math.Max(A.X, B.X));

            return new AabbBoxShape(zone, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            var twoDVector = GetVector();
            // var f = twoDVector.X *twoDVector.Y
            if (twoDVector.X > 0)
            {
                var getposOnLine = p.GetposOnLine(this);
                return getposOnLine == Pt2LinePos.Left ? 1 : 0;
            }

            if (!(twoDVector.X < 0)) return 0;
            {
                var getposOnLine = p.GetposOnLine((this));

                return getposOnLine == Pt2LinePos.Right ? 1 : 0;
            }
        }

        public bool IsTouch(Round another)
        {
            var o = another.O;
            var cross = TwoDVector.TwoDVectorByPt(A, o).Cross(TwoDVector.TwoDVectorByPt(o, B));
            var aX = B.X - A.X;
            var bY = B.Y - A.Y;
            var x = aX * aX + bY * bY;
            return 4 * cross * cross / x <= another.R * another.R;
        }


        public TwoDPoint Slide(TwoDPoint p)
        {
            var twoDVector = new TwoDVectorLine(A, p).GetVector();
            var dVector = GetVector();
            var dot = twoDVector.Dot(dVector);
            var norm = dVector.Norm();
            var f = dot / norm;
            if (f < 0f)
            {
                return A;
            }

            return f > 1f ? B : A.move(dVector.GetUnit().Multi(f));
        }
    }
}