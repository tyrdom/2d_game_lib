using System;


namespace collision_and_rigid
{
    public class ClockwiseBalanceAngle
    {
        public readonly TwoDPoint A;
        public readonly TwoDPoint B;
        public readonly TwoDPoint O;

        public ClockwiseBalanceAngle(TwoDPoint a, TwoDPoint o, TwoDPoint b)
        {
            A = a;
            O = o;
            B = b;
        }


        public float GetRad()
        {
            var oa = new TwoDVector(O, A);
            var ob = new TwoDVector(O, B);
            var cos = oa.GetCos(ob);
            var acos = MathTools.Acos(cos);
            return acos;
        }

        public string Log()
        {
            return $"{A}\\{O}/{B}";
        }

        public bool CheckTuring()
        {
            var twoDVectorLine = new TwoDVectorLine(A, O);
            var getposOnLine = B.GetPosOf(twoDVectorLine);

            return getposOnLine != Pt2LinePos.Right;
        }

        public bool Cover(TwoDPoint? pt)
        {
            if (pt == null)
            {
                return false;
            }

            var oa = new TwoDVectorLine(O, A);
            var ob = new TwoDVectorLine(O, B);
            var getPosOnLine = pt.GetPosOf(oa);
            var pt2LinePos = pt.GetPosOf(ob);
            var b = getPosOnLine != Pt2LinePos.Left && pt2LinePos != Pt2LinePos.Right;
            return b;
        }

        public bool BlockUseCover(TwoDPoint pt)
        {
            var opt = new TwoDVectorLine(O, pt);
            var aPos = A.GetPosOf(opt);
            var bPos = B.GetPosOf(opt);
            var b = aPos == Pt2LinePos.Left && bPos != Pt2LinePos.Left;
            return b;
        }

        public bool RealLessThanEnd(TwoDPoint pt)
        {
            var opt = new TwoDVectorLine(O, pt);
            var bPos = B.GetPosOf(opt);
            return bPos == Pt2LinePos.Right;
        }

        public bool RealCover(TwoDPoint pt)
        {
            var oa = new TwoDVectorLine(O, A);
            var ob = new TwoDVectorLine(O, B);
            var getPosOnLine = pt.GetPosOf(oa);
            var pt2LinePos = pt.GetPosOf(ob);
            var b = getPosOnLine == Pt2LinePos.Right && pt2LinePos == Pt2LinePos.Left;
            return b;
        }

        public Zone GetZone(float r)
        {
            var oa = new TwoDVectorLine(O, A);
            var oav = oa.GetVector();
            var ob = new TwoDVectorLine(O, B);
            var obv = ob.GetVector();
            var oaq = oav.WhichQ();
            var obq = obv.WhichQ();

            var up = MathTools.Max(A.Y, B.Y) + r;
            var down = MathTools.Min(A.Y, B.Y) - r;
            var right = MathTools.Max(A.X, B.X) + r;
            var left = MathTools.Min(A.X, B.X) - r;
            var rr = oav.Norm() + r;
            var oUp = O.Y + rr;
            var oDown = O.Y - rr;
            var oLeft = O.X - rr;
            var oRight = O.X + rr;
            var getposOnLine = B.GetPosOf(oa);

            if (oaq == obq)
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
            else
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
                                up = oUp;
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

            var zone = new Zone(up, down, left, right);
            return zone;
        }
    }
}