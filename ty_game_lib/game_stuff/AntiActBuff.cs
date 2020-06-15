using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public interface IAntiActBuff
    {
        public int RestTick { get; set; }

        public (ITwoDTwoP?, IAntiActBuff?) GoTickDrivePos(TwoDPoint oldPt);

        public float HitWall();
    }

    class PushOnEarth : IAntiActBuff
    {
        public TwoDVector PushVector;
        public TwoDVector DecreasePerTick;

        public PushOnEarth(TwoDVector pushVector, TwoDVector decreasePerTick, int restTick)
        {
            PushVector = pushVector;
            DecreasePerTick = decreasePerTick;
            RestTick = restTick;
        }

        public int RestTick { get; set; }

        public (ITwoDTwoP, IAntiActBuff?) GoTickDrivePos(TwoDPoint oldPt)
        {
            RestTick -= 1;
            if (RestTick <= 0)
            {
                return (PushVector, null);
            }

            var push = PushVector;
            var twoDVector = push.Minus(DecreasePerTick);
            var dot = push.Dot(twoDVector);
            if (dot > 0)
            {
                twoDVector = new TwoDVector(0f, 0f);
            }

            PushVector = twoDVector;

            return (push, this);
        }

        public float HitWall()
        {
            var sqNorm = PushVector.SqNorm();
            PushVector = TwoDVector.Zero();
            RestTick = RestTick + 1 + (int) (sqNorm * TempConfig.HitWallTickParam);
            return sqNorm;
        }
    }

    public class PushOnAir : IAntiActBuff
    {
        public TwoDVector PushVector;
        public float Height;
        public float UpSpeed;


        public PushOnAir(TwoDVector pushVector, float height, float upSpeed, int restTick)
        {
            PushVector = pushVector;
            Height = height;
            UpSpeed = upSpeed;
            RestTick = restTick;
        }

        public int RestTick { get; set; }

        public (ITwoDTwoP, IAntiActBuff?) GoTickDrivePos(TwoDPoint oldPt)
        {
            var nowHeight = Height + UpSpeed;
            RestTick -= 1;
            if (RestTick <= 0)
            {
                return (PushVector, null);
            }

            if (nowHeight <= 0)
            {
                var pushOnEarth =
                    new PushOnEarth(PushVector, PushVector.GetUnit().Multi(TempConfig.Friction), RestTick);
                return (PushVector, pushOnEarth);
            }

            var nowUpSpeed = UpSpeed - TempConfig.G;
            Height = nowHeight;
            UpSpeed = nowUpSpeed;
            return (PushVector, this);
        }

        public float HitWall()
        {
            var sqNorm = PushVector.SqNorm();
            PushVector = TwoDVector.Zero();
            RestTick = RestTick + 1 + (int) (sqNorm * TempConfig.HitWallTickParam);
            return sqNorm;
        }
    }

    class Caught : IAntiActBuff
    {
        public List<TwoDPoint> MovesOnPoints;
        public CharacterInBattle WhoCatchMe;

        public Caught(List<TwoDPoint> movesOnPoints, int restTick, ref CharacterInBattle whoCatchMe)
        {
            MovesOnPoints = movesOnPoints;
            RestTick = restTick;
            WhoCatchMe = whoCatchMe;
        }

        public int RestTick { get; set; }

        public (ITwoDTwoP, IAntiActBuff?) GoTickDrivePos(TwoDPoint oldPt)
        {
            RestTick -= 1;
            var count = MovesOnPoints.Count;

            var pt = count <= 0 ? oldPt : MovesOnPoints[0];

            if (RestTick <= 0)
            {
                WhoCatchMe.Catching = null;
                return (pt, null);
            }

            if (count > 0)
            {
                MovesOnPoints.RemoveAt(0);
            }

            return (pt, this);
        }

        public float HitWall()
        {
            RestTick += TempConfig.HitWallCatchTickParam;
            return TempConfig.HitWallCatchDmgParam;
        }
    }
}