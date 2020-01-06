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

        public TwoDPoint? CrossAnotherPointInLinesNotIncludeEnds(TwoDVectorLine lineB)
        {
            var c = lineB.A;

            var sA = A.Get2S(lineB);
            var sB = B.Get2S(lineB);
            var sC = c.Get2S(this);
            var b = sA - sB;
            var sD = b + sC;

            var b1 = sC * sD < 0;
            var b2 = sA * sB < 0;
            if (b1 && b2)
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
            var getPosOnLineA = A.GetPosOnLine(lineB);
            var getPosOnLineB = B.GetPosOnLine(lineB);
            var getPosOnLineC = c.GetPosOnLine(this);
            var getPosOnLineD = d.GetPosOnLine(this);
            return getPosOnLineA switch
            {
                Pt2LinePos.On => getPosOnLineC != getPosOnLineD,
                var posA when posA != getPosOnLineB => getPosOnLineC != getPosOnLineD,
                _ => false
            };
        }

        public bool SimpleIsCross(TwoDVectorLine lineB)
        {
            return (A.GetPosOnLine(lineB) == Pt2LinePos.Right) && (B.GetPosOnLine(lineB) != Pt2LinePos.Right);
        }

        public bool IsCrossAnother(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var getposOnLineA = A.GetPosOnLine(lineB);
            var getposOnLineB = B.GetPosOnLine(lineB);
            var getposOnLineC = c.GetPosOnLine(this);
            var getposOnLineD = d.GetPosOnLine(this);
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
            var zone = A.GenZone(B);
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
                var getposOnLine = p.GetPosOnLine(this);
                return getposOnLine == Pt2LinePos.Left ? 1 : 0;
            }

            if (twoDVector.Y < 0)
            {
                var getposOnLine = p.GetPosOnLine((this));

                return getposOnLine == Pt2LinePos.Right ? 1 : 0;
            }

            return 0;
        }

        float? GetX(float y)
        {
            var bY = y - B.Y;
            var aY = y - A.Y;
            var u = aY * B.X - bY * A.X;
            var d = aY - bY;
            if (d > 0 || d < 0)
            {
                return u / d;
            }

            return null;
        }

        float? GetY(float x)
        {
            var bY = x - B.X;
            var aY = x - A.X;
            var u = aY * B.Y - bY * A.Y;
            var d = aY - bY;
            if (d > 0 || d < 0)
            {
                return u / d;
            }

            return null;
        }

        public (Zone?, Zone?) CutByH(float h, Zone z)
        {
            var x = GetX(h);
            var twoDVector = GetVector();
            var b = twoDVector.IsAlmostRightUp();
            if (x != null)
            {
                if (b)
                {
                    var uZone = new Zone(z.Up, h, x.Value, z.Right);
                    var dZone = new Zone(h, z.Down, z.Left, x.Value);
                    return (uZone, dZone);
                }
                else
                {
                    var uZone = new Zone(z.Up, h, z.Left, x.Value);
                    var dZone = new Zone(h, z.Down, x.Value, z.Right);
                    return (uZone, dZone);
                }
            }

            return (null, null);
        }

        public (Zone?, Zone?) CutByV(float v, Zone z)
        {
            var y = GetX(v);
            var twoDVector = GetVector();
            var b = twoDVector.IsAlmostRightUp();
            if (y == null) return (null, null);
            if (b)
            {
                var rZone = new Zone(z.Up, y.Value, v, z.Right);
                var lZone = new Zone(y.Value, z.Down, z.Left, v);
                return (lZone, rZone);
            }
            else
            {
                var lZone = new Zone(z.Up, y.Value, z.Left, v);
                var rZone = new Zone(y.Value, z.Down, v, z.Right);
                return (lZone, rZone);
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

        public TwoDPoint ShadowBy(TwoDPoint p)
        {
            var f = GetMultiFromA(p);
            return A.move(GetVector().GetUnit().Multi(f));
        }


        public float GetMultiFromA(TwoDPoint p)
        {
            var twoDVector = new TwoDVectorLine(A, p).GetVector();
            var dVector = GetVector();
            var dot = twoDVector.Dot(dVector);
            var norm = dVector.Norm();
            var f = dot / norm;
            return f;
        }

        public (TwoDPoint?, TwoDPoint?) CrossPtWithRound(Round rd)
        {
            var twoDPoint = ShadowBy(rd.O);
            var distanceRight = DistanceRight(rd.O);
            var rdR = rd.R * rd.R - distanceRight * distanceRight;
            if (rdR < 0)
            {
                return (null, null);
            }

            if (!(rdR > 0)) return (null, twoDPoint);
            var sqrt = MathF.Sqrt(rdR);

            var twoDVector = GetVector().GetUnit();
            var v1 = twoDVector.Multi(sqrt);
            var v2 = twoDVector.Multi(-sqrt);
            var p1 = twoDPoint.move(v1);
            var p2 = twoDPoint.move(v2);
            return (p1, p2);
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


        public (bool, TwoDVectorLine, ClockwiseTurning) TouchByCt(ClockwiseTurning ct)
        {
            var balanceAngle = ct.AOB;
            var o = balanceAngle.O;
            var rd = new Round(o, ct.R);
            var dVectorLine = new TwoDVectorLine(o, A);

            var (pt1, pt2) = CrossPtWithRound(rd);
            if (pt1 == null || dVectorLine.GetVector().SqNorm() <= ct.R * ct.R)
            {
//                Console.Out.WriteLine("ptpt null::" + pt1 + '_' + pt2);
                return (false, this, ct);
            }

            TwoDPoint? pp = null;


            var cover1 = balanceAngle.Cover(pt1)
                         && GetMultiFromA(pt1) < 1 && GetMultiFromA(pt1) > 0;
            var cover2 = balanceAngle.Cover(pt2) && GetMultiFromA(pt2) < 1 && GetMultiFromA(pt2) > 0;
            if (cover1)
            {
                if (cover2)
                {
                    var twoDVectorLine = new TwoDVectorLine(balanceAngle.O, pt1);
                    if (pt2.GetPosOnLine(twoDVectorLine) == Pt2LinePos.Left &&
                        GetMultiFromA(pt2) > GetMultiFromA(pt1))
                    {
                        pp = pt1;
                    }
                    else if (pt2.GetPosOnLine(twoDVectorLine) == Pt2LinePos.Right &&
                             GetMultiFromA(pt2) < GetMultiFromA(pt1))
                    {
                        pp = pt2;
                    }
                    else
                    {
                        pp = null;
                    }
                }
                else
                {
                    pp = pt1;
                }
            }
            else
            {
                if (cover2)
                {
                    pp = pt2;
                }
            }


            if (pp != null)
            {
                var clockwiseBalanceAngle = new ClockwiseBalanceAngle(pp, balanceAngle.O, balanceAngle.B);
                var clockwiseTurning = new ClockwiseTurning(clockwiseBalanceAngle, ct.R, null, ct.Next);

                var newLine = new TwoDVectorLine(A, pp);

                return (true, newLine, clockwiseTurning);
            }

            {
                return (false, this, ct);
            }
        }


        public (bool, TwoDVectorLine, TwoDVectorLine) TouchByLine(TwoDVectorLine line)
        {
            var getposOnLine = line.B.GetPosOnLine(this);
            if (getposOnLine != Pt2LinePos.Left)
            {
                return (false, this, line);
            }

            var cp = CrossAnotherPointInLinesNotIncludeEnds(line);
            if (cp == null)
            {
                return (false, this, line);
            }
            else
            {
                var line1 = new TwoDVectorLine(A, cp);
                var line2 = new TwoDVectorLine(cp, line.B);
                return (true, line1, line2);
            }
        }
    }
}