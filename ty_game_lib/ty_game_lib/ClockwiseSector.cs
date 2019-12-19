using System;

namespace ty_game_lib
{
    public class ClockwiseSector : Shape
    {
        private ClockwiseAngle AOB;
        private float R;


        public ClockwiseSector(ClockwiseAngle a, float r)
        {
            AOB = a;
            R = r;
        }

        public ClockwiseSector FixArBr()
        {
            var twoDVector = new TwoDVectorLine(AOB.O, AOB.A).GetVector().GetUnit().Multi(R);
            var twoDPointA = AOB.O.move(twoDVector);
            var twoDVector1 = new TwoDVectorLine(AOB.O, AOB.B).GetVector().GetUnit().Multi(R);
            var twoDPointB = AOB.O.move(twoDVector1);
            var angle = new ClockwiseAngle(twoDPointA, AOB.O, twoDPointB);
            return new ClockwiseSector(angle, R);
        }

        public AabbPackBox CovToAabbPackBox()
        {
            var fixAb = FixArBr();
            var fixAbAob = fixAb.AOB;
            var a = fixAbAob.A;
            var b = fixAbAob.B;
            var o = fixAbAob.O;
            var oa = new TwoDVectorLine(o, a);
            var oav = oa.GetVector();
            var ob = new TwoDVectorLine(o, b);
            var obv = ob.GetVector();
            var oaq = oav.WhichQ();
            var obq = obv.WhichQ();
            var up = MathF.Max(o.Y, MathF.Max(a.Y, b.Y));
            var down = MathF.Min(o.Y, MathF.Min(a.Y, b.Y));
            var right = MathF.Max(o.X, MathF.Max(a.X, b.X));
            var left = MathF.Min(o.X, MathF.Min(a.X, b.X));

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


            return new AabbPackBox(new Zone(up, down, left, right), this);
        }
    }
}