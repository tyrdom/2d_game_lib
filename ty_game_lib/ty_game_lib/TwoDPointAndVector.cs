using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;

namespace ty_game_lib
{
    public class TwoDPoint
    {
        public TwoDPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public readonly float X;
        public readonly float Y;

        public (TwoDPoint, TwoDPoint) SwapPoint(TwoDPoint b)
        {
            return (b, this);
        }

        public Pt2LinePos GetposOnLine(TwoDVectorLine aline)
        {
            var xLine = new TwoDVectorLine(aline.A, this);

            var cross = xLine.GetVector().Cross(aline.GetVector());
            return cross switch
            {
                float cro when cro > 0 => Pt2LinePos.Right,
                float cro when cro < 0 => Pt2LinePos.Left,
                _ => Pt2LinePos.On
            };
        }
    }


    public class TwoDVector
    {
        private float X { get; }

        private float Y { get; }

        public TwoDVector(float x, float y)
        {
            X = x;
            Y = y;
        }


        public float Cross(TwoDVector b)
        {
            return X * b.Y - b.X * Y;
        }
    }

    public enum Pt2LinePos
    {
        Right,
        On,
        Left
    }
}