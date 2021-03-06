﻿using System;
using collision_and_rigid;

namespace game_stuff
{
    public class AngleSight
    {
        public Scope StandardScope { get; }

        public float NowR { get; private set; }
        public TwoDVector Aim { get; private set; }

        public static AngleSight StandardAngleSight()
        {
            return new AngleSight(StuffLocalConfig.StandardSightVector);
        }

        private AngleSight(TwoDVector upLeft)
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


        public bool InSight(TwoDVectorLine sLine, SightMap? map, Scope? scope)
        {
            var useScope = scope ?? StandardScope;

            var twoDVector = sLine.GetVector().ClockwiseTurn(Aim);
            var c1 = twoDVector.Cross(useScope.FaceRightLeftPt);
            var c2 = twoDVector.Cross(useScope.FaceRightRightPt);
            var b = twoDVector.SqNorm() <= NowR * NowR;

            var isBlockSightLine = map?.IsBlockSightLine(sLine) ?? false;


            var blockSightLine = c1 >= 0 && c2 <= 0 && b && !isBlockSightLine;

#if DEBUG
            // Console.Out.WriteLine($" _nowR ::: {NowR}");

#endif
            return blockSightLine;
        }


        public bool OpChangeAim(TwoDVector? newAim, Scope? scope, float twoSToSeePerTick)
        {
            var temp = NowR;
            var useScope = scope ?? StandardScope;

            var oldAim = Aim;

            if (newAim != null) Aim = newAim.GetUnit();

            var cosT = Aim.GetUnit().Dot(oldAim) / oldAim.Norm();
            var cosA = useScope.FaceRightLeftPt.X;
            var cos2A = 2 * cosA * cosA - 1;
            var t = cosT > cos2A ? MathTools.Acos(cosT) : useScope.Theta;

            var nowRSquare = NowR * NowR;
            var snowR = nowRSquare * t;
            var twoSr = twoSToSeePerTick - snowR;
            if (twoSr <= 0)
            {
                var sqrt = MathTools.Sqrt(twoSToSeePerTick / t);
                NowR = MathTools.Min(useScope.MaxR, sqrt);
            }
            else
            {
                var rSquare = twoSr / useScope.Theta + nowRSquare;
                var sqrt = MathTools.Sqrt(rSquare);
                NowR = MathTools.Min(useScope.MaxR, sqrt);
            }

            var b = MathTools.Abs(temp - NowR) > 0.001f;
            return b;
        }

        public Zone GenZone(TwoDPoint getAnchor)
        {
            var up = getAnchor.Y + NowR;
            var down = getAnchor.Y - NowR;
            var left = getAnchor.X - NowR;
            var right = getAnchor.X + NowR;
            return new Zone(up, down, left, right);
        }
    }
}