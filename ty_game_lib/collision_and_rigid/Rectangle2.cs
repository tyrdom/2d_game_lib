using System.Linq;

namespace collision_and_rigid
{
    public class Rectangle2 : IRawBulletShape


    {
        private float Height { get; }
        private float Width { get; }
        private TwoDVector MidPos { get; }
        private TwoDVector MidRotate { get; }

        public Rectangle2(float width, float height, TwoDVector midPos, TwoDVector midRotate)
        {
            Width = width;
            Height = height;
            MidPos = midPos;
            MidRotate = midRotate;
        }

        private Poly GenPoly()
        {
            var p1 = new TwoDPoint(-Width / 2f, -Height / 2f);
            var p2 = new TwoDPoint(-Width / 2f, Height / 2f);
            var p3 = new TwoDPoint(Width / 2f, Height / 2f);
            var p4 = new TwoDPoint(Width / 2f, -Height / 2f);
            var twoDPoints = new[] { p1, p2, p3, p4 };
            var dPoints = twoDPoints.Select(x => x.AntiWiseClockTurnAboutZero(MidRotate).Move(MidPos));
            return new Poly(dPoints.ToArray());
        }

        public Zone GenBulletZone(float r)
        {
            var poly = GenPoly();
            var moreHigh = poly.GenZone().MoreWide(r).MoreHigh(r);
            return moreHigh;
        }

        public IBulletShape GenBulletShape(float r)
        {
            var genPoly = GenPoly();
            var genBlockShapes = genPoly.GenBlockShapes(r, true);
            // var genBlockAabbBoxShapes = Poly.GenBlockAabbBoxShapes(genBlockShapes);
            return new SimpleBlocks(genBlockShapes);
        }
    }
}