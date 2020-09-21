using System;
using collision_and_rigid;

namespace game_stuff
{
    public class Scope
    {
        public static Scope StandardScope()
        {
            return new Scope(TempConfig.StandardSightVector);
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

        private Scope(TwoDVector upLeft)
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

    public class AngleSight
    {
        private Scope StandardScope { get; }

        public float NowR { get; private set; }
        public TwoDVector Aim { get; private set; }

        public static AngleSight StandardAngleSight()
        {
            return new AngleSight(TempConfig.StandardSightVector);
        }

        public AngleSight(TwoDVector upLeft)
        {
            var nowR = upLeft.Norm();

            var vector = upLeft.GetUnit();
            var cosA = vector.X;
            var cos2A = 2 * cosA * cosA - 1;

            var faceRightRightPt = new TwoDVector(vector.X, -vector.Y);
            var theta = MathTools.Acos(cos2A);
            StandardScope = new Scope(vector, faceRightRightPt, nowR, theta);

            Aim = new TwoDVector(1f, 0f);

            NowR = nowR;
        }

        //todo UseNotDefaultScope


        public bool InSight(TwoDVectorLine sLine, SightMap map)
        {
            var twoDVector = sLine.GetVector().ClockwiseTurn(Aim);
            var c1 = twoDVector.Cross(StandardScope.FaceRightLeftPt);
            var c2 = twoDVector.Cross(StandardScope.FaceRightRightPt);
            var b = twoDVector.SqNorm() <= NowR * NowR;

            var isBlockSightLine = map.IsBlockSightLine(sLine);


            var blockSightLine = c1 >= 0 && c2 <= 0 && b && !isBlockSightLine;

#if DEBUG
            Console.Out.WriteLine($" _nowR ::: {NowR}");

#endif
            return blockSightLine;
        }


        public void OpChangeAim(TwoDVector? newAim, Scope scope)
        {
        }

        public void OpChangeAim(TwoDVector? newAim)
        {
            var oldAim = Aim;

            if (newAim != null) Aim = newAim.GetUnit();

            var cosT = Aim.GetUnit().Dot(oldAim) / oldAim.Norm();
            var cosA = StandardScope.FaceRightLeftPt.X;
            var cos2A = 2 * cosA * cosA - 1;
            var t = cosT > cos2A ? MathTools.Acos(cosT) : StandardScope.Theta;

            var nowRSquare = NowR * NowR;
            var snowR = nowRSquare * t;
            var twoSr = TempConfig.TwoSToSeePerTick - snowR;
            if (twoSr <= 0)
            {
                var sqrt = MathTools.Sqrt(TempConfig.TwoSToSeePerTick / t);
                NowR = MathTools.Min(StandardScope.MaxR, sqrt);
            }
            else
            {
                var rSquare = twoSr / StandardScope.Theta + nowRSquare;
                var sqrt = MathTools.Sqrt(rSquare);
                NowR = MathTools.Min(StandardScope.MaxR, sqrt);
            }
        }
    }
}