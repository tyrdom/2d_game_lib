using System;

namespace collision_and_rigid
{
    public class Round : IShape, IRawBulletShape, IBulletShape
    {
        public TwoDPoint O;
        public float R;

        public Round(TwoDPoint o, float r)
        {
            O = o;
            R = r;
        }

        public AabbBoxShape CovToAabbPackBox()
        {
            var zone = GetZones();
            return new AabbBoxShape(zone, this);
        }


        public (Zone? leftZone, Zone? rightZone) CutByV(float v, Zone z)
        {
            var (down, up) = GetY(v);
            if (down == null || up == null)
            {
                return (null, null);
            }

            if (v < O.X)
            {
                var leftZone = new Zone(up.Value, down.Value, z.Left, v);
                var rightZone = new Zone(z.Up, z.Down, v, z.Right);
                return (leftZone, rightZone);
            }

            if (v > O.X)
            {
                var leftZone2 = new Zone(z.Up, z.Down, z.Left, v);
                var rightZone2 = new Zone(up.Value, down.Value, v, z.Right);
                return (leftZone2, rightZone2);
            }

            var leftZone3 = new Zone(z.Up, z.Down, z.Left, v);
            var rightZone3 = new Zone(z.Up, z.Down, v, z.Right);
            return (leftZone3, rightZone3);
        }

        public (Zone?, Zone?) CutByH(float h, Zone z)
        {
            //TODO
            throw new NotImplementedException();
        }

        public int TouchByRightShootPointInAAbbBoxInQSpace(TwoDPoint p)
        {
            var sqNorm = TwoDVector.TwoDVectorByPt(O, p).SqNorm();
            if (sqNorm < R * R) return -1;
            {
                return p.X < O.X ? 1 : 0;
            }
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
            return $"O:: {O.Log()} R::{R}";
        }
    }
}