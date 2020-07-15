using System;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public interface IAntiActBuffConfig
    {
        public IAntiActBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, CharacterStatus whoDid);
    }


    public class PushEarthAntiActBuffConfig : IAntiActBuffConfig
    {
        private float PushForce; //推力
        private PushType PushType; //方向或者中心
        private TwoDVector? PushFixVector; //修正向量，中心push为中心点，方向为方向修正
        private int TickLast;

        public PushEarthAntiActBuffConfig(float pushForce, PushType pushType, TwoDVector pushFixVector,
            int tickLast)
        {
            PushForce = pushForce;
            PushType = pushType;
            PushFixVector = pushFixVector;
            TickLast = tickLast;
        }

        private IAntiActBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float friction)
        {
            var dVector1 = unit.Multi(speed);

            var vector1 = unit.Multi(speed > 0 ? friction : -friction);
            if (height == null)
            {
                var pushOnEarth = new PushOnEarth(dVector1, vector1, TickLast);
                return pushOnEarth;
            }

            {
                var pushOnAir = new PushOnAir(dVector1, height.Value, upSpeed, TickLast);
                return pushOnAir;
            }
        }

        public IAntiActBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, CharacterStatus whoDid)
        {
            var mass = TempConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDPoint = PushFixVector != null ? pos.Move(PushFixVector) : pos;
                    var twoDVector = twoDPoint.GenVector(obPos).GetUnit();
                    var genBuffFromUnit =
                        GenBuffFromUnit(twoDVector, PushForce / mass, height, upSpeed, TempConfig.Friction);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var clockwiseTurn = PushFixVector != null ? aim.ClockwiseTurn(PushFixVector) : aim;
                    var unit = clockwiseTurn.GetUnit();
                    var genBuffFromUnit1 =
                        GenBuffFromUnit(unit, PushForce / mass, height, upSpeed, TempConfig.Friction);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PushAirAntiActBuffConfig : IAntiActBuffConfig
    {
        private float PushForce;
        private PushType PushType;
        private float UpForce;

        private TwoDVector pushAboutVector;
        private int TickLast;

        public PushAirAntiActBuffConfig(float pushForce, PushType pushType, float upForce,
            TwoDVector pushAboutVector, int tickLast)
        {
            PushForce = pushForce;
            PushType = pushType;
            UpForce = upForce;
            this.pushAboutVector = pushAboutVector;
            TickLast = tickLast;
        }

        private float GenUp(float? air, float bodyMass)
        {
            var maxUp = GameTools.GetMaxUpSpeed(air);
            return MathTools.Min(maxUp, UpForce / bodyMass);
        }

        public IAntiActBuff GenBuff(TwoDPoint anchor, TwoDPoint obPos, TwoDVector aim, float? height
            , float upSpeed, BodySize bodySize, CharacterStatus whoDid)
        {
            var mass = TempConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDVector = anchor.Move(pushAboutVector).GenVector(obPos).GetUnit();
                    var genBuffFromUnit = GenBuffFromUnit(twoDVector, PushForce / mass, height, upSpeed, mass);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var unit = aim.ClockwiseTurn(pushAboutVector).GetUnit();
                    var genBuffFromUnit1 = GenBuffFromUnit(unit, PushForce / mass, height, upSpeed, mass);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IAntiActBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float f)
        {
            var max = MathTools.Max(GenUp(height, f), upSpeed);
            var pushOnAir = new PushOnAir(unit.Multi(speed), height.GetValueOrDefault(0), max, TickLast);
            return pushOnAir;
        }
    }

    public enum PushType
    {
        Center,
        Vector
    }

    public class CatchAntiActBuffConfig : IAntiActBuffConfig
    {
        public CatchAntiActBuffConfig(TwoDVector[] twoDVectors, int lastTick, Skill? trickSkill,
            IAntiActBuffConfig? lastBuff)
        {
            TwoDVectors = twoDVectors;
            LastTick = lastTick;
            TrickSkill = trickSkill;
            // LastBuff = lastBuff;
        }

        private TwoDVector[] TwoDVectors { get; }
        private int LastTick { get; }
        public Skill? TrickSkill { get; }
        // public IAntiActBuffConfig? LastBuff { get; }

        public IAntiActBuff GenABuff(TwoDPoint anchor, TwoDVector aim, CharacterStatus whoDid)
        {
            var twoDPoints = TwoDVectors.Select(twoDVector => twoDVector.AntiClockwiseTurn(aim.GetUnit()))
                .Select(anchor.Move).ToList();
            var caught = new Caught(twoDPoints, LastTick, whoDid);
            return caught;
        }

        public IAntiActBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, CharacterStatus whoDid)
        {
            var antiActBuff = GenABuff(pos, aim, whoDid);
            return antiActBuff;
        }
    }
}