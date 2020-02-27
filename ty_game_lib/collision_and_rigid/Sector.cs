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
            var twoDPoint = AOB.O;
            var round = new Round(twoDPoint,R);
            var genBulletRawZone = round.GenBulletZone(r);
            return genBulletRawZone;
        }

        public IBulletShape GenBulletShape(float r)
        {
            var genBlockShapes = GenBlockShapes(r);
            var genBlockAabbBoxShapes = Poly.GenBlockAabbBoxShapes(genBlockShapes);
            var simpleBlocks = new SimpleBlocks(genBlockAabbBoxShapes);
            return simpleBlocks;
        }
    }
}