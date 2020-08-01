using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace collision_and_rigid
{
    public enum Attitude
    {
        RightUp,
        LeftUp,
        Horizon,
        Vertical
    }

    public class TwoDVector : ITwoDTwoP
    {
        public TwoDVector(float a, float b)
        {
            X = a;
            Y = b;
        }

        public string LogVector()
        {
            return $"[{X}||{Y}]";
        }

        public float X { get; set; }

        public float Y { get; set; }

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

        public static TwoDVector Zero()
        {
            return new TwoDVector(0f, 0f);
        }

        public TwoDVector GetUnit()
        {
            var f = Norm();
            return new TwoDVector(X / f, Y / f);
        }

        public TwoDVector Minus(TwoDVector another)
        {
            return new TwoDVector(X - another.X, Y - another.Y);
        }


        public TwoDVector AddX(float x)
        {
            X += x;

            return this;
        }

        public TwoDVector CounterClockwiseHalfPi()
        {
            return new TwoDVector(-Y, X);
        }

        public TwoDVector Multi(float m)
        {
            return new TwoDVector(X * m, Y * m);
        }

        public float Dot(TwoDVector v)
        {
            return v.X * X + v.Y * Y;
        }

        public TwoDVector ClockwiseTurn(TwoDVector v)
        {
            var vX = X * v.X + Y * v.Y;
            var vY = -X * v.Y + Y * v.X;
            return new TwoDVector(vX, vY);
        }

        public TwoDVector AntiClockwiseTurn(TwoDVector v)
        {
            var vX = X * v.X - Y * v.Y;
            var vY = X * v.Y + Y * v.X;
            return new TwoDVector(vX, vY);
        }

        public TwoDVector Normalize()
        {
            X /= Norm();
            Y /= Norm();
            return this;
        }

        public float Norm()
        {
            return MathTools.Sqrt(SqNorm());
        }

        public float SqNorm()
        {
            return X * X + Y * Y;
        }

        public bool IsAlmostRightUp()
        {
            var b = (X >= 0) ^ (Y < 0);
            return b;
        }

        public Attitude GetAttitude()
        {
            if (X > 0)
            {
                if (Y > 0) return Attitude.RightUp;

                return Y < 0 ? Attitude.LeftUp : Attitude.Vertical;
            }

            if (!(X < 0)) return Attitude.Horizon;
            if (Y < 0) return Attitude.RightUp;

            return Y > 0 ? Attitude.LeftUp : Attitude.Vertical;
        }

        public Quad WhichQ()
        {
            if (X > 0) return Y > 0 ? Quad.One : Quad.Four;

            return Y > 0 ? Quad.Two : Quad.Three;
        }

        public TwoDPoint ToPt()
        {
            var twoDPoint = new TwoDPoint(X, Y);
            return twoDPoint;
        }

        public string Log()
        {
            return LogVector();
        }
    }

    public enum Pt2LinePos
    {
        Right,
        On,
        Left
    }
}