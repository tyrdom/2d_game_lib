using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace ty_game_lib
{
    public class ClockwiseTurning : IShape
    {
        public ClockwiseBalanceAngle AOB;
        private float R;
        private TwoDVectorLine? Last;
        private TwoDVectorLine? Next;

        TwoDPoint? TouchAnotherOne(ClockwiseTurning another)
        {
            var r1 = new Round(AOB.O,R);
            var r2 = new Round(another.AOB.O,another.R);
            
        }

        TwoDPoint? TouchByLine(TwoDVectorLine line)
        {
            throw new NotImplementedException();
        }

        public ClockwiseTurning? CutBy(IShape shape)
        {
            switch (shape)
            {
                case ClockwiseTurning clockwiseTurning:
                    
                    break;
                case TwoDVectorLine twoDVectorLine:
                    break;
            }

            throw new NotImplementedException();
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
            var oaPos = p.GetposOnLine(oa);
            var obPos = p.GetposOnLine(ob);

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
                            var mPos = p.GetposOnLine(om);
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
            var a = AOB.A;
            var b = AOB.B;
            var o = AOB.O;
            var oa = new TwoDVectorLine(o, a);
            var oav = oa.GetVector();
            var ob = new TwoDVectorLine(o, b);
            var obv = ob.GetVector();
            var oaq = oav.WhichQ();
            var obq = obv.WhichQ();
            var up = MathF.Max(a.Y, b.Y);
            var down = MathF.Min(a.Y, b.Y);
            var right = MathF.Max(a.X, b.X);
            var left = MathF.Min(a.X, b.X);

            var oUp = o.Y + R;
            var oDown = o.Y - R;
            var oLeft = o.X - R;
            var oRight = o.X + R;
            var getposOnLine = b.GetposOnLine(oa);

            if (oaq == obq)
            {
                switch (getposOnLine)
                {
                    case Pt2LinePos.Right:
                        break;
                    case Pt2LinePos.On:
                        break;
                    case Pt2LinePos.Left:
                        up = oUp;
                        down = oDown;
                        left = oLeft;
                        right = oRight;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (oaq)
                {
                    case Quad.One:
                        right = oRight;
                        switch (obq)
                        {
                            case Quad.Two:
                                down = oDown;
                                left = oLeft;
                                break;
                            case Quad.Three:
                                down = oDown;

                                break;
                            case Quad.Four:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case Quad.Two:
                        up = oUp;
                        switch (obq)
                        {
                            case Quad.One:
                                break;

                            case Quad.Three:
                                left = oLeft;
                                down = oDown;
                                break;
                            case Quad.Four:
                                left = oLeft;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case Quad.Three:
                        left = oLeft;
                        switch (obq)
                        {
                            case Quad.One:
                                up = oUp;
                                break;
                            case Quad.Two:
                                break;

                            case Quad.Four:
                                up = oUp;
                                right = oRight;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case Quad.Four:
                        down = oDown;
                        switch (obq)
                        {
                            case Quad.One:
                                left = oLeft;
                                up = oUp;
                                break;
                            case Quad.Two:
                                left = oLeft;
                                break;
                            case Quad.Three:
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            return new AabbBoxShape(new Zone(up, down, left, right), this);
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

        public List<AabbBoxShape> CovToVertAabbPackBoxes()
        {
            var zones = new List<Zone>();


            var a = AOB.A;
            var b = AOB.B;
            var o = AOB.O;
            var u = o.move(new TwoDVector(0, R));
            var d = o.move(new TwoDVector(0, -R));
            var oa = new TwoDVectorLine(o, a);
            var oav = oa.GetVector();
            var ob = new TwoDVectorLine(o, b);
            var obv = ob.GetVector();
//            var oaq = oav.WhichQ();
//            var obq = obv.WhichQ();


            if (oav.X > 0)


            {
                if (obv.X >= 0)
                {
                    zones.Add(CovToAabbPackBox().Zone);
                }
                else
                {
                    var aod = new ClockwiseBalanceAngle(a, o, d);
                    var dob = new ClockwiseBalanceAngle(d, o, b);
                    zones.Add(new ClockwiseTurning(aod, R, Last, Next).CovToAabbPackBox().Zone);
                    zones.Add(new ClockwiseTurning(dob, R, Last, Next).CovToAabbPackBox().Zone);
                }
            }
            else if (oav.X < 0)
            {
                if (obv.X <= 0)
                {
                    zones.Add(CovToAabbPackBox().Zone);
                }
                else
                {
                    var aod = new ClockwiseBalanceAngle(a, o, u);
                    var dob = new ClockwiseBalanceAngle(u, o, b);
                    zones.Add(new ClockwiseTurning(aod, R, Last, Next).CovToAabbPackBox().Zone);
                    zones.Add(new ClockwiseTurning(dob, R, Last, Next).CovToAabbPackBox().Zone);
                }
            }
            else
            {
                zones.Add(CovToAabbPackBox().Zone);
            }

            return zones.Select(zone => new AabbBoxShape(zone, this)).ToList();
        }

        public bool IsCross(TwoDVectorLine twoDVectorLine)
        {
            var sPoint = twoDVectorLine.A;
            var ePoint = twoDVectorLine.B;
            var lP = Last.B;
            var rP = Next.A;
            var getposOnLine = sPoint.GetposOnLine(Last);
            var pt2LinePos = sPoint.GetposOnLine(Next);

            if (getposOnLine == Pt2LinePos.Left)
            {
                var slLine = new TwoDVectorLine(sPoint, lP);
                var linePos = ePoint.GetposOnLine(slLine);
                if (linePos != Pt2LinePos.Left)
                {
                    return false;
                }
            }


            if (pt2LinePos == Pt2LinePos.Left)
            {
                var srLine = new TwoDVectorLine(sPoint, rP);
                var linePos = ePoint.GetposOnLine(srLine);
                if (linePos == Pt2LinePos.Left)
                {
                    return false;
                }
            }

            return true;
        }
    }
}