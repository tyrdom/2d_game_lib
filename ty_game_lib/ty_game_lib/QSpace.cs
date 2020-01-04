#nullable enable
using System;
using System.Collections.Generic;


namespace ty_game_lib
{
    public abstract class QSpace
    {
        public abstract TwoDPoint? GetSlidePoint(AabbBoxShape lineInBoxShape);
        public virtual Quad? TheQuad { get; set; }
        public abstract void InsertBox(AabbBoxShape boxShape);
        public abstract Zone Zone { get; set; }
        public abstract List<AabbBoxShape> AabbPackBoxShapes { get; set; }
        public abstract void Remove(AabbBoxShape boxShape);
        public abstract IEnumerable<AabbBoxShape> TouchBy(AabbBoxShape boxShape);
        public abstract QSpace TryCovToLimitQSpace(int limit);
        public abstract (int, AabbBoxShape?) TouchWithARightShootPoint(TwoDPoint p);
        public abstract string OutZones();
    }


    public struct Zone
    {
        public readonly float Up;
        public readonly float Down;
        public readonly float Left;
        public readonly float Right;

        public Zone(float up, float down, float left, float right)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }

        public TwoDPoint RU()
        {
            return new TwoDPoint(Right, Up);
        }

        public TwoDPoint RD()
        {
            return new TwoDPoint(Right, Down);
        }

        public TwoDPoint LU()
        {
            return new TwoDPoint(Left, Up);
        }

        public TwoDPoint LD()
        {
            return new TwoDPoint(Left, Down);
        }

        public (float, float) GetMid()
        {
            var horizon = (Up + Down) / 2;
            var vertical = (Right + Left) / 2;
            return (horizon, vertical);
        }

        public Zone[] CutTo4(float horizon, float vertical)
        {
            var z1 = new Zone(Up, horizon, vertical, Right);
            var z2 = new Zone(Up, horizon, Left, vertical);
            var z3 = new Zone(horizon, Down, Left, vertical);
            var z4 = new Zone(horizon, Down, vertical, Right);
            return new Zone[4] {z1, z2, z3, z4};
        }

        bool IsIn(Zone anotherZone)
        {
            return anotherZone.Left <= Left && anotherZone.Right >= Right && anotherZone.Up >= Up &&
                   anotherZone.Down <= Down;
        }

        public bool NotCross(Zone anotherZone)
        {
            return Right < anotherZone.Left || anotherZone.Right < Left ||
                   Up < anotherZone.Down || anotherZone.Up < Down;
        }

        public Zone Inter(Zone another)
        {
            var nUp = MathF.Min(Up, another.Up);
            var nLeft = MathF.Max(Left, another.Left);
            var nDown = MathF.Max(Down, another.Down);
            var nRight = MathF.Min(Right, another.Right);
            return new Zone(nUp, nDown, nLeft, nRight);
        }

        public Zone Join(Zone another)
        {
            var nUp = MathF.Max(Up, another.Up);
            var nLeft = MathF.Min(Left, another.Left);
            var nDown = MathF.Min(Down, another.Down);
            var nRight = MathF.Max(Right, another.Right);
            return new Zone(nUp, nDown, nLeft, nRight);
        }

        public bool IncludePt(TwoDPoint pt)
        {
            var x = pt.X;
            var y = pt.Y;
            return Up >= y && y >= Down && x >= Left && Right >= x;
        }
    }


    public enum Quad
    {
        One,
        Two,
        Three,
        Four
    }
}