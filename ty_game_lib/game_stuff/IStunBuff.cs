using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public interface IStunBuff
    {
        public uint RestTick { get; set; }
        public ITwoDTwoP GetItp();
        public (ITwoDTwoP, IStunBuff?) GoTickDrivePos(TwoDPoint oldPt);

        public float HitWall();
    }

    public class PushOnEarth : IStunBuff
    {
        private TwoDVector PushVector { get; set; }
        private TwoDVector DecreasePerTick { get; }

        public PushOnEarth(TwoDVector pushVector, TwoDVector decreasePerTick, uint restTick)
        {
            PushVector = pushVector;
            DecreasePerTick = decreasePerTick;
            RestTick = restTick;
        }

        public uint RestTick { get; set; }

        public ITwoDTwoP GetItp()
        {
            return PushVector;
        }

        public (ITwoDTwoP, IStunBuff?) GoTickDrivePos(TwoDPoint oldPt)
        {
            RestTick -= 1;
            if (RestTick <= 0)
            {
                return (PushVector, null);
            }

            var push = PushVector;
#if DEBUG
            Console.Out.WriteLine($"{push.GetType().TypeHandle.Value.ToString()}  PV before::{PushVector}");
#endif

            var twoDVector = push.Minus(DecreasePerTick);
#if DEBUG
            Console.Out.WriteLine(
                $" {GetType()} Decrease~~{DecreasePerTick}  PV  mid::{twoDVector}");
#endif
            var dot = push.Dot(twoDVector);
            if (dot <= 0)
            {
                twoDVector = new TwoDVector(0f, 0f);
            }

            PushVector = twoDVector;
#if DEBUG
            Console.Out.WriteLine($"PV after::{PushVector}");
#endif
            return (push, this);
        }

        public float HitWall()
        {
            var sqNorm = PushVector.SqNorm();
            PushVector = TwoDVector.Zero();
            RestTick = RestTick + 1 + (uint) (sqNorm * TempConfig.HitWallTickParam);
            return sqNorm;
        }
    }

    public class PushOnAir : IStunBuff
    {
        private TwoDVector PushVector { get; set; }
        public float Height;
        public float UpSpeed;


        public PushOnAir(TwoDVector pushVector, float height, float upSpeed, uint restTick)
        {
            PushVector = pushVector;
            Height = height;
            UpSpeed = upSpeed;
            RestTick = restTick;
        }

        public uint RestTick { get; set; }

        public ITwoDTwoP GetItp()
        {
            return PushVector;
        }

        public (ITwoDTwoP, IStunBuff?) GoTickDrivePos(TwoDPoint oldPt)
        {
            var nowHeight = Height + UpSpeed;
            RestTick -= 1;
            if (RestTick <= 0)
            {
                return (PushVector, null);
            }

            if (nowHeight <= 0)
            {
                var decreasePerTick = PushVector.GetUnit().Multi(TempConfig.Friction);
#if DEBUG
                Console.Out.WriteLine(
                    $"{PushVector}~~~~~air gen_earth buff~~~~~~{decreasePerTick}");

#endif
                var pushOnEarth =
                    new PushOnEarth(PushVector, decreasePerTick, RestTick);
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
            RestTick = RestTick + 1 + (uint) (sqNorm * TempConfig.HitWallTickParam);
            return sqNorm;
        }
    }

    class Caught : IStunBuff
    {
        public List<TwoDPoint> MovesOnPoints { get; }
        public IBattleUnitStatus WhoCatchMe { get; }

        public Caught(List<TwoDPoint> movesOnPoints, uint restTick, IBattleUnitStatus whoCatchMe)
        {
            MovesOnPoints = movesOnPoints;
            RestTick = restTick;
            WhoCatchMe = whoCatchMe;
        }

        public uint RestTick { get; set; }

        public ITwoDTwoP GetItp()
        {
            return MovesOnPoints.Count > 0 ? MovesOnPoints[0] : TwoDPoint.Zero();
        }

        public (ITwoDTwoP, IStunBuff?) GoTickDrivePos(TwoDPoint oldPt)
        {
            RestTick -= 1;
            var count = MovesOnPoints.Count;

            var pt = count <= 0 ? oldPt : MovesOnPoints[0];


            if (count > 0)
            {
                MovesOnPoints.RemoveAt(0);
                return (pt, this);
            }
#if DEBUG
            Console.Out.WriteLine($"~~~~~caught gen_earth buff~~~~~~{pt}");
#endif
            if (RestTick > 0)
            {
                WhoCatchMe.CatchingWho = null;
                return (pt, new PushOnEarth(TwoDVector.Zero(), TwoDVector.Zero(), RestTick));
            }

            WhoCatchMe.CatchingWho = null;
            return (pt, null);
        }

        public float HitWall()
        {
            RestTick += TempConfig.HitWallCatchTickParam;
            return TempConfig.HitWallCatchDmgParam;
        }
    }
}