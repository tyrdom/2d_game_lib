using System;
using System.Collections.Generic;
using System.Linq;
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

        public TwoDVector(TwoDPoint a, TwoDPoint b)
        {
            X = b.X - a.X;
            Y = b.Y - a.Y;
        }

        public string LogVector()
        {
            return $"[{X}||{Y}]";
        }


        public TwoDVector MaxFixX(float a)
        {
            X = MathTools.Max(a, X);
            return this;
        }

        public Pt2LinePos GetPosOfAnother(TwoDVector another)
        {
            return ToPt().GetPosOf(new TwoDVectorLine(TwoDPoint.Zero(), another.ToPt()));
        }

        public float GetSin(TwoDVector end)
        {
            return Cross(end) / Norm() / end.Norm();
        }

        public float GetCos(TwoDVector end)
        {
            return Dot(end) / Norm() / end.Norm();
        }

        public float X { get; private set; }

        public float Y { get; private set; }

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
            return f <= 0 ? new TwoDVector(0f, 0f) : new TwoDVector(X / f, Y / f);
        }

        public TwoDVector? GetUnit2()
        {
            var f = Norm();
            return f <= 0 ? null : new TwoDVector(X / f, Y / f);
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

        public TwoDVector AddY(float y)
        {
            Y += y;

            return this;
        }

        public void Add(TwoDVector twoDVector)
        {
            X += twoDVector.X;
            Y += twoDVector.Y;
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

        public override string ToString()
        {
            return LogVector();
        }

        public static IEnumerable<TwoDVector> GenLinearListToAnother(TwoDVector v1, int k1)
        {
            return Enumerable.Range(1, k1).Select(x => v1.Multi((float)x / k1));
        }

        public TwoDVector Sum(TwoDVector twoDVector)
        {
            var dVector = new TwoDVector(X + twoDVector.X, Y + twoDVector.Y);
            return dVector;
        }
    }

    public enum Pt2LinePos
    {
        Right,
        On,
        Left
    }
}