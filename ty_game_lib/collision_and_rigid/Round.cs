using System;
using System.Collections.Generic;

namespace ty_game_lib
{
    public class Round : IShape
    {
        public TwoDPoint O;
        public float R;

        public Round(TwoDPoint o, float r)
        {
            O = o;
            R = r;
        }

        public (float?, float?) GetX(float y)
        {
            var oY = y - O.Y;
            var f = R * R - oY * oY;
            if (f < 0)
            {
                return (null, null);
            }

            if (!(f > 0)) return (null, O.X);
            var sqrt = MathF.Sqrt(f);
            return (O.X - sqrt, O.X + sqrt);
        }

        public (float?, float?) GetY(float x)
        {
            var oY = x - O.X;
            var f = R * R - oY * oY;
            if (f < 0)
            {
                return (null, null);
            }

            if (!(f > 0)) return (null, O.Y);
            var sqrt = MathF.Sqrt(f);
            return (O.Y - sqrt, O.Y + sqrt);
        }

        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = new Zone(O.Y + R, O.Y - R, O.X - R, O.X + R);
            return new AabbBoxShape(zone, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            throw new System.NotImplementedException();
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
            var d = MathF.Sqrt(dSq);
            if (rr * rr < dSq || rd * rd > dSq || dSq <= 0)
            {
                return (null, null);
            }

            else


            {
                var sq = (R * R - r2 * r2 + dSq) / (2 * d);
                var h = MathF.Sqrt(R * R - sq * sq);
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
        }
    }
}