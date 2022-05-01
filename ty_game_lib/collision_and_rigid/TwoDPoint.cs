using System;
using System.Collections.Generic;

namespace collision_and_rigid
{
    [Serializable]
    public class TwoDPoint : ITwoDTwoP
    {
        public readonly float X;
        public readonly float Y;

        public TwoDPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public string LogPt()
        {
            return $"[{X}|{Y}]";
        }

        public TwoDPoint ClockTurnAboutZero(TwoDVector v)
        {
            var vX = X * v.X + Y * v.Y;
            var vY = -X * v.Y + Y * v.X;
            return new TwoDPoint(vX, vY);
        }

        public TwoDPoint AntiWiseClockTurnAboutZero(TwoDVector v)
        {
            var vX = X * v.X - Y * v.Y;
            var vY = X * v.Y + Y * v.X;
            return new TwoDPoint(vX, vY);
        }

        public Zone GenZone(TwoDPoint b)
        {
            var zone = new Zone(Math.Max(Y, b.Y), Math.Min(Y, b.Y), Math.Min(X, b.X), Math.Max(X, b.X));
            return zone;
        }

        public bool Same(TwoDPoint an)
        {
            if (X > an.X || X < an.X) return false;

            if (Y > an.Y || Y < an.Y) return false;

            return true;
        }

        public Quad WhichQ(QSpaceBranch qSpaceBranch)
        {
            var zone = qSpaceBranch.Zone;
            return WhichQ(zone);
        }

        public Quad WhichQ(Zone zone)
        {
            var (h, v) = zone.GetMid();
            var b = X < v;
            if (b)
                return Y > h ? Quad.Two : Quad.Three;
            return Y > h ? Quad.One : Quad.Four;
        }

        public int FastGenARightShootCrossALotAabbBoxShape(IEnumerable<IAaBbBox> aabbBoxShapes)
        {
            var n = 0;
            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var objZone = aabbBoxShape.Zone;
                if (objZone.Up >= Y && objZone.Down < Y)
                    //                    Console.Out.WriteLine("TOUCH::"+SomeTools.ZoneLog(objZone));
                    n++;
            }

            return n;
        }

        public float GetLineDistance(TwoDVectorLine line)
        {
            if (line.A.Same(line.B))
            {
                return GetDistance(line.A);
            }

            var multiFromA = line.GetMultiFromA(this);
            if (multiFromA <= 0)
            {
                return GetDistance(line.A);
            }

            if (multiFromA >= 1)
            {
                return GetDistance(line.B);
            }

            var dPoint = line.A.Move(line.GetVector().GetUnit().Multi(multiFromA));
            return GetDistance(dPoint);
        }

        public float GetDistance(TwoDPoint another)
        {
            var dX = X - another.X;
            var dY = Y - another.Y;
            var x = dX * dX + dY * dY;
            var sqrt = MathTools.Sqrt(x);
            return sqrt;
        }

        public float GetSqDistance(TwoDPoint another)
        {
            var dX = X - another.X;
            var dY = Y - another.Y;
            var x = dX * dX + dY * dY;
            return x;
        }

        public Pt2LinePos CanCutToCov(TwoDVectorLine line1, TwoDVectorLine line2)
        {
            var pos1 = GetPosOf(line1);
            var pos2 = GetPosOf(line2);

            if (pos1 != Pt2LinePos.Right)
            {
                return pos2 != Pt2LinePos.Right ? Pt2LinePos.On : Pt2LinePos.Left;
            }

            if (pos2 != Pt2LinePos.Right) return Pt2LinePos.Right;
            throw new Exception("wrong cut pt");
        }


        public Zone GetZone()
        {
            return new Zone(Y, Y, X, X);
        }


        public TwoDPoint GenPosInLocal(TwoDPoint zero, TwoDVector xAim)
        {
            return TwoDVector.TwoDVectorByPt(zero, this).ClockwiseTurn(xAim).ToPt();
        }

        public (int, BlockBox?) GenARightShootCrossALotAabbBoxShapeInQSpace(
            IEnumerable<BlockBox> aabbBoxShapes)
        {
            var n = 0;
            BlockBox? aShape = null;
//            Console.Out.WriteLine("Count::" + aabbBoxShapes.Count);
            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var zone = aabbBoxShape.Zone;
//                Console.Out.WriteLine("XY:::" + X + "  " + Y + "\n zone：：" + SomeTools.ZoneLog(zone));
                if (zone.Up <= zone.Down && zone.IncludePt(this))
                {
                    //-3 为在线上
                    return (-3, aabbBoxShape);
                }

                if (!(Y <= zone.Up) || !(Y > zone.Down)) continue;
                if (X < zone.Left)
                {
//                        Console.Out.WriteLine("!TOUCH::" + SomeTools.ZoneLog(zone));
                    n++;
                }
                else if (X >= zone.Left && X <= zone.Right)
                {
                    aShape = aabbBoxShape;
                    var shape = aabbBoxShape.Shape;
                    var touchByRightShootPointInAAbbBox = shape.TouchByRightShootPointInAAbbBoxInQSpace(
                        this);

//                        Console.Out.WriteLine("a num:" + touchByRightShootPointInAAbbBox + "zone: " +
//                                              SomeTools.ZoneLog(aabbBoxShape.Zone));
//
//                        Console.Out.WriteLine(SomeTools.ZoneLog(zone));
                    if (touchByRightShootPointInAAbbBox < 0) return (touchByRightShootPointInAAbbBox, aShape);
                    n += touchByRightShootPointInAAbbBox;
                }

                //                    Console.Out.WriteLine("@@@" + X + "?<?" + zone.Left);
            }

            return (n, aShape);
        }

        public (TwoDPoint, TwoDPoint) SwapPoint(TwoDPoint b)
        {
            return (b, this);
        }

        public float Get2S(TwoDVectorLine aline)
        {
            var xLine = new TwoDVectorLine(aline.A, this);

            return xLine.GetVector().Cross(aline.GetVector());
        }

        public Pt2LinePos GetPosOf(TwoDVectorLine aline)
        {
            var cross = Get2S(aline);
            return cross switch
            {
                { } cro when cro > 0 => Pt2LinePos.Right,
                { } cro when cro < 0 => Pt2LinePos.Left,
                _ => Pt2LinePos.On
            };
        }

        public bool IsInBlock(WalkBlock walkBlock)
        {
            return walkBlock.RealCoverPoint(this);
        }

        public TwoDPoint Move(TwoDVector v)
        {
            return new TwoDPoint(X + v.X, Y + v.Y);
        }

        public bool InRound(Round belongRd)
        {
            var oX = belongRd.O.X - X;
            var oY = belongRd.O.Y - Y;
            var x = oX * oX + oY * oY;
            var belongRdR = belongRd.R;
            var b = x < belongRdR * belongRdR;
            return b;
        }

        public TwoDVector Vector()
        {
            return new TwoDVector( X,  Y);
        }
        public TwoDVector GenVector(TwoDPoint b)
        {
            return new TwoDVector(b.X - X, b.Y - Y);
        }

        public static TwoDPoint Zero()
        {
            return new TwoDPoint(0f, 0f);
        }

        public override string ToString()
        {
            return LogPt();
        }

        public int WhichQ(float horizon, float vertical)
        {
            if (X > vertical)
            {
                if (Y > horizon)
                {
                    return 1;
                }

                if (Y < horizon)
                {
                    return 4;
                }
            }

            if (X < vertical)
            {
                if (Y > horizon)
                {
                    return 2;
                }

                if (Y < horizon)
                {
                    return 3;
                }
            }

            return 0;
        }

        public TwoDPoint GetMid(TwoDPoint pos)
        {
            return new TwoDPoint((pos.X + X) / 2,
                (pos.Y + Y) / 2);
        }

        public TwoDPoint Clone()
        {
            return new TwoDPoint(X, Y);
        }
    }
}