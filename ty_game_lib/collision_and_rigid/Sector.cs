﻿using System;
using System.Collections.Generic;


namespace collision_and_rigid
{
    public class Sector : IRawBulletShape
    {
        private readonly ClockwiseBalanceAngle AOB;
        private readonly float R;

        public Sector(ClockwiseBalanceAngle aob, float r)
        {
            AOB = aob;
            R = r;
        }

        public Sector(float x, float y, TwoDVector move, TwoDVector rotate)
        {
            var o = TwoDPoint.Zero().Move(move);
            var a = new TwoDPoint(x, y).ClockTurnAboutZero(rotate).Move(move);
            var b = new TwoDPoint(x, -y).ClockTurnAboutZero(rotate).Move(move);
            var clockwiseBalanceAngle = new ClockwiseBalanceAngle(a, o, b);
            AOB = clockwiseBalanceAngle;
            R = MathTools.Sqrt(x * x + y * y);
        }

        private List<IBlockShape> GenBlockShapes(float r)

        {
            var a = AOB.A;
            var b = AOB.B;
            var o = AOB.O;

            var oa = new TwoDVectorLine(o, a);

            var oav = oa.GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);
            var oaOut = oa.MoveVector(oav);
            var na = a.Move(oa.GetVector().GetUnit().Multi(r));


            var bo = new TwoDVectorLine(b, o);

            var bov = bo.GetVector().GetUnit().CounterClockwiseHalfPi().Multi(r);
            var boOut = bo.MoveVector(bov);
            var nb = b.Move(bo.GetVector().GetUnit().Multi(-r));

            var naONb = new ClockwiseBalanceAngle(na, o, nb);
            var aobOut = new ClockwiseTurning(naONb, r + R, null, null);
            var p1 = new ClockwiseBalanceAngle(oaOut.B, o, na);
            var piece1 = new ClockwiseTurning(p1, r, null, null);
            var p2 = new ClockwiseBalanceAngle(nb, o, boOut.A);
            var piece2 = new ClockwiseTurning(p2, r, null, null);

            var p3 = new ClockwiseBalanceAngle(boOut.B, o, oaOut.A);
            var piece3 = new ClockwiseTurning(p3, r, null, null);

            return new List<IBlockShape> {oaOut, piece1, aobOut, piece2, boOut, piece3};
        }

        public Zone GenBulletZone(float r)
        {
            var a = AOB.A;
            var b = AOB.B;
            var o = AOB.O;

            var oa = new TwoDVectorLine(o, a);
            var ob = new TwoDVectorLine(o, b);
            var zone1 = AOB.GetZone(r);

            var zone2 = oa.GenZone().MoreWide(r).MoreHigh(r);
            var zone3 = ob.GenZone().MoreWide(r).MoreHigh(r);
            var zoneJ = zone1.Join(zone2).Join(zone3);
            return zoneJ;
        }

        public IBulletShape GenBulletShape(float r)
        {
            var genBlockShapes = GenBlockShapes(r);

            var simpleBlocks = new SimpleBlocks(genBlockShapes);
            return simpleBlocks;
        }
    }
}