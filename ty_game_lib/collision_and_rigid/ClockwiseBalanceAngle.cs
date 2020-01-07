using System;
using System.Diagnostics.SymbolStore;

namespace collision_and_rigid
{
    public class ClockwiseBalanceAngle
    {
        public TwoDPoint A;
        public TwoDPoint O;
        public TwoDPoint B;

        public ClockwiseBalanceAngle(TwoDPoint a, TwoDPoint o, TwoDPoint b)
        {
            A = a;
            O = o;
            B = b;
        }

        public bool CheckTuring()
        {
            var twoDVectorLine = new TwoDVectorLine(A, O);
            var getposOnLine = B.game_stuff(twoDVectorLine);

            return getposOnLine != Pt2LinePos.Right;
        }

        public bool Cover(TwoDPoint pt)
        {
            var oa = new TwoDVectorLine(O, A);
            var ob = new TwoDVectorLine(O, B);
            var getPosOnLine = pt.game_stuff(oa);
            var pt2LinePos = pt.game_stuff(ob);
            var b = getPosOnLine != Pt2LinePos.Left && pt2LinePos != Pt2LinePos.Right;
            return b;
        }

         public Zone GetZone(float r)
        {
            var a = A;
            var b = B;
            var o = O;
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

            var oUp = o.Y + r;
            var oDown = o.Y - r;
            var oLeft = o.X - r;
            var oRight = o.X + r;
            var getposOnLine = b.game_stuff(oa);

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

            var zone = new Zone(up, down, left, right);
            return zone;
        }
    }
}