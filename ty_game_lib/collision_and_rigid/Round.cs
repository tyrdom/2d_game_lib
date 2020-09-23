using System;

namespace collision_and_rigid
{
    public class Round : IRawBulletShape, IBulletShape,IShape
    {
        public TwoDPoint O;
        public float R;

        public Round(TwoDPoint o, float r)
        {
            O = o;
            R = r;
        }


        public (float? left, float? right) GetX(float y)
        {
            var oY = y - O.Y;
            var f = R * R - oY * oY;
            if (f < 0) return (null, null);

            if (!(f > 0)) return (null, O.X);

            var sqrt = MathTools.Sqrt(f);


            return (O.X - sqrt, O.X + sqrt);
        }

        public (float? down, float? up) GetY(float x)
        {
            var oY = x - O.X;
            var f = R * R - oY * oY;
            if (f < 0) return (null, null);

            if (!(f > 0)) return (null, O.Y);

            var sqrt = MathTools.Sqrt(f);

            return (O.Y - sqrt, O.Y + sqrt);
        }

        public Zone GetZones()
        {
            return new Zone(O.Y + R, O.Y - R, O.X - R, O.X + R);
        }

        public bool IsTouch(Round another)
        {
            var rr = another.R + R;
            var oX = O.X - another.O.X;
            var oY = O.Y - another.O.Y;
            return
                rr * rr >=
                oX * oX + oY * oY;
        }

        public (TwoDPoint?, TwoDPoint?) GetCrossPt(Round another)
        {
            var r2 = another.R;
            var rr = r2 + R;
            var rd = R - r2;
            var oX = -O.X + another.O.X;
            var oY = -O.Y + another.O.Y;

            var dSq = oX * oX + oY * oY;
            var d = MathTools.Sqrt(dSq);


            if (rr * rr < dSq || rd * rd > dSq || dSq <= 0)
            {
                return (null, null);
            }

            var sq = (R * R - r2 * r2 + dSq) / (2 * d);


            var h = MathTools.Sqrt(R * R - sq * sq);


            var xp = O.X + sq * oX / d;
            var yp = O.Y + sq * oY / d;
            if (h <= 0)
            {
                var twoDPoint = new TwoDPoint(xp, yp);
                return (null, twoDPoint);
            }

            var x1 = xp - h * oY / d;
            var y1 = yp + h * oX / d;
            var x2 = xp + h * oY / d;
            var y2 = yp - h * oX / d;
            var p1 = new TwoDPoint(x1, y1);
            var p2 = new TwoDPoint(x2, y2);
            return (p1, p2);
        }

        public Zone GenBulletZone(float r)
        {
            var moreHigh = GetZones().MoreWide(r).MoreHigh(r);
            return moreHigh;
        }

        public IBulletShape GenBulletShape(float r)
        {
            var round = new Round(O, R + r);
            return round;
        }

        public bool PtRealInShape(TwoDPoint point)
        {
            return TwoDVector.TwoDVectorByPt(O, point).SqNorm() <= R * R;
        }

        public string Log()
        {
            return $"O:: {O.ToString()} R::{R}";
        }
    }
}