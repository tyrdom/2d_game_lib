#nullable enable
using System;
using System.Collections.Generic;

namespace collision_and_rigid
{
    public class TwoDVectorLine : IShape, IBlockShape, IRawBulletShape
    {
        public bool AOut;
        public bool BOut;

        public TwoDVectorLine(TwoDPoint a, TwoDPoint b, bool aOut = false, bool bOut = false)
        {
            A = a;
            B = b;
            AOut = aOut;
            BOut = bOut;
        }


        public TwoDPoint A { get; }
        public TwoDPoint B { get; }

        public bool IsEmpty()
        {
            return A.Same(B);
        }

        public List<(TwoDPoint, CondAfterCross, CondAfterCross)>
            CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(
                IBlockShape blockShape)
        {
            return blockShape switch
            {
                null => throw new Exception("no good block!!"),
                ClockwiseTurning clockwiseTurning => UnionByCt(clockwiseTurning),
                TwoDVectorLine twoDVectorLine => UnionByLine(twoDVectorLine),
                _ => throw new ArgumentOutOfRangeException(nameof(blockShape))
            };
        }

        public TwoDPoint GetStartPt()
        {
            return A;
        }

        public TwoDPoint GetEndPt()
        {
            return B;
        }

        public (List<IBlockShape>, CondAfterCross, List<IBlockShape>) CutByPointReturnGoodBlockCondAndTemp(
            CondAfterCross nowCond,
            List<(TwoDPoint, CondAfterCross)>? ptsAndCond, List<IBlockShape> temp, CondAfterCross endCond
        )
        {
            if (ptsAndCond == null)
                ptsAndCond = new List<(TwoDPoint, CondAfterCross)>();
            else
                ptsAndCond.Sort((p1, p2) => GetMultiFromA(p1.Item1).CompareTo(GetMultiFromA(p2.Item1)));

            var blockShapes = new List<IBlockShape>();
            var nPt = A;

            for (var i = 0; i < ptsAndCond.Count; i++)
            {
                var (pt, cond) = ptsAndCond[i];
                switch (cond)
                {
                    case CondAfterCross.OutToIn:
                    {
                        var twoDVectorLine = new TwoDVectorLine(nPt, pt, i == 0);
                        if (temp.Count > 0) blockShapes.AddRange(temp);
                        temp.Clear();
                        blockShapes.Add(twoDVectorLine);
                        break;
                    }
                    case CondAfterCross.InToOut:
                        temp.Clear();
                        break;
                    default:
                    {
                        if ((nowCond == CondAfterCross.InToOut ||
                             nowCond == CondAfterCross.MaybeOutToIn) &&
                            cond == CondAfterCross.MaybeOutToIn)
                        {
                            var twoDVectorLine = new TwoDVectorLine(nPt, pt, i == 0);

                            temp.Add(twoDVectorLine);
                        }
                        else
                        {
                            Console.Out.WriteLine("Some Cond unexpected::" + nowCond + " and " +
                                                  cond);
                        }

                        break;
                    }
                }


                nowCond = cond;
                nPt = pt;
            }

            switch (endCond)
            {
                case CondAfterCross.OutToIn:
                    if (nowCond == CondAfterCross.InToOut)
                        Console.Out.WriteLine("Cond Error at end pt:::" + nowCond + " and " + endCond);

                    break;
                case CondAfterCross.InToOut:
                    var twoDVectorLine = new TwoDVectorLine(nPt, B, false, true);
                    blockShapes.Add(twoDVectorLine);
                    nowCond = CondAfterCross.InToOut;
                    break;
                case CondAfterCross.MaybeOutToIn:
                    if (nowCond != CondAfterCross.OutToIn)
                    {
                        var twoDVectorLine2 = new TwoDVectorLine(nPt, B, false, true);
                        temp.Add(twoDVectorLine2);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(nowCond), nowCond, null);
            }


            return (blockShapes, nowCond, temp);
        }

        public bool CheckAfter(IBlockShape another)
        {
            return GetEndPt().Same(another.GetStartPt());
        }

        public bool CheckBefore(IBlockShape another)
        {
            return GetStartPt().Same(another.GetEndPt());
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
                var getposOnLine = p.GetPosOf(this);
                return getposOnLine == Pt2LinePos.Left ? 1 : 0;
            }

            if (twoDVector.Y < 0)
            {
                var getposOnLine = p.GetPosOf(this);

                return getposOnLine == Pt2LinePos.Right ? 1 : 0;
            }

            return 0;
        }

        public bool IsSightBlockByAnother(IShape another)
        {
            return another switch
            {
                TwoDVectorLine twoDVectorLine => IsSightBlockByAnother(twoDVectorLine),
                _ => throw new ArgumentOutOfRangeException(nameof(another))
            };
        }

        public TwoDVector GetVector()
        {
            return TwoDVector.TwoDVectorByPt(A, B);
        }


        public bool TwoSide(TwoDVectorLine lineB)
        {
            var sA = A.Get2S(lineB);
            var sB = B.Get2S(lineB);
            return sA > 0 && sB < 0 || sA < 0 && sB > 0;
        }

        public TwoDPoint? CrossPointForWholeLine(TwoDVectorLine lineB)
        {
            var c = lineB.A;

            var sA = A.Get2S(lineB);
            var sB = B.Get2S(lineB);
            var b = sA - sB;
            if (b > 0 || b < 0)
            {
                var a = sA / b;
                var ab = GetVector();
                return A.Move(new TwoDVector(a * ab.X, a * ab.Y));
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
                return A.Move(new TwoDVector(a * ab.X, a * ab.Y));
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

            var b1 = (sC < 0) ^ (sD < 0);
            if (sA < 0f)
            {
                if (sB < 0f) return null;

                if (sB > 0f)
                {
                    var a = sA / b;
                    var ab = GetVector();
                    return A.Move(new TwoDVector(a * ab.X, a * ab.Y));
                }

                if (b1) return B;
            }
            else if (sA > 0f)
            {
                if (sB > 0f) return null;

                if (sB < 0f)
                {
                    var a = sA / b;
                    var ab = GetVector();
                    return A.Move(new TwoDVector(a * ab.X, a * ab.Y));
                }

                if (b1) return B;
            }
            else
            {
                if (b1) return A;
            }


            return null;
        }

        public TwoDVectorLine MoveVector(TwoDVector v)
        {
            return new TwoDVectorLine(A.Move(v), B.Move(v));
        }

        public bool IsPointOnAnother(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var getPosOnLineA = A.GetPosOf(lineB);
            var getPosOnLineB = B.GetPosOf(lineB);
            var getPosOnLineC = c.GetPosOf(this);
            var getPosOnLineD = d.GetPosOf(this);
            return getPosOnLineA switch
            {
                Pt2LinePos.On => getPosOnLineC != getPosOnLineD,
                var posA when posA != getPosOnLineB => getPosOnLineC != getPosOnLineD,
                _ => false
            };
        }


        public bool SimpleIsCross(TwoDVectorLine lineB)
        {
            return A.GetPosOf(lineB) == Pt2LinePos.Right && B.GetPosOf(lineB) != Pt2LinePos.Right;
        }


        public bool IsSightBlockByAnother(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var getposOnLineA = A.GetPosOf(lineB);
            var getposOnLineB = B.GetPosOf(lineB);
            var getposOnLineC = c.GetPosOf(this);
            var getposOnLineD = d.GetPosOf(this);
            return getposOnLineA switch
                   {
                       Pt2LinePos.Left => getposOnLineB == Pt2LinePos.Right,
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

        public bool IsCrossAnother(TwoDVectorLine lineB)
        {
            var c = lineB.A;
            var d = lineB.B;
            var getposOnLineA = A.GetPosOf(lineB);
            var getposOnLineB = B.GetPosOf(lineB);
            var getposOnLineC = c.GetPosOf(this);
            var getposOnLineD = d.GetPosOf(this);
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

        public TwoDVectorLine CounterClockwiseHalfPi()
        {
            var twoDPoint = A.Move(GetVector().CounterClockwiseHalfPi());
            return new TwoDVectorLine(A, twoDPoint);
        }


        private float? GetX(float y)
        {
            var bY = y - B.Y;
            var aY = y - A.Y;
            var u = aY * B.X - bY * A.X;
            var d = aY - bY;
            if (d > 0 || d < 0) return u / d;

            return null;
        }

        private float? GetY(float x)
        {
            var bY = x - B.X;
            var aY = x - A.X;
            var u = aY * B.Y - bY * A.Y;
            var d = aY - bY;
            if (d > 0 || d < 0) return u / d;

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
            return A.Move(GetVector().GetUnit().Multi(f));
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

        public float GetScaleInPt(TwoDPoint p)
        {
            var twoDVector = new TwoDVectorLine(A, p).GetVector();
            var dVector = GetVector();
            var dot = twoDVector.Dot(dVector);
            var sqNorm = dVector.SqNorm();
            var f = dot / sqNorm;
            return f;
        }

        public (TwoDPoint?, TwoDPoint?) CrossPtWithRound(Round rd)
        {
            var twoDPoint = ShadowBy(rd.O);
            var distanceRight = DistanceRight(rd.O);
            var rdR = rd.R * rd.R - distanceRight * distanceRight;
            if (rdR < 0) return (null, null);

            if (!(rdR > 0)) return (null, twoDPoint);

            var sqrt = MathTools.Sqrt(rdR);


            var twoDVector = GetVector().GetUnit();
            var v1 = twoDVector.Multi(sqrt);
            var v2 = twoDVector.Multi(-sqrt);
            var p1 = twoDPoint.Move(v1);
            var p2 = twoDPoint.Move(v2);
            return (p1, p2);
        }


        public float DistanceRight(TwoDPoint p)
        {
            var get2S = p.Get2S(this);
            return get2S / GetVector().Norm();
        }

        public TwoDPoint Slide(TwoDPoint p, bool safe)
        {
            var twoDVector = new TwoDVectorLine(A, p).GetVector();
            var dVector = GetVector();
            var dot = twoDVector.Dot(dVector);
            var norm = dVector.SqNorm();
            var f = dot / norm;
            if (f <= 0f) return AOut && !safe ? A.Move(dVector.GetUnit().Multi(f)) : A;

            if (f >= 1f) return BOut && !safe ? A.Move(dVector.GetUnit().Multi(f)) : B;

            return A.Move(dVector.GetUnit().Multi(f));
        }


        public (bool, TwoDVectorLine, ClockwiseTurning) TouchByCt(ClockwiseTurning ct)
        {
            var balanceAngle = ct.Aob;
            var o = balanceAngle.O;
            var rd = new Round(o, ct.R);
            var dVectorLine = new TwoDVectorLine(o, A);

            var (pt1, pt2) = CrossPtWithRound(rd);
            if (pt1 == null || pt2 == null || dVectorLine.GetVector().SqNorm() <= ct.R * ct.R) return (false, this, ct);

            TwoDPoint? pp = null;

            var cover1 = balanceAngle.Cover(pt1)
                         && GetScaleInPt(pt1) <= 1 && GetScaleInPt(pt1) >= 0;
            var cover2 = balanceAngle.Cover(pt2) && GetScaleInPt(pt2) <= 1 && GetScaleInPt(pt2) >= 0;

            if (cover1)
            {
                if (cover2)
                {
                    var twoDVectorLine = new TwoDVectorLine(balanceAngle.O, pt1);
                    if (pt2.GetPosOf(twoDVectorLine) == Pt2LinePos.Left &&
                        GetScaleInPt(pt2) > GetScaleInPt(pt1))
                        pp = pt1;
                    else if (pt2.GetPosOf(twoDVectorLine) == Pt2LinePos.Right &&
                             GetScaleInPt(pt2) < GetScaleInPt(pt1))
                        pp = pt2;
                }
                else
                {
                    pp = pt1;
                }
            }
            else
            {
                if (cover2) pp = pt2;
            }


            if (pp == null) return (false, this, ct);
            var clockwiseBalanceAngle = new ClockwiseBalanceAngle(pp, balanceAngle.O, balanceAngle.B);
            var clockwiseTurning = new ClockwiseTurning(clockwiseBalanceAngle, ct.R, null, ct.Next);

            var newLine = new TwoDVectorLine(A, pp);

            return (true, newLine, clockwiseTurning);
        }


        public List<(TwoDPoint, CondAfterCross, CondAfterCross)> UnionByCt(ClockwiseTurning clockwiseTurning)
        {
            var rd = clockwiseTurning.BelongRd();
            var o = clockwiseTurning.Aob.O;
            var cAobB = clockwiseTurning.Aob.B;
            var cAobA = clockwiseTurning.Aob.A;
            var (item1, item2) = CrossPtWithRound(rd);
            var twoDPoints = new List<(TwoDPoint, CondAfterCross, CondAfterCross)>();
            if (item1 == null || item2 == null) return twoDPoints;

            var f1 = GetScaleInPt(item1);
            var f2 = GetScaleInPt(item2);
//            Console.Out.WriteLine("maybe have cross pt" + f1 + "==" + f2);
            var (p1, p2) = f1 < f2 ? (item1, item2) : (item2, item1);

            var f11 = MathTools.Min(f1, f2);
            var f22 = MathTools.Max(f1, f2);


            var op1 = new TwoDVectorLine(o, p1);
            var op2 = new TwoDVectorLine(o, p2);
            var b = p2.GetPosOf(op1) == Pt2LinePos.Right;
            var f1B = f11 > 0 && f11 <= 1;
            var f2B = f22 > 0 && f22 <= 1;
            if (b)
            {
                var cb1 = cAobA.GetPosOf(this) == Pt2LinePos.Right;
                var cb2 = cAobB.GetPosOf(op1) != Pt2LinePos.Left;
                var f1B1 = f1B && cb1 && cb2;
                if (f1B1)
                    twoDPoints.Add(f11 < 1
                        ? (p1, CondAfterCross.OutToIn, CondAfterCross.InToOut)
                        : (p1, CondAfterCross.MaybeOutToIn, CondAfterCross.InToOut));

                var cb3 = cAobB.GetPosOf(this) == Pt2LinePos.Right;
                var cb4 = cAobA.GetPosOf(op2) != Pt2LinePos.Right;
                var f2B1 = f2B && cb3 && cb4;
                if (f2B1) twoDPoints.Add((p2, CondAfterCross.InToOut, CondAfterCross.OutToIn));
            }
            else
            {
//                Console.Out.WriteLine("@##@");
                var b3 = cAobB.GetPosOf(this) != Pt2LinePos.Right;
                var b4 = cAobA.GetPosOf(op1) == Pt2LinePos.Left;
                var f1B1 = b3 && b4 && f1B;
//                Console.Out.WriteLine("?p1?" + b3 + b4 + f1B);
                if (f1B1)
                    //                    Console.Out.WriteLine("????");
                    twoDPoints.Add(f11 < 1
                        ? (p1, CondAfterCross.OutToIn, CondAfterCross.InToOut)
                        : (p1, CondAfterCross.MaybeOutToIn, CondAfterCross.InToOut));

                var b5 = cAobA.GetPosOf(this) != Pt2LinePos.Right;
                var b6 = cAobB.GetPosOf(op2) == Pt2LinePos.Right;
                var f2B1 = b5 && b6 && f2B;

//                Console.Out.WriteLine("?p2?" + b5 + b6 + f2B);
                if (f2B1) twoDPoints.Add((p2, CondAfterCross.InToOut, CondAfterCross.OutToIn));
            }

            return twoDPoints;
        }


        public List<(TwoDPoint, CondAfterCross, CondAfterCross)> UnionByLine(TwoDVectorLine line)
        {
            var pt = CrossPointForWholeLine(line);
            var twoDPoints = new List<(TwoDPoint, CondAfterCross, CondAfterCross)>();
            if (pt == null) return twoDPoints;

            var b1 = A.GetPosOf(line) == Pt2LinePos.Left && B.GetPosOf(line) != Pt2LinePos.Left;
            var b2 = A.GetPosOf(line) == Pt2LinePos.Right && B.GetPosOf(line) != Pt2LinePos.Right;
            var b11 = B.GetPosOf(line) == Pt2LinePos.Right;
            var b3 = line.A.GetPosOf(this) != Pt2LinePos.Right && line.B.GetPosOf(this) == Pt2LinePos.Right;
            var b4 = line.B.GetPosOf(this) != Pt2LinePos.Right && line.A.GetPosOf(this) == Pt2LinePos.Right;
            var c1 = b1 && b3;

            var c2 = b1 && b4;
            var c3 = b2 && b3;
            var c4 = b2 && b4;

            if (c1)
                twoDPoints.Add(b11
                    ? (pt, CondAfterCross.OutToIn, CondAfterCross.OutToIn)
                    : (pt, CondAfterCross.MaybeOutToIn, CondAfterCross.OutToIn));

            else if (c2)
                twoDPoints.Add(b11
                    ? (pt, CondAfterCross.OutToIn, CondAfterCross.InToOut)
                    : (pt, CondAfterCross.MaybeOutToIn, CondAfterCross.InToOut));
            else if (c3)
                twoDPoints.Add((pt, CondAfterCross.InToOut, CondAfterCross.OutToIn));
            else if (c4) twoDPoints.Add((pt, CondAfterCross.InToOut, CondAfterCross.InToOut));

            return twoDPoints;
        }

        public (bool, TwoDVectorLine, TwoDVectorLine) TouchByLineInSamePoly(TwoDVectorLine line)
        {
            var posOf = line.B.GetPosOf(this);
            if (posOf != Pt2LinePos.Left) return (false, this, line);

            var cp = CrossAnotherPointInLinesNotIncludeEnds(line);
            if (cp == null)
            {
                return (false, this, line);
            }

            var line1 = new TwoDVectorLine(A, cp);
            var line2 = new TwoDVectorLine(cp, line.B);
            return (true, line1, line2);
        }

        public Zone GenBulletZone(float r)
        {
            var z = GenZone().MoreHigh(r).MoreWide(r);
            return z;
        }

        public TwoDVectorLine Reverse()
        {
            return new TwoDVectorLine(B, A);
        }

        public IBulletShape GenBulletShape(float r)
        {
            var twoDVector = GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);
            var twoDVectorLine1 = MoveVector(twoDVector);
            var twoDVectorLine2 =
                Reverse().MoveVector(Reverse().GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r));
            var clockwiseBalanceAngle1 = new ClockwiseBalanceAngle(twoDVectorLine1.B, B, twoDVectorLine2.A);
            var clockwiseBalanceAngle2 = new ClockwiseBalanceAngle(twoDVectorLine2.B, A, twoDVectorLine1.A);
            var clockwiseTurning1 = new ClockwiseTurning(clockwiseBalanceAngle1, r, twoDVectorLine1, twoDVectorLine2);
            var clockwiseTurning2 = new ClockwiseTurning(clockwiseBalanceAngle2, r, twoDVectorLine2, twoDVectorLine1);
            var blockShapes = new List<IBlockShape>
                {twoDVectorLine1, clockwiseTurning1, twoDVectorLine2, clockwiseTurning2};
            return new SimpleBlocks(Poly.GenBlockAabbBoxShapes(blockShapes));
        }
    }
}