﻿using System;
using System.Collections.Generic;

namespace collision_and_rigid
{
    public class TwoDPoint
    {
        public readonly float X;
        public readonly float Y;

        public TwoDPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public TwoDPoint ClockTurnAboutZero(TwoDVector v)
        {
            var vX = X * v.X + Y * v.Y;
            var vY = -X * v.Y + Y * v.X;
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
            var (h, v) = zone.GetMid();
            var b = X < v;
            if (b)
                return Y > h ? Quad.Two : Quad.Three;
            return Y > h ? Quad.One : Quad.Four;
        }

        public Quad WhichQ(Zone zone)
        {
            var (h, v) = zone.GetMid();
            var b = X < v;
            if (b)
                return Y > h ? Quad.Two : Quad.Three;
            return Y > h ? Quad.One : Quad.Four;
        }

        public int FastGenARightShootCrossALotAabbBoxShape(List<AabbBoxShape> aabbBoxShapes)
        {
            var n = 0;
            aabbBoxShapes.ForEach(aabbBoxShape =>
            {
                var objZone = aabbBoxShape.Zone;
                if (objZone.Up >= Y && objZone.Down < Y)
                    //                    Console.Out.WriteLine("TOUCH::"+SomeTools.ZoneLog(objZone));
                    n++;
            });
            return n;
        }

        public Zone GetZone()
        {
            return new Zone(Y, Y, X, X);
        }

      

        public TwoDPoint GenPosInLocal(TwoDPoint zero, TwoDVector xAim)
        {
            return TwoDVector.TwoDVectorByPt(zero, this).AntiClockwiseTurn(xAim).ToPt();
        }

        public (int, AabbBoxShape?) GenARightShootCrossALotAabbBoxShape(List<AabbBoxShape> aabbBoxShapes)
        {
            var n = 0;
            AabbBoxShape? aShape = null;
//            Console.Out.WriteLine("Count::" + aabbBoxShapes.Count);
            foreach (var aabbBoxShape in aabbBoxShapes)
            {
                var zone = aabbBoxShape.Zone;
//                Console.Out.WriteLine("XY:::" + X + "  " + Y + "\n zone：：" + SomeTools.ZoneLog(zone));
                if (Y <= zone.Up && Y > zone.Down
                )
                {
//                    Console.Out.WriteLine("@@@" + X + "?<?" + zone.Left);

                    if (X < zone.Left)
                    {
//                        Console.Out.WriteLine("!TOUCH::" + SomeTools.ZoneLog(zone));
                        n++;
                    }
                    else if (X >= zone.Left && X < zone.Right)
                    {
                        aShape = aabbBoxShape;
                        var touchByRightShootPointInAAbbBox = aabbBoxShape.Shape.TouchByRightShootPointInAAbbBox(this);
//                        Console.Out.WriteLine("a num:" + touchByRightShootPointInAAbbBox + "zone: " +
//                                              SomeTools.ZoneLog(aabbBoxShape.Zone));
//
//                        Console.Out.WriteLine(SomeTools.ZoneLog(zone));
                        if (touchByRightShootPointInAAbbBox < 0) return (touchByRightShootPointInAAbbBox, aShape);
                        n += touchByRightShootPointInAAbbBox;
                    }
                }
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
            return walkBlock.CoverPoint(this);
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

        public TwoDVector GenVector(TwoDPoint b)
        {
            return new TwoDVector(b.X - X, b.Y - Y);
        }
    }
}