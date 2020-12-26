using collision_and_rigid;

namespace game_stuff
{
    public class Scope
    {
        public static Scope StandardScope()
        {
            return new Scope(LocalConfig.StandardSightVector);
        }


        public Scope(TwoDVector faceRightLeftPt, TwoDVector faceRightRightPt, float maxR, float theta)
        {
            FaceRightLeftPt = faceRightLeftPt;
            FaceRightRightPt = faceRightRightPt;
            MaxR = maxR;
            Theta = theta;
        }

        public TwoDVector FaceRightLeftPt { get; }
        public TwoDVector FaceRightRightPt { get; }
        public float MaxR { get; }
        public float Theta { get; }


        public Scope(TwoDVector upLeft)
        {
            var maxR = upLeft.Norm();

            var vector = upLeft.GetUnit();
            var cosA = vector.X;
            var cos2A = 2 * cosA * cosA - 1;
            FaceRightLeftPt = vector;
            FaceRightRightPt = new TwoDVector(vector.X, -vector.Y);
            Theta = MathTools.Acos(cos2A);
            MaxR = maxR;
        }

        public Scope GenNewScope(float multi)
        {
            var theta = Theta / multi;
            var maxR = MaxR * multi;
            var f = theta / 2;
            var cos = MathTools.Cos(f);
            var sin = MathTools.Sin(f);
            var twoDVector = new TwoDVector(cos, sin);
            var twoDVector2 = new TwoDVector(cos, -sin);
            return new Scope(twoDVector, twoDVector2, maxR, theta);
        }
    }
}