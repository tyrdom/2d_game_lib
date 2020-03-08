using System;
using System.ComponentModel.Design;
using collision_and_rigid;

namespace game_stuff
{
    public class AngleSight
    {
        private readonly TwoDVector _upLeft;
        private readonly TwoDVector _upRight;
        public TwoDVector Aim;
        private readonly float MaxR;
        private float NowR;
        private readonly float Theta;

        public static AngleSight StandardAngleSight()
        {
            return new AngleSight(TempConfig.StandardSightR,TempConfig.StandardVector);
        }
        public AngleSight(float r, TwoDVector upLeft)
        {
            Aim = new TwoDVector(1f, 0f);
            NowR = r;
            var vector = upLeft.GetUnit();
            _upLeft = vector;
            _upRight = new TwoDVector(vector.X, -vector.Y);
            MaxR = r;
            var cosA = vector.X;
            var cos2A = 2 * cosA * cosA - 1;
            Theta = MathF.Acos(cos2A);
        }

        public bool InSight(TwoDVectorLine sLine, SightMap map)
        {
            var twoDVector = sLine.GetVector().ClockwiseTurn(Aim);
            var c1 = twoDVector.Cross(_upLeft);
            var c2 = twoDVector.Cross(_upRight);
            var b = twoDVector.SqNorm() <= NowR * NowR;
            var isBlockSightLine = map.IsBlockSightLine(sLine);
            return c1 >= 0 && c2 <= 0 && b && isBlockSightLine;
        }

        public void OpChangeAim(TwoDVector newAim)
        {
            var oldAim = Aim;
            Aim = newAim.GetUnit();

            var cosT = newAim.GetUnit().Dot(oldAim) / oldAim.Norm();
            var cosA = _upLeft.X;
            var cos2A = 2 * cosA * cosA - 1;
            var t = cosT > cos2A ? MathF.Acos(cosT) : Theta;

            var nowRSquare = NowR * NowR;
            var snowR = nowRSquare * t;
            var twoSr = TempConfig.TwoSToSeePerTick - snowR;
            if (twoSr <= 0)
            {
                var sqrt = MathF.Sqrt(TempConfig.TwoSToSeePerTick / t);
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