using System;
using System.Numerics;

namespace ty_game_lib
{
    public class TwoDVector
    {
        public float X { get; }

        public float Y { get; }

        public TwoDVector(float a, float b)
        {
            X = a;
            Y = b;
        }

        public static TwoDVector TwoDVectorByPt(TwoDPoint a, TwoDPoint b)
        {
            var x = b.X - a.X;
            var y = b.Y - a.Y;
            return new TwoDVector(x, y);
        }


        public float Cross(TwoDVector b)
        {
            return X * b.Y - b.X * Y;
        }

        public TwoDVector GetUnit()
        {
            var f = MathF.Sqrt(X * X + Y * Y);
            return new TwoDVector(X / f, Y / f);
        }

        public TwoDVector DicHalfPi()
        {
            return new TwoDVector(Y, -X);
        }

        public TwoDVector Multi(float m)
        {
            return new TwoDVector(X * m, Y * m);
        }

        public float Dot(TwoDVector v)
        {
            return v.X * X + v.Y * Y;
        }

        public float Norm()
        {
            return MathF.Sqrt(X * X + Y * Y);
        }
        
        

        public Quad WhichQ()
        {
            if (X > 0)
            {
                return Y > 0 ? Quad.One : Quad.Four;
            }

            return Y > 0 ? Quad.Two : Quad.Three;
        }
    }

    public enum Pt2LinePos
    {
        Right,
        On,
        Left
    }
}