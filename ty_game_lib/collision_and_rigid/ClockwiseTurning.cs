using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace collision_and_rigid
{
    public class ClockwiseTurning : IShape, IBlockShape
    {
        public readonly ClockwiseBalanceAngle Aob;
        public readonly float R;
        public readonly TwoDVectorLine? Last;
        public readonly TwoDVectorLine? Next;
        private readonly TwoDVectorLine _tangentA;
        private readonly TwoDVectorLine _tangentB;

        public ClockwiseTurning(ClockwiseBalanceAngle aob, float r, TwoDVectorLine? last, TwoDVectorLine? next)
        {
            if (aob.CheckTuring())
            {
                Aob = aob;
                _tangentA = new TwoDVectorLine(Aob.A, Aob.O).CounterClockwiseHalfPi();
                _tangentB = new TwoDVectorLine(Aob.B, Aob.O).CounterClockwiseHalfPi();
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            R = r;

            Last = last;
            Next = next;
        }

        public Round BelongRd()
        {
            return new Round(Aob.O, R);
        }

        public (bool, ClockwiseTurning, ClockwiseTurning) TouchAnotherOne(ClockwiseTurning another)
        {
            var angle1 = Aob;
            var angle2 = another.Aob;
            var r1 = new Round(angle1.O, R);
            var r2 = new Round(angle2.O, another.R);
            var (pt1, pt2) = r1.GetCrossPt(r2);

            if (pt1 == null)
            {
                return (false, this, another);
            }

            TwoDPoint? pp = null;

            var cover1 = angle1.Cover(pt1) && angle2.Cover(pt1);
            var cover2 = angle1.Cover(pt2) && angle2.Cover(pt2);
            if (cover1)
            {
                if (cover2)
                {
                    var twoDVectorLine = new TwoDVectorLine(angle1.O, pt1);
                    var line2 = new TwoDVectorLine(angle2.O, pt1);
                    if (pt2.GetPosOf(twoDVectorLine) == Pt2LinePos.Left
                        && pt2.GetPosOf(line2) == Pt2LinePos.Right)
                    {
                        pp = pt2;
                    }
                    else if (pt2.GetPosOf(twoDVectorLine) == Pt2LinePos.Right
                             && pt2.GetPosOf(line2) == Pt2LinePos.Left)
                    {
                        pp = pt1;
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

            if (pp == null) return (false, this, another);
            var clockwiseBalanceAngle = new ClockwiseBalanceAngle(angle1.A, angle1.O, pp);
            var clockwiseTurning = new ClockwiseTurning(clockwiseBalanceAngle, R, Last, null);

            var clockwiseBalanceAngle2 = new ClockwiseBalanceAngle(pp, angle2.O, angle2.B);
            var clockwiseTurning2 = new ClockwiseTurning(clockwiseBalanceAngle2, R, null, Next);
            return (true, clockwiseTurning, clockwiseTurning2);
        }

        public (bool, ClockwiseTurning, TwoDVectorLine) TouchByLine(TwoDVectorLine line)
        {
            var o = Aob.O;
            var rd = new Round(o, R);
            var dVectorLine = new TwoDVectorLine(o, line.B);

            var (pt1, pt2) = line.CrossPtWithRound(rd);
            if (pt1 == null || dVectorLine.GetVector().SqNorm() <= R * R)
            {
                return (false, this, line);
            }

            TwoDPoint? pp = null;
            var cover1 = Aob.Cover(pt1) && line.GetMultiFromA(pt1) <= 1 && line.GetMultiFromA(pt1) >= 0;
            var cover2 = Aob.Cover(pt2) && line.GetMultiFromA(pt2) <= 1 && line.GetMultiFromA(pt2) >= 0;
            if (cover1)
            {
                if (cover2)
                {
                    var twoDVectorLine = new TwoDVectorLine(Aob.O, pt1);
                    if (pt2.GetPosOf(twoDVectorLine) == Pt2LinePos.Left &&
                        line.GetMultiFromA(pt2) > line.GetMultiFromA(pt1))
                    {
                        pp = pt2;
                    }
                    else if (pt2.GetPosOf(twoDVectorLine) == Pt2LinePos.Right &&
                             line.GetMultiFromA(pt2) < line.GetMultiFromA(pt1))
                    {
                        pp = pt1;
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

            if (pp == null) return (false, this, line);
            var clockwiseBalanceAngle = new ClockwiseBalanceAngle(Aob.A, Aob.O, pp);
            var clockwiseTurning = new ClockwiseTurning(clockwiseBalanceAngle, R, Last, null);

            var newLine = new TwoDVectorLine(pp, line.B);

            return (true, clockwiseTurning, newLine);
        }


        public TwoDPoint Slide(TwoDPoint p)
        {
            var a = Aob.A;
            var b = Aob.B;
            var o = Aob.O;

            var oa = new TwoDVectorLine(o, a);
            var ob = new TwoDVectorLine(o, b);
            var oaPos = p.GetPosOf(oa);
            var obPos = p.GetPosOf(ob);

            switch (oaPos)
            {
                case Pt2LinePos.Right:
                    switch (obPos)
                    {
                        case Pt2LinePos.Right:
                            return Next != null ? Next.Slide(p) : b;
                        case Pt2LinePos.On:
                            return b;
                        case Pt2LinePos.Left:
                            var ovr = new TwoDVectorLine(o, p).GetVector().GetUnit().Multi(R);
                            return o.move(ovr);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case Pt2LinePos.On:
                    switch (obPos)
                    {
                        case Pt2LinePos.Right:
                            return Next != null ? Next.Slide(p) : b;
                        case Pt2LinePos.On:
                            var twoDVector = new TwoDVectorLine(a, b).GetVector().CounterClockwiseHalfPi().GetUnit()
                                .Multi(R);
                            return o.move(twoDVector);
                        case Pt2LinePos.Left:
                            return a;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }


                case Pt2LinePos.Left:
                    switch (obPos)
                    {
                        case Pt2LinePos.Right:

                            var twoDVector = new TwoDVectorLine(a, b).GetVector().CounterClockwiseHalfPi().GetUnit()
                                .Multi(R);
                            var m = o.move(twoDVector);
                            var om = new TwoDVectorLine(o, m);
                            var mPos = p.GetPosOf(om);
                            return mPos switch
                                {
                                    Pt2LinePos.Right => Next != null ? Next.Slide(p) : b,
                                    Pt2LinePos.On => m,
                                    Pt2LinePos.Left => Last != null ? Last.Slide(p) : a,
                                    _ => throw new ArgumentOutOfRangeException()
                                }
                                ;

                        case Pt2LinePos.On:
                            return Last != null ? Last.Slide(p) : a;
                        case Pt2LinePos.Left:
                            return Last != null ? Last.Slide(p) : a;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ClockwiseTurning FixArBr()
        {
            var twoDVector = new TwoDVectorLine(Aob.O, Aob.A).GetVector().GetUnit().Multi(R);
            var twoDPointA = Aob.O.move(twoDVector);
            var twoDVector1 = new TwoDVectorLine(Aob.O, Aob.B).GetVector().GetUnit().Multi(R);
            var twoDPointB = Aob.O.move(twoDVector1);
            var angle = new ClockwiseBalanceAngle(twoDPointA, Aob.O, twoDPointB);
            return new ClockwiseTurning(angle, R, Last, Next);
        }


        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = Aob.GetZone(R);
            return new AabbBoxShape(zone, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            var o = Aob.O;
            var ox = (p.X - o.X);
            var oy = (p.Y - o.Y);
            if (ox * ox + oy * oy < R * R)
            {
                return -1;
            }

            {
                return -2;
            }
        }

        public bool IsTouchAnother(IShape another)
        {
            throw new NotImplementedException();
        }

        public (bool, IBlockShape[]) BlockShapeUnionInSamePloy(IBlockShape another)
        {
            switch (another)
            {
                case ClockwiseTurning clockwiseTurning:
                    var (item1, item2, item3) = TouchAnotherOne(clockwiseTurning);
                    IBlockShape[] clockwiseTurnings = {item2, item3};
                    return (item1, clockwiseTurnings);
                case TwoDVectorLine twoDVectorLine:
                    var (b, clockwiseTurning1, twoDVectorLine1) = TouchByLine(twoDVectorLine);
                    IBlockShape[] bl = {clockwiseTurning1, twoDVectorLine1};
                    return (b, bl);

                default:
                    throw new ArgumentOutOfRangeException(nameof(another));
            }
        }

        public bool IsEmpty()
        {
            var same = Aob.A.Same(Aob.B);
            return same;
        }

        public List<(TwoDPoint, CondAfterCross, CondAfterCross)>
            CrossAnotherBlockShapeReturnCrossPtAndThisCondAnotherCond(
                IBlockShape blockShape)
        {
            switch (blockShape)
            {
                case ClockwiseTurning clockwiseTurning:
                    return UnionByAnotherCt(clockwiseTurning);
                case TwoDVectorLine twoDVectorLine:
                    return UnionByLine(twoDVectorLine);
                default:
                    throw new NotImplementedException();
            }
        }


        List<(TwoDPoint, CondAfterCross, CondAfterCross)> UnionByLine(TwoDVectorLine line)
        {
            var rd = BelongRd();
            var (item1, item2) = line.CrossPtWithRound(rd);
            var pts = new List<(TwoDPoint, CondAfterCross, CondAfterCross)>();
            if (item1 == null)
            {
                return pts;
            }

            var o1 = Aob.O;
            var oi1 = new TwoDVectorLine(o1, item1);
            var (p1, p2) = item2.GetPosOf(oi1) == Pt2LinePos.Right ? (item1, item2) : (item2, item1);

            var f1 = line.GetMultiFromA(p1);
            var f2 = line.GetMultiFromA(p2);

            var p1InAngel = Aob.BlockUseCover(p1);
            var p2InAngel = Aob.BlockUseCover(p2);
            if (f1 < f2)
            {
                var b1 = f1 >= 0 && f1 < 1;
                var b2 = f2 > 0 && f2 <= 1;

                var p1B = b1 && p1InAngel;
                if (p1B)
                {
                    pts.Add((p1, CondAfterCross.InToOut, CondAfterCross.OutToIn));
                }

                var p2B = b2 && p2InAngel;
                if (p2B)
                {
                    pts.Add((p2, CondAfterCross.MaybeOutToIn, CondAfterCross.InToOut));
                }
            }
            else
            {
                var b2 = f1 >= 0 && f1 < 1;
                var b1 = f2 > 0 && f2 <= 1;

                var p1B = b1 && p1InAngel;
                if (p1B)
                {
                    pts.Add((p1, CondAfterCross.MaybeOutToIn, CondAfterCross.InToOut));
                }

                var p2B = b2 && p2InAngel;
                if (p2B)
                {
                    pts.Add((p2, CondAfterCross.InToOut, CondAfterCross.OutToIn));
                }
            }

            return pts;
        }


        List<(TwoDPoint, CondAfterCross, CondAfterCross)> UnionByAnotherCt(ClockwiseTurning clockwiseTurning)
        {
            var twoDPoints = new List<(TwoDPoint, CondAfterCross, CondAfterCross)>();
            var rd1 = BelongRd();
            var rd2 = clockwiseTurning.BelongRd();
            var (item1, item2) = rd1.GetCrossPt(rd2);
            if (item1 == null)
            {
                return twoDPoints;
            }

            var o1 = Aob.O;
            var o2 = clockwiseTurning.Aob.O;
            var a2 = clockwiseTurning.Aob.A;
            var b2 = clockwiseTurning.Aob.B;
            var oi1 = new TwoDVectorLine(o1, item1);
            var (p1, p2) = item2.GetPosOf(oi1) == Pt2LinePos.Right ? (item1, item2) : (item2, item1);
            var o2P1 = new TwoDVectorLine(o2, p1);
            var o2P2 = new TwoDVectorLine(o2, p2);
            var p1InAngel1 = Aob.BlockUseCover(p1);
            var p2InAngel1 = Aob.BlockUseCover(p2);
            var p2B1 = a2.GetPosOf(o2P2) != Pt2LinePos.Right;
            var p2B2 = b2.GetPosOf(o2P2) == Pt2LinePos.Right;
            var p1B1 = a2.GetPosOf(o2P1) == Pt2LinePos.Left;
            var p1B2 = b2.GetPosOf(o2P1) != Pt2LinePos.Left;
            var p1Bb = p1B1 && p1B2 && p1InAngel1;
            var p2Bb = p2B1 && p2B2 && p2InAngel1;
            if (p1Bb)
            {
                twoDPoints.Add((p1, CondAfterCross.MaybeOutToIn, CondAfterCross.InToOut));
            }

            if (p2Bb)
            {
                twoDPoints.Add((p2, CondAfterCross.InToOut, CondAfterCross.OutToIn));
            }

            return twoDPoints;
        }


        public ClockwiseTurning GetAPiece(TwoDPoint a, TwoDPoint b)
        {
            var clockwiseBalanceAngle = new ClockwiseBalanceAngle(a, Aob.O, b);

            return new ClockwiseTurning(clockwiseBalanceAngle, R, a.Same(Aob.A) ? Last : null,
                b.Same(Aob.B) ? Next : null);
        }

        public TwoDPoint GetStartPt()
        {
            return Aob.A;
        }

        public TwoDPoint GetEndPt()
        {
            return Aob.B;
        }

        public (List<IBlockShape>, CondAfterCross, List<IBlockShape>) CutByPointReturnGoodBlockCondAndTemp(
            CondAfterCross nowCond,
            List<(TwoDPoint, CondAfterCross)>? ptsAndCond, List<IBlockShape> temp)
        {
            if (ptsAndCond == null)
            {
                ptsAndCond = new List<(TwoDPoint, CondAfterCross)>();
            }
            else
            {
                ptsAndCond.Sort(
                    (p1, p2) =>
                    {
                        var twoDPoint1 = p1.Item1;
                        var twoDPoint2 = p2.Item1;
                        var op2 = new TwoDVectorLine(Aob.O, twoDPoint2);
                        var pt2LinePos = twoDPoint1.GetPosOf(op2);
                        var i = pt2LinePos switch
                        {
                            Pt2LinePos.Right => 1,
                            Pt2LinePos.On => 0,
                            Pt2LinePos.Left => -1,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        return i;
                    });
            }

            var blockShapes = new List<IBlockShape>();
            var nPt = Aob.A;
            foreach (var (pt, cond) in ptsAndCond)
            {
                switch (cond)
                {
                    case CondAfterCross.OutToIn:
                    {
                        var twoDVectorLine = GetAPiece(nPt, pt);
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
                            var twoDVectorLine = GetAPiece(nPt, pt);

                            temp.Add(twoDVectorLine);
                        }
                        else
                        {
                            Console.Out.WriteLine("Some Cond unexpected::" + nowCond.ToString() + " and " + cond.ToString());
                        }

                        break;
                    }
                }


                nowCond = cond;
                nPt = pt;
            }

            switch (nowCond)
            {
                case CondAfterCross.OutToIn:
                    break;
                case CondAfterCross.InToOut:
                    var twoDVectorLine = GetAPiece(nPt, Aob.B);
                    blockShapes.Add(twoDVectorLine);
                    break;
                case CondAfterCross.MaybeOutToIn:

                    var twoDVectorLine2 = GetAPiece(nPt, Aob.B);
                    temp.Add(twoDVectorLine2);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(nowCond), nowCond, null);
            }


            return (blockShapes, nowCond, temp);
        }

        public (Zone?, Zone?) CutByH(float h, Zone z)
        {
            var round = new Round(Aob.O, R);
            var (item1, item2) = round.GetX(h);

            if (item1 == null)
            {
                return (null, null);
            }


            var b = item1.Value < z.Right && item1.Value > z.Left;
            var b1 = item2.Value < z.Right && item2.Value > z.Left;

            if (b)
            {
                var pt = new TwoDPoint(item1.Value, h);

                if (h > Aob.O.Y)
                {
                    var uZone = z.RU().GenZone(pt);
                    var dZone = pt.GenZone(z.LD());
                    return (uZone, dZone);
                }
                else
                {
                    var uZone = z.LU().GenZone(pt);
                    var dZone = pt.GenZone(z.RD());
                    return (uZone, dZone);
                }
            }

            if (!b1) return (null, null);
            {
                var pt = new TwoDPoint(item2.Value, h);

                if (h < Aob.O.Y)
                {
                    var uZone = z.RU().GenZone(pt);
                    var dZone = pt.GenZone(z.LD());
                    return (uZone, dZone);
                }
                else
                {
                    var uZone = z.LU().GenZone(pt);
                    var dZone = pt.GenZone(z.RD());
                    return (uZone, dZone);
                }
            }
        }

        public (Zone?, Zone?) CutByV(float v, Zone z)
        {
            var o = Aob.O;
            var round = new Round(o, R);
            var (item1, item2) = round.GetY(v);

            if (item1 == null)
            {
                return (null, null);
            }

            var b1 = item1.Value < z.Up && item1.Value > z.Down;
            var b2 = item2.Value < z.Up && item2.Value > z.Down;
            if (b1)
            {
                var pt = new TwoDPoint(v, item1.Value);

                if (v > Aob.O.X)
                {
                    var rZone = z.RU().GenZone(pt);
                    var lZone = pt.GenZone(z.LD());
                    return (rZone, lZone);
                }
                else
                {
                    var lZone = z.LU().GenZone(pt);
                    var rZone = pt.GenZone(z.RD());
                    return (rZone, lZone);
                }
            }

            if (b2)
            {
                var pt = new TwoDPoint(v, item2.Value);

                if (v < Aob.O.X)
                {
                    var rZone = z.RU().GenZone(pt);
                    var lZone = pt.GenZone(z.LD());
                    return (lZone, rZone);
                }
                else
                {
                    var lZone = z.LU().GenZone(pt);
                    var rZone = pt.GenZone(z.RD());
                    return (lZone, rZone);
                }
            }

            return (null, null);
        }

        public List<AabbBoxShape> CovToVertAabbPackBoxes()
        {
            var zones = new List<Zone>();
            var a = Aob.A;
            var b = Aob.B;
            var o = Aob.O;
            var u = o.move(new TwoDVector(0, R));
            var d = o.move(new TwoDVector(0, -R));
            var l = o.move(new TwoDVector(-R, 0));
            var r = o.move(new TwoDVector(R, 0));
            var oa = new TwoDVectorLine(o, a);
            var oav = oa.GetVector();
            var ob = new TwoDVectorLine(o, b);
            var obv = ob.GetVector();
            var passU = false;

            var passD
                = false;

            var passL
                = false;

            var passR
                = false;
            if (oav.X < 0 && obv.X > 0)
            {
                passU = true;
            }

            if (oav.X > 0 && obv.X < 0)
            {
                passD = true;
            }

            if (oav.Y < 0 && obv.Y > 0)
            {
                passL = true;
            }

            if (oav.Y > 0 && obv.Y < 0)
            {
                passR = true;
            }

            if (passU)
            {
                if (passR)
                {
                    zones.Add(a.GenZone(u));
                    zones.Add(u.GenZone(r));
                    zones.Add(r.GenZone(b));
                }
                else if (passL)
                {
                    zones.Add(a.GenZone(l));
                    zones.Add(l.GenZone(u));
                    zones.Add(u.GenZone(b));
                }
                else
                {
                    zones.Add(a.GenZone(u));
                    zones.Add(u.GenZone(b));
                }
            }

            else if (passR)
            {
                if (passD)
                {
                    zones.Add(a.GenZone(r));
                    zones.Add(r.GenZone(d));
                    zones.Add(d.GenZone(b));
                }
                else
                {
                    zones.Add(a.GenZone(r));
                    zones.Add(r.GenZone(b));
                }
            }

            else if (passD)
            {
                if (passL)
                {
                    zones.Add(a.GenZone(d));
                    zones.Add(d.GenZone(r));
                    zones.Add(r.GenZone(b));
                }
                else
                {
                    zones.Add(a.GenZone(d));
                    zones.Add(d.GenZone(b));
                }
            }

            else if (passL)
            {
                zones.Add(a.GenZone(l));
                zones.Add(l.GenZone(b));
            }

            else
            {
                zones.Add(a.GenZone(b));
            }

//            {
//            }
//
//
//            if (oav.X > 0)
//
//
//            {
//                if (obv.X >= 0)
//                {
//                    zones.Add(CovToAabbPackBox().Zone);
//                }
//                else
//                {
//                    var aod = new ClockwiseBalanceAngle(a, o, d);
//                    var dob = new ClockwiseBalanceAngle(d, o, b);
//                    zones.Add(aod.GetZone(R));
//                    zones.Add(dob.GetZone(R));
//                }
//            }
//            else if (oav.X < 0)
//            {
//                if (obv.X <= 0)
//                {
//                    zones.Add(CovToAabbPackBox().Zone);
//                }
//                else
//                {
//                    var aod = new ClockwiseBalanceAngle(a, o, u);
//                    var dob = new ClockwiseBalanceAngle(u, o, b);
//                    zones.Add(aod.GetZone(R));
//                    zones.Add(dob.GetZone(R));
//                }
//            }
//            else
//            {
//                zones.Add(CovToAabbPackBox().Zone);
//            }
            return zones.Select(zone => new AabbBoxShape(zone, this)).ToList();
        }


        public bool IsCross(TwoDVectorLine twoDVectorLine)
        {
            var sPoint = twoDVectorLine.A;
            var ePoint = twoDVectorLine.B;
            var lP = Aob.A;
            var rP = Aob.B;
            var getPosOnLine = sPoint.GetPosOf(_tangentA);

            var pt2LinePos = sPoint.GetPosOf(_tangentB);
            if (getPosOnLine == Pt2LinePos.Left)
            {
                var slLine = new TwoDVectorLine(sPoint, lP);
                var linePos = ePoint.GetPosOf(slLine);
                if (linePos != Pt2LinePos.Left)
                {
                    return false;
                }
            }

            if (pt2LinePos == Pt2LinePos.Left)
            {
                var srLine = new TwoDVectorLine(sPoint, rP);
                var linePos = ePoint.GetPosOf(srLine);
                if (linePos == Pt2LinePos.Left)
                {
                    return false;
                }
            }

            return true;
        }
    }
}