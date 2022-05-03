using System;

namespace collision_and_rigid
{
    public class Rectangle : IRawBulletShape
    {
        private TwoDPoint LeftDown { get; }
        private TwoDPoint LeftUp { get; }
        private float Height { get; }

        public Rectangle(TwoDPoint leftDown, TwoDPoint leftUp, float height)
        {
            LeftDown = leftDown;
            LeftUp = leftUp;
            Height = height;
        }

        private Poly CovPoly()
        {
            var twoDVectorLine = new TwoDVectorLine(LeftDown, LeftUp);
            var twoDVector = twoDVectorLine.GetVector().ClockwiseTurn(new TwoDVector(0, 1)).GetUnit().Multi(Height);
            var rd = LeftDown.Move(twoDVector);
            var ru = LeftUp.Move(twoDVector);
            var twoDPoints = new[] { LeftDown, LeftUp, ru, rd };
            var poly = new Poly(twoDPoints);
            // Console.Out.WriteLine($"Blade Wave Rectangle:{poly}");
            return poly;
        }

        public Zone GenBulletZone(float r)
        {
            var moreHigh = CovPoly().GenZone().MoreWide(r).MoreHigh(r);
            return moreHigh;
        }

        public IBulletShape GenBulletShape(float r)
        {
            var genBlockShapes = CovPoly().GenBlockShapes(r, true);
            return new SimpleBlocks(genBlockShapes);
        }
    }
}