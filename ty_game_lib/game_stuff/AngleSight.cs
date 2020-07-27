
using collision_and_rigid;

namespace game_stuff
{
    public class AngleSight
    {
        private readonly TwoDVector _upLeft;
        private readonly TwoDVector _upRight;
        public TwoDVector Aim;
        private readonly float _maxR;
        private float _nowR;
        private readonly float _theta;

        public static AngleSight StandardAngleSight()
        {
            return new AngleSight(TempConfig.StandardSightR,TempConfig.StandardSightVector);
        }
        public AngleSight(float r, TwoDVector upLeft)
        {
            Aim = new TwoDVector(1f, 0f);
            _nowR = r;
            var vector = upLeft.GetUnit();
            _upLeft = vector;
            _upRight = new TwoDVector(vector.X, -vector.Y);
            _maxR = r;
            var cosA = vector.X;
            var cos2A = 2 * cosA * cosA - 1;
            _theta = MathTools.Acos(cos2A);
        }

        public bool InSight(TwoDVectorLine sLine, SightMap map)
        {
            var twoDVector = sLine.GetVector().ClockwiseTurn(Aim);
            var c1 = twoDVector.Cross(_upLeft);
            var c2 = twoDVector.Cross(_upRight);
            var b = twoDVector.SqNorm() <= _nowR * _nowR;
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
            var t = cosT > cos2A ? MathTools.Acos(cosT) : _theta;

            var nowRSquare = _nowR * _nowR;
            var snowR = nowRSquare * t;
            var twoSr = TempConfig.TwoSToSeePerTick - snowR;
            if (twoSr <= 0)
            {
                var sqrt = MathTools.Sqrt(TempConfig.TwoSToSeePerTick / t);
                _nowR = MathTools.Min(_maxR, sqrt);
            }
            else
            {
                var rSquare = twoSr / _theta + nowRSquare;
                var sqrt = MathTools.Sqrt(rSquare);
                _nowR = MathTools.Min(_maxR, sqrt);
            }
        }
    }
}