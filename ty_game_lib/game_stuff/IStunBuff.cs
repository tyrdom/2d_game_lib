using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IStunBuff
    {
        public IBattleUnitStatus Caster { get; }
        public uint RestTick { get; set; }
        public ITwoDTwoP GetItp();
        public (ITwoDTwoP, IStunBuff?) GoTickDrivePos(TwoDPoint oldPt);

        public Damage HitWall();
    }

    public class PushOnEarth : IStunBuff
    {
        private TwoDVector PushVector { get; set; }
        private TwoDVector DecreasePerTick { get; }

        public PushOnEarth(TwoDVector pushVector, TwoDVector decreasePerTick, uint restTick, IBattleUnitStatus caster)
        {
            PushVector = pushVector;
            DecreasePerTick = decreasePerTick;
            RestTick = restTick;
            Caster = caster;
        }

        public IBattleUnitStatus Caster { get; }
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

        public Damage HitWall()
        {
            var sqNorm = PushVector.SqNorm();
            PushVector = TwoDVector.Zero();
            RestTick = RestTick + 1 + (uint) (sqNorm * LocalConfig.HitWallTickParam);

            return Caster.GenDamage(sqNorm * sqNorm * CommonConfig.OtherConfig.hit_wall_dmg_param, true);
        }
    }

    public class PushOnAir : IStunBuff
    {
        private TwoDVector PushVector { get; set; }
        public float Height;
        public float UpSpeed;


        public PushOnAir(TwoDVector pushVector, float height, float upSpeed, uint restTick, IBattleUnitStatus caster)
        {
            PushVector = pushVector;
            Height = height;
            UpSpeed = upSpeed;
            RestTick = restTick;
            Caster = caster;
        }

        public IBattleUnitStatus Caster { get; }
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
                var decreasePerTick = PushVector.GetUnit().Multi(CommonConfig.OtherConfig.friction);
#if DEBUG
                Console.Out.WriteLine(
                    $"{PushVector}~~~~~air gen_earth buff~~~~~~{decreasePerTick}");

#endif
                var pushOnEarth =
                    new PushOnEarth(PushVector, decreasePerTick, RestTick, Caster);
                return (PushVector, pushOnEarth);
            }

            var nowUpSpeed = UpSpeed - CommonConfig.OtherConfig.g_acc;
            Height = nowHeight;
            UpSpeed = nowUpSpeed;
            return (PushVector, this);
        }

        public Damage HitWall()
        {
            var sqNorm = PushVector.SqNorm();
            PushVector = TwoDVector.Zero();
            RestTick = RestTick + 1 + (uint) (sqNorm * LocalConfig.HitWallTickParam);
            return Caster.GenDamage(sqNorm * sqNorm * CommonConfig.OtherConfig.hit_wall_dmg_param, true);
        }
    }

    internal class Caught : IStunBuff
    {
        private List<TwoDPoint> MovesOnPoints { get; }
        public IBattleUnitStatus Caster { get; }

        public Caught(List<TwoDPoint> movesOnPoints, uint restTick, IBattleUnitStatus caster)
        {
            MovesOnPoints = movesOnPoints;
            RestTick = restTick;
            Caster = caster;
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
                Caster.CatchingWho = null;
                return (pt, new PushOnEarth(TwoDVector.Zero(), TwoDVector.Zero(), RestTick, Caster));
            }

            Caster.CatchingWho = null;
            return (pt, null);
        }

        public Damage HitWall()
        {
            RestTick += LocalConfig.HitWallCatchTickParam;
            MovesOnPoints.Clear();
            return Caster.GenDamage(CommonConfig.OtherConfig.hit_wall_catch_dmg_param, true);
        }
    }
}