#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace collision_and_rigid
{
    public class TwoDVectorLine : IBlockShape, IRawBulletShape
    {
        private readonly bool AOut;
        private readonly bool BOut;

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


        public TwoDVectorLine AsTwoDVectorLine()
        {
            return this;
        }

        public List<BlockBox> GenAabbBoxShape()
        {
            return new List<BlockBox> {CovToAabbPackBox()};
        }

        public override string ToString()
        {
            return $"{A}|{AOut}--{B}{BOut}";
        }

        public bool Include(TwoDPoint pos)
        {
            return false;
        }


        public TwoDPoint GetMid()
        {
            var aX = (A.X + B.X) / 2;
            var aY = (A.Y + B.Y) / 2;
            return new TwoDPoint(aX, aY);
        }


        public List<(TwoDPoint crossPt, CondAfterCross shape1AfterCond, CondAfterCross shape2AfterCond)>
            CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(IBlockShape blockShape)
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

        public (List<IBlockShape>, CondAfterCross, List<IBlockShape>) CutByPointReturnGoodBlockCondAndTemp
        (
            CondAfterCross startCond,
            List<(TwoDPoint, CondAfterCross)>? ptsAndCond,
            List<IBlockShape> temp,
            CondAfterCross endCond
        )
        {
            //交点排序
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
                    case CondAfterCross.ToIn:
                    {
#if DEBUG
                        if (startCond == CondAfterCross.ToIn)
                        {
                            var selectMany = ptsAndCond.Aggregate("cutPt::",
                                (s, x) => s + $"==pt::{x.Item1.LogPt()} c:{x.Item2}==");

                            Console.Out.WriteLine($" Cond InTIn~~~{ToString()}  {selectMany} :::{i}");
                            throw new Exception($" Cond InTIn~~~{ToString()}  {selectMany} :::{i}");
                        }
#endif
                        var twoDVectorLine = new TwoDVectorLine(nPt, pt, AOut && i == 0);
                        if (temp.Count > 0) blockShapes.AddRange(temp);
                        temp.Clear();
                        blockShapes.Add(twoDVectorLine);
                        break;
                    }
                    case CondAfterCross.ToOut:
#if DEBUG
                        if (startCond == CondAfterCross.ToOut)
                        {
                            var selectMany = ptsAndCond.Aggregate("cutPt::",
                                (s, x) => s + $"==pt::{x.Item1.LogPt()} c:{x.Item2}==");

                            Console.Out.WriteLine($" Cond OutTOut~~~{ToString()}  {selectMany} :::{i}");
                            throw new Exception($" Cond OutTOut~~~{ToString()}  {selectMany} :::{i}");
                        }
#endif
                        temp.Clear();
                        break;
                    default:
                    {
                        if ((startCond == CondAfterCross.ToOut ||
                             startCond == CondAfterCross.MaybeOutToIn) &&
                            cond == CondAfterCross.MaybeOutToIn)
                        {
                            var twoDVectorLine = new TwoDVectorLine(nPt, pt, AOut && i == 0);

                            temp.Add(twoDVectorLine);
                        }
                        else
                        {
                            Console.Out.WriteLine("Some Cond unexpected::" + startCond + " and " +
                                                  cond);
                        }

                        break;
                    }
                }


                startCond = cond;
                nPt = pt;
            }

            switch (endCond)
            {
                case CondAfterCross.ToIn:
                    if (startCond == CondAfterCross.ToOut)
                    {
                        var selectMany = ptsAndCond.Aggregate("cutPt::",
                            (s, x) => s + $"==pt::{x.Item1.LogPt()} c:{x.Item2}==");

                        Console.Out.WriteLine($" Cond end InTOut~~~{ToString()}  {selectMany} :::");
                        throw new Exception($" Cond end IntTOut~~~{ToString()}  {selectMany} :::");
                    }

                    break;
                case CondAfterCross.ToOut:
                    var twoDVectorLine = new TwoDVectorLine(nPt, B, nPt == A && AOut, BOut);
                    blockShapes.Add(twoDVectorLine);
                    startCond = CondAfterCross.ToOut;
                    break;
                case CondAfterCross.MaybeOutToIn:
                    if (startCond != CondAfterCross.ToIn)
                    {
                        var twoDVectorLine2 = new TwoDVectorLine(nPt, B, nPt == A && AOut, BOut);
                        temp.Add(twoDVectorLine2);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startCond), startCond, null);
            }


            return (blockShapes, startCond, temp);
        }

        public bool CheckAfter(IBlockShape another)
        {
            return GetEndPt().Same(another.GetStartPt());
        }

        public bool CheckBefore(IBlockShape another)
        {
            return GetStartPt().Same(another.GetEndPt());
        }

        public BlockBox CovToAabbPackBox()
        {
            var zone = GenZone();
            return new BlockBox(zone, this);
        }

        public int TouchByRightShootPointInAAbbBoxInQSpace(TwoDPoint p)
        {
            var twoDVector = GetVector();
            // var f = twoDVector.X *twoDVector.Y
            if (twoDVector.Y > 0)
            {
                var getposOnLine = p.GetPosOf(this);
                return getposOnLine switch
                {
                    Pt2LinePos.Right => 0,
                    Pt2LinePos.On => -3,
                    Pt2LinePos.Left => 1,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (!(twoDVector.Y < 0)) return -3;
            {
                var getposOnLine = p.GetPosOf(this);
                return getposOnLine switch
                {
                    Pt2LinePos.Right => 1,
                    Pt2LinePos.On => -3,
                    Pt2LinePos.Left => 0,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public bool IsSightBlockByWall(IShape another)
        {
            return another switch
            {
                TwoDVectorLine twoDVectorLine => IsSightBlockByWall(twoDVectorLine),
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
            return new TwoDVectorLine(A.Move(v), B.Move(v), AOut, BOut);
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


        public bool IsGoTrough(TwoDVectorLine lineB)
        {
            var pt2LinePos = A.GetPosOf(lineB);
            var b = pt2LinePos != Pt2LinePos.Right;
            var b1 = B.GetPosOf(lineB) == Pt2LinePos.Right;

            return b && b1;
        }


        public bool IsSightBlockByWall(TwoDVectorLine wall)
        {
#if DEBUG
            Console.Out.WriteLine($"this {this} vs wall {wall}");
#endif
            var c = wall.A;
            var d = wall.B;
            var getposOnLineA = A.GetPosOf(wall);
            var getposOnLineB = B.GetPosOf(wall);
            var getposOnLineC = c.GetPosOf(this);
            var getposOnLineD = d.GetPosOf(this);
            return getposOnLineA switch
                   {
                       Pt2LinePos.Left => getposOnLineB == Pt2LinePos.Right,
                       // Pt2LinePos.Right => getposOnLineB == Pt2LinePos.Left,
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

        public (Zone? leftZone, Zone? rightZone) CutByV(float v, Zone z)
        {
            var y = GetY(v);
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
            // if (norm <= 0)
            // {
            //     return 0;
            // }

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


        private float DistanceRight(TwoDPoint p)
        {
            var get2S = p.Get2S(this);
            return get2S / GetVector().Norm();
        }

        public TwoDPoint Slide(TwoDPoint p)
        {
            var twoDVector = new TwoDVectorLine(A, p).GetVector();
            var dVector = GetVector();
            var dot = twoDVector.Dot(dVector);
            var sqNorm = dVector.SqNorm();
            var f = dot / sqNorm;
            var gap = dVector.GetUnit().AntiClockwiseTurn(new TwoDVector(0, 1)).Multi(0.01f);
// #if DEBUG
//             Console.Out.WriteLine($"ffffffffffffff::::{f}");
// #endif
            if (f <= 0f) return AOut ? A.Move(dVector.Multi(f)) : A.Move(dVector.Multi(0.01f)).Move(gap);

            if (f >= 1f) return BOut ? A.Move(dVector.Multi(f)) : A.Move(dVector.Multi(0.99f)).Move(gap);

            var twoDPoint = A.Move(dVector.Multi(f)).Move(gap);
// #if DEBUG
//             Console.Out.WriteLine($"ffffffffffffff::::lllllllllllll{twoDPoint.Log()}");
// #endif
            return twoDPoint;
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


        private List<(TwoDPoint crossPt, CondAfterCross thisCond, CondAfterCross anotherCond)> UnionByCt(
            ClockwiseTurning clockwiseTurning)
        {
            var rd = clockwiseTurning.BelongRd();
            var o = clockwiseTurning.Aob.O;
            var cAobB = clockwiseTurning.Aob.B;
            var cAobA = clockwiseTurning.Aob.A;
            var (item1, item2) = CrossPtWithRound(rd);
            var twoDPoints = new List<(TwoDPoint, CondAfterCross, CondAfterCross)>();
// #if DEBUG
//             Console.Out.WriteLine($" line {Log()} vs cross {rd.Log()} pt {item1?.Log()} :: {item2?.Log()} ");
//
// #endif
            if (item1 == null || item2 == null) return twoDPoints;


            var f1 = GetScaleInPt(item1);
            var f2 = GetScaleInPt(item2);

            var (p1, p2) = f1 < f2 ? (item1, item2) : (item2, item1);

            var f11 = MathTools.Min(f1, f2);
            var f22 = MathTools.Max(f1, f2);
#if DEBUG
            Console.Out.WriteLine($"{ToString()}maybe have cross pt {p1}  " + f11 + $"== {p2}  " + f22);

#endif
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
                        ? (p1, CondAfterCross.ToIn, CondAfterCross.ToOut)
                        : (p1, CondAfterCross.MaybeOutToIn, CondAfterCross.ToOut));

                var cb3 = cAobB.GetPosOf(this) == Pt2LinePos.Right;
                var cb4 = cAobA.GetPosOf(op2) != Pt2LinePos.Right;
                var f2B1 = f2B && cb3 && cb4;
                if (f2B1) twoDPoints.Add((p2, CondAfterCross.ToOut, CondAfterCross.ToIn));
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
                        ? (p1, CondAfterCross.ToIn, CondAfterCross.ToOut)
                        : (p1, CondAfterCross.MaybeOutToIn, CondAfterCross.ToOut));

                var b5 = cAobA.GetPosOf(this) != Pt2LinePos.Right;
                var b6 = cAobB.GetPosOf(op2) == Pt2LinePos.Right;
                var f2B1 = b5 && b6 && f2B;

//                Console.Out.WriteLine("?p2?" + b5 + b6 + f2B);
                if (f2B1) twoDPoints.Add((p2, CondAfterCross.ToOut, CondAfterCross.ToIn));
            }

            return twoDPoints;
        }


        private List<(TwoDPoint crossPt, CondAfterCross shape1CrossCond, CondAfterCross shape2CrossCond)> UnionByLine(
            TwoDVectorLine line)
        {
            var twoDPoints = new List<(TwoDPoint, CondAfterCross, CondAfterCross)>();

//             if (SameLine(line) && GetVector().Dot(line.GetVector()) < 0)
//             {
// #if DEBUG
//                 Console.Out.WriteLine($"same Line block:: {this}  vs {line}");
// #endif
//                 var lineB = line.GetEndPt();
//                 var endM = GetMultiFromA(lineB);
//                 if (endM >= 1) return twoDPoints;
//                 var lineA = line.GetStartPt();
//                 var startM = GetMultiFromA(lineA);
//                 if (startM <= 0) return twoDPoints;
//
//                 if (endM > 0)
//                 {
//                     twoDPoints.Add((lineB, CondAfterCross.MaybeOutToIn, CondAfterCross.ToOut));
//                 }
//                 if (startM <= 1)
//                 {
//                     var valueTuple = (lineA, CondAfterCross.ToOut, CondAfterCross.MaybeOutToIn);
//                     twoDPoints.Add(valueTuple);
//                 }
//
//                 return twoDPoints;
//             }

            var pt = CrossPointForWholeLine(line);
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
#if DEBUG
            // Console.Out.WriteLine($"{Log()}maybe have cross pt {pt.Log()}  ");

#endif
            if (c1)
                twoDPoints.Add(b11
                    ? (pt, CondAfterCross.ToIn, CondAfterCross.ToIn)
                    : (pt, CondAfterCross.MaybeOutToIn, CondAfterCross.ToIn));

            else if (c2)
                twoDPoints.Add(b11
                    ? (pt, CondAfterCross.ToIn, CondAfterCross.ToOut)
                    : (pt, CondAfterCross.MaybeOutToIn, CondAfterCross.ToOut));
            else if (c3)
                twoDPoints.Add((pt, CondAfterCross.ToOut, CondAfterCross.ToIn));
            else if (c4) twoDPoints.Add((pt, CondAfterCross.ToOut, CondAfterCross.ToOut));

            return twoDPoints;
        }

        private bool SameLine(TwoDVectorLine line)
        {
            return A.GetPosOf(line) == Pt2LinePos.On && B.GetPosOf(line) == Pt2LinePos.On;
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
            return new SimpleBlocks(blockShapes);
        }
    }
}