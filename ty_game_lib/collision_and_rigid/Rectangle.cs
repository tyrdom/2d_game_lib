namespace collision_and_rigid
{
    public class Rectangle : IRawBulletShape
    {
        private TwoDPoint LeftDown;
        private TwoDPoint LeftUp;
        private float Height;

        public Rectangle(TwoDPoint leftDown, TwoDPoint leftUp, float height)
        {
            LeftDown = leftDown;
            LeftUp = leftUp;
            Height = height;
        }

        Poly CovPoly(){
            var twoDVectorLine = new TwoDVectorLine(LeftDown,LeftUp);
            var twoDVector = twoDVectorLine.GetVector().ClockwiseTurn(new TwoDVector(0, 1)).GetUnit().Multi(Height);
            var rd = LeftDown.Move(twoDVector);
            var ru = LeftUp.Move(twoDVector);
            var twoDPoints = new []{LeftDown,LeftUp,ru,rd};
            var poly = new Poly(twoDPoints);
            return poly;
        }

        public Zone GenBulletZone(float r)
        {
            var moreHigh = CovPoly().GenZone().MoreWide(r).MoreHigh(r);
            return moreHigh;
        }

        public IBulletShape GenBulletShape(float r)
        {
            var genBlockShapes = CovPoly().GenBlockShapes(r,true);
            var genBlockAabbBoxShapes = Poly.GenBlockAabbBoxShapes(genBlockShapes);
            return new SimpleBlocks(genBlockAabbBoxShapes);
        }
    }
}