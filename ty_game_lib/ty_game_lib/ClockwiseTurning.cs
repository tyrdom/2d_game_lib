using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace ty_game_lib
{
    public class ClockwiseTurning : Shape
    {
        public ClockwiseBalanceAngle AOB;
        private float R;
        private Shape Last;
        private Shape Next;


        public ClockwiseTurning(ClockwiseBalanceAngle aob, float r, Shape last, Shape next)
        {
            AOB = aob;
            R = r;
            Last = last;
            Next = next;
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
                        left = oLeft;
                        switch (obq)
                        {
                            case Quad.Two:
                                down = oDown;
                                right = oRight;
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
            var oaq = oav.WhichQ();
            var obq = obv.WhichQ();
            var up = MathF.Max(a.Y, b.Y);
            var down = MathF.Min(a.Y, b.Y);
            var right = MathF.Max(a.X, b.X);
            var left = MathF.Min(a.X, b.X);
         
            switch (oaq)
            {
                case Quad.One when oaq == Quad.Four:
                {
                    if (obq == Quad.One && obq == Quad.Four)
                    {
                        if (a.Y > b.Y)
                        {
                            zones.Add(CovToAabbPackBox().Zone);
                        }
                        else if (a.Y < b.Y)
                        {
                            var aod = new ClockwiseBalanceAngle(a, o, d);
                            var dou = new ClockwiseBalanceAngle(d, o, u);
                            var uob = new ClockwiseBalanceAngle(u, o, b);
                            zones.Add(new ClockwiseTurning(aod, R, Last, Next).CovToAabbPackBox().Zone);
                            zones.Add(new ClockwiseTurning(dou, R, Last, Next).CovToAabbPackBox().Zone);
                            zones.Add(new ClockwiseTurning(uob, R, Last, Next).CovToAabbPackBox().Zone);
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        var aod = new ClockwiseBalanceAngle(a, o, d);
                        var dob = new ClockwiseBalanceAngle(d, o, b);
                        zones.Add(new ClockwiseTurning(aod, R, Last, Next).CovToAabbPackBox().Zone);
                        zones.Add(new ClockwiseTurning(dob, R, Last, Next).CovToAabbPackBox().Zone);
                    }

                    break;
                }
                case Quad.Two when oaq == Quad.Three:
                    if (obq == Quad.Two && obq == Quad.Three)
                    {
                        if (a.Y < b.Y)
                        {
                            zones.Add(CovToAabbPackBox().Zone);
                        }
                        else if (a.Y > b.Y)
                        {
                            var aou = new ClockwiseBalanceAngle(a, o, u);
                            var uod = new ClockwiseBalanceAngle(u, o, d);
                            var dob = new ClockwiseBalanceAngle(d, o, b);
                            zones.Add(new ClockwiseTurning(aou, R, Last, Next).CovToAabbPackBox().Zone);
                            zones.Add(new ClockwiseTurning(uod, R, Last, Next).CovToAabbPackBox().Zone);
                            zones.Add(new ClockwiseTurning(dob, R, Last, Next).CovToAabbPackBox().Zone);
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        var aod = new ClockwiseBalanceAngle(a, o, u);
                        var dob = new ClockwiseBalanceAngle(u, o, b);
                        zones.Add(new ClockwiseTurning(aod, R, Last, Next).CovToAabbPackBox().Zone);
                        zones.Add(new ClockwiseTurning(dob, R, Last, Next).CovToAabbPackBox().Zone);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            var aabbBoxShapes = new List<AabbBoxShape>();
            foreach (var zone in zones)
            {
                var aabbBoxShape = new AabbBoxShape(zone,this);
                aabbBoxShapes.Add(aabbBoxShape);
            }

            return aabbBoxShapes;
        }
    }
}