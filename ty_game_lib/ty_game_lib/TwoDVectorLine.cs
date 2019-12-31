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


        public bool TwoSide(TwoDVectorLine lineB)
        {
            var sA = A.Get2S(lineB);
            var sB = B.Get2S(lineB);
            return (sA > 0 && sB < 0) || (sA < 0 && sB > 0);
        }

        public TwoDPoint? CrossPoint(TwoDVectorLine lineB)
        {
            var c = lineB.A;

            var sA = A.Get2S(lineB);
            var sB = B.Get2S(lineB);
            var sC = c.Get2S(this);
            var b = sA - sB;
            var sD = b + sC;
            if (b > 0 || b < 0)
            {
                var a = sA / b;
                var ab = GetVector();
                return A.move(new TwoDVector(a * ab.X, a * ab.Y));
            }

            return null;
        }

        public TwoDPoint? CrossAnotherPointInLinesIncludeEnds(TwoDVectorLine lineB)
        {
            var c = lineB.A;

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
                    return A.move(new TwoDVector(a * ab.X, a * ab.Y));
                }
                else
                {
                    if (b1)
                    {
                        return B;
                    }
                }
            }
            else if (sA > 0f)
            {
                if (sB > 0f)
                {
                    return null;
                }

                if (sB < 0f)
                {
                    var a = sA / b;
                    var ab = GetVector();
                    return A.move(new TwoDVector(a * ab.X, a * ab.Y));
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

        public Zone GenZone()
        {
            var zone = new Zone(Math.Max(A.Y, B.Y), Math.Min(A.Y, B.Y), Math.Min(A.X, B.X), Math.Max(A.X, B.X));
            return zone;
        }

        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = GenZone();
            return new AabbBoxShape(zone, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            var twoDVector = GetVector();
            // var f = twoDVector.X *twoDVector.Y
            if (twoDVector.Y > 0)
            {
                var getposOnLine = p.GetposOnLine(this);
                return getposOnLine == Pt2LinePos.Left ? 1 : 0;
            }

            if (twoDVector.Y < 0)
            {
                var getposOnLine = p.GetposOnLine((this));

                return getposOnLine == Pt2LinePos.Right ? 1 : 0;
            }

            return 0;
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

        public TwoDPoint Shadow(TwoDPoint p)
        {
            var twoDVector = new TwoDVectorLine(A, p).GetVector();
            var dVector = GetVector();
            var dot = twoDVector.Dot(dVector);
            var norm = dVector.Norm();
            var f = dot / norm;
            return A.move(dVector.GetUnit().Multi(f));
        }

        public (TwoDPoint?, TwoDPoint?) CrossPtWithRound(Round rd)
        {
            var twoDPoint = Shadow(rd.O);
            var distanceRight = DistanceRight(rd.O);
            var rdR = rd.R * rd.R - distanceRight * distanceRight;
            if (rdR < 0)
            {
                return (null, null);
            }

            var sqrt = MathF.Sqrt(rdR);

            var twoDVector = GetVector().GetUnit();
            var v1 = twoDVector.Multi(sqrt);
            var v2 = twoDVector.Multi(-sqrt);
            var p1 = twoDPoint.move(v1);
            var p2 = twoDPoint.move(v2);
            return (p1, p2);
        }

        public TwoDPoint? CrossRdOnThisLineAndNearStartP(Round rd)
        {
            var (item1, item2) = CrossPtWithRound(rd);
            if (item1 == null)
            {
                return null;
            }

            var vector1 = new TwoDVectorLine(A, item1).GetVector();
            var dVector = GetVector();
            var f1 = vector1.Dot(dVector) / dVector.Norm();
            var vector2 = new TwoDVectorLine(A, item1).GetVector();
            var f2 = vector2.Dot(dVector) / dVector.Norm();
            if (f1 > 0 && f1 < 1)
            {
                if (f2 > 0 && f2 < 1)
                {
                    return f1 < f2 ? item1 : item2;
                }
                else
                {
                    return item1;
                }
            }
            else
            {
                return f2 > 0 && f2 < 1 ? item2 : null;
            }
        }

        public float DistanceRight(TwoDPoint p)
        {
            var get2S = p.Get2S(this);
            return get2S / GetVector().Norm();
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