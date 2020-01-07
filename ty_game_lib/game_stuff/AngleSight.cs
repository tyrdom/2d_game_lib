using System;
using collision_and_rigid;

namespace game_stuff
{
    public class AngleSight
    {
        private TwoDVector Aim;
        private float NowR;
        private TwoDVector _upLeft;
        private TwoDVector _upRight;
        private float MaxR;
        private float Theta;

        public AngleSight(float R, TwoDVector upLeft)
        {
            Aim = new TwoDVector(1f, 0f);
            NowR = R;
            var vector = upLeft.GetUnit();
            _upLeft = vector;
            _upRight = new TwoDVector(vector.X, -vector.Y);
            MaxR = R;
            var cosA = vector.X;
            var cos2A = 2 * cosA * cosA - 1;
            Theta = MathF.Acos(cos2A);
        }

        bool InSight(TwoDVectorLine sLine)
        {
            var twoDVector = sLine.GetVector().ClockwiseTurn(Aim);
            var c1 = twoDVector.Cross(_upLeft);
            var c2 = twoDVector.Cross(_upRight);
            return c1 >= 0 && c2 <= 0;
        }

        void OpChangeAim(TwoDVector newAim, float twoSToSeePerTick)
        {
            var oldAim = Aim;
            Aim = newAim;

            var cosT = newAim.GetUnit().Dot(oldAim) / oldAim.Norm();
            var cosA = _upLeft.X;
            var cos2A = 2 * cosA * cosA - 1;
            var t = cosT > cos2A ? MathF.Acos(cosT) : Theta;

            var nowRSquare = NowR * NowR;
            var snowR = nowRSquare * t;
            var twoSr = twoSToSeePerTick - snowR;
            if (twoSr <= 0)
            {
                var sqrt = MathF.Sqrt(twoSToSeePerTick / t);
                NowR = MathF.Min(MaxR, sqrt);
            }
            else
            {
                var rSquare = twoSr / Theta + nowRSquare;
                var sqrt = MathF.Sqrt(rSquare);
                NowR = MathF.Min(MaxR, sqrt);
            }
        }
    }
}