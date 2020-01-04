using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;

namespace ty_game_lib
{
    public class ClockwiseTurning : IShape
    {
        public ClockwiseBalanceAngle AOB;
        public float R;
        private TwoDVectorLine? Last;
        public TwoDVectorLine? Next;

        public (bool, ClockwiseTurning, ClockwiseTurning) TouchAnotherOne(ClockwiseTurning another)
        {
            var angle1 = AOB;
            var angle2 = another.AOB;
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
                    if (pt2.GetPosOnLine(twoDVectorLine) == Pt2LinePos.Left
                        && pt2.GetPosOnLine(line2) == Pt2LinePos.Right)
                    {
                        pp = pt2;
                    }
                    else if (pt2.GetPosOnLine(twoDVectorLine) == Pt2LinePos.Right
                             && pt2.GetPosOnLine(line2) == Pt2LinePos.Left)
                    {
                        pp = pt1;
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
                var clockwiseBalanceAngle = new ClockwiseBalanceAngle(angle1.A, angle1.O, pp);
                var clockwiseTurning = new ClockwiseTurning(clockwiseBalanceAngle, R, Last, null);

                var clockwiseBalanceAngle2 = new ClockwiseBalanceAngle(pp, angle2.O, angle2.B);
                var clockwiseTurning2 = new ClockwiseTurning(clockwiseBalanceAngle2, R, null, Next);
                return (true, clockwiseTurning, clockwiseTurning2);
            }

            {
                return (false, this, another);
            }
        }

        public (bool, ClockwiseTurning, TwoDVectorLine) TouchByLine(TwoDVectorLine line)
        {
            var o = AOB.O;
            var rd = new Round(o, R);
            var dVectorLine = new TwoDVectorLine(o, line.B);

            var (pt1, pt2) = line.CrossPtWithRound(rd);
            if (pt1 == null || dVectorLine.GetVector().SqNorm() <= R * R)
            {
                return (false, this, line);
            }

            TwoDPoint? pp = null;
            var cover1 = AOB.Cover(pt1) && line.GetMultiFromA(pt1) < 1 && line.GetMultiFromA(pt1) > 0;
            var cover2 = AOB.Cover(pt2) && line.GetMultiFromA(pt2) < 1 && line.GetMultiFromA(pt2) > 0;
            if (cover1)
            {
                if (cover2)
                {
                    var twoDVectorLine = new TwoDVectorLine(AOB.O, pt1);
                    if (pt2.GetPosOnLine(twoDVectorLine) == Pt2LinePos.Left &&
                        line.GetMultiFromA(pt2) > line.GetMultiFromA(pt1))
                    {
                        pp = pt2;
                    }
                    else if (pt2.GetPosOnLine(twoDVectorLine) == Pt2LinePos.Right &&
                             line.GetMultiFromA(pt2) < line.GetMultiFromA(pt1))
                    {
                        pp = pt1;
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
                var clockwiseBalanceAngle = new ClockwiseBalanceAngle(AOB.A, AOB.O, pp);
                var clockwiseTurning = new ClockwiseTurning(clockwiseBalanceAngle, R, Last, null);

                var newLine = new TwoDVectorLine(pp, line.B);

                return (true, clockwiseTurning, newLine);
            }
            else
            {
                return (false, this, line);
            }
        }


        public ClockwiseTurning(ClockwiseBalanceAngle aob, float r, TwoDVectorLine? last, TwoDVectorLine? next)
        {
            if (aob.CheckTuring())
            {
                AOB = aob;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            R = r;

            Last = last;
            Next = next;
        }


        public TwoDPoint Slide(TwoDPoint p)
        {
            var a = AOB.A;
            var b = AOB.B;
            var o = AOB.O;

            var oa = new TwoDVectorLine(o, a);
            var ob = new TwoDVectorLine(o, b);
            var oaPos = p.GetPosOnLine(oa);
            var obPos = p.GetPosOnLine(ob);

            switch (oaPos)
            {
                case Pt2LinePos.Right:
                    switch (obPos)
                    {
                        case Pt2LinePos.Right:
                            return Next != null ? Next.Slide(p) : b;
                            break;
                        case Pt2LinePos.On:
                            return b;
                            break;
                        case Pt2LinePos.Left:
                            var ovr = new TwoDVectorLine(o, p).GetVector().GetUnit().Multi(R);
                            return o.move(ovr);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case Pt2LinePos.On:
                    switch (obPos)
                    {
                        case Pt2LinePos.Right:
                            return Next != null ? Next.Slide(p) : b;
                            break;
                        case Pt2LinePos.On:
                            var twoDVector = new TwoDVectorLine(a, b).GetVector().CounterClockwiseHalfPi().GetUnit()
                                .Multi(R);
                            return o.move(twoDVector);
                            break;
                        case Pt2LinePos.Left:
                            return a;
                            break;
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
                            var mPos = p.GetPosOnLine(om);
                            return mPos switch
                                {
                                    Pt2LinePos.Right => Next != null ? Next.Slide(p) : b,
                                    Pt2LinePos.On => m,
                                    Pt2LinePos.Left => Last != null ? Last.Slide(p) : a,
                                    _ => throw new ArgumentOutOfRangeException()
                                }
                                ;

                            break;
                        case Pt2LinePos.On:
                            return Last != null ? Last.Slide(p) : a;
                            break;
                        case Pt2LinePos.Left:
                            return Last != null ? Last.Slide(p) : a;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ClockwiseTurning FixArBr()
        {
            var twoDVector = new TwoDVectorLine(AOB.O, AOB.A).GetVector().GetUnit().Multi(R);
            var twoDPointA = AOB.O.move(twoDVector);
            var twoDVector1 = new TwoDVectorLine(AOB.O, AOB.B).GetVector().GetUnit().Multi(R);
            var twoDPointB = AOB.O.move(twoDVector1);
            var angle = new ClockwiseBalanceAngle(twoDPointA, AOB.O, twoDPointB);
            return new ClockwiseTurning(angle, R, Last, Next);
        }


        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = AOB.GetZone(R);
            return new AabbBoxShape(zone, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            var o = AOB.O;
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


        public (Zone?, Zone?) CutByH(float h, Zone z)
        {
            var round = new Round(AOB.O, R);
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

                if (h > AOB.O.Y)
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

                if (h < AOB.O.Y)
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
            var o = AOB.O;
            var a = AOB.A;
            var b = AOB.B;
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

                if (v > AOB.O.X)
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

                if (v < AOB.O.X)
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
            var a = AOB.A;
            var b = AOB.B;
            var o = AOB.O;
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
            var lP = Last.B;
            var rP = Next.A;
            var getPosOnLine = sPoint.GetPosOnLine(Last);

            var pt2LinePos = sPoint.GetPosOnLine(Next);
            if (getPosOnLine == Pt2LinePos.Left)
            {
                var slLine = new TwoDVectorLine(sPoint, lP);
                var linePos = ePoint.GetPosOnLine(slLine);
                if (linePos != Pt2LinePos.Left)
                {
                    return false;
                }
            }

            if (pt2LinePos == Pt2LinePos.Left)
            {
                var srLine = new TwoDVectorLine(sPoint, rP);
                var linePos = ePoint.GetPosOnLine(srLine);
                if (linePos == Pt2LinePos.Left)
                {
                    return false;
                }
            }

            return true;
        }
    }
}