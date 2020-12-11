using System;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public interface IStunBuffConfig
    {
        public uint TickLast { get; }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, IBattleUnitStatus whoDid);
    }


    public class PushEarthStunBuffConfig : IStunBuffConfig
    {
        private float PushForce; //推力
        private PushType PushType; //方向或者中心
        private TwoDVector? PushFixVector; //修正向量，中心push为中心点，方向为方向修正
        public uint TickLast { get; }

        public PushEarthStunBuffConfig(float pushForce, PushType pushType, TwoDVector? pushFixVector,
            uint lastTick)
        {
            PushForce = pushForce;
            PushType = pushType;
            PushFixVector = pushFixVector;
            TickLast = lastTick;
        }

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float friction)
        {
            var dVector1 = unit.Multi(speed);

            var vector1 = unit.Multi(speed > 0 ? friction : -friction);

#if DEBUG
            Console.Out.WriteLine($" vector1~~~~ {vector1.ToString()}");
#endif
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

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, IBattleUnitStatus whoDid)
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

    public class PushAirStunBuffConfig : IStunBuffConfig
    {
        private float PushForce;
        private PushType PushType;
        private float UpForce;

        private TwoDVector? PushFixVector;
        public uint TickLast { get; }

        public PushAirStunBuffConfig(float pushForce, PushType pushType, float upForce,
            TwoDVector? pushFixVector, uint tickLast)
        {
            PushForce = pushForce;
            PushType = pushType;
            UpForce = upForce;
            PushFixVector = pushFixVector;
            TickLast = tickLast;
        }

        private float GenUp(float? air, float bodyMass)
        {
            var maxUp = GameTools.GetMaxUpSpeed(air);
            return MathTools.Min(maxUp, UpForce / bodyMass);
        }

        public IStunBuff GenBuff(TwoDPoint anchor, TwoDPoint obPos, TwoDVector aim, float? height
            , float upSpeed, BodySize bodySize, IBattleUnitStatus whoDid)
        {
            var mass = TempConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDPoint = PushFixVector != null ? anchor.Move(PushFixVector) : anchor;
                    var twoDVector = twoDPoint.GenVector(obPos).GetUnit();
                    var genBuffFromUnit = GenBuffFromUnit(twoDVector, PushForce / mass, height, upSpeed, mass);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var clockwiseTurn = PushFixVector != null ? aim.ClockwiseTurn(PushFixVector) : aim;
                    var unit = clockwiseTurn.GetUnit();
                    var genBuffFromUnit1 = GenBuffFromUnit(unit, PushForce / mass, height, upSpeed, mass);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float f)
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

    public class CatchStunBuffConfig : IStunBuffConfig
    {
        public CatchStunBuffConfig(TwoDVector[] twoDVectors, uint tickLast, Skill trickSkill
        )
        {
            TwoDVectors = twoDVectors;
            TickLast = tickLast;
            TrickSkill = trickSkill;
        }

        private TwoDVector[] TwoDVectors { get; }
        public uint TickLast { get; }
        public Skill TrickSkill { get; }


        private IStunBuff GenABuff(TwoDPoint anchor, TwoDVector aim, IBattleUnitStatus whoDid)
        {
            var twoDPoints = TwoDVectors.Select(twoDVector => twoDVector.AntiClockwiseTurn(aim.GetUnit()))
                .Select(anchor.Move)
                .ToList();
#if DEBUG
            Console.Out.WriteLine($"pos {anchor} aim {aim}");
            foreach (var twoDVector in TwoDVectors)
            {
                Console.Out.WriteLine($"{twoDVector}");
            }

            foreach (var twoDPoint in twoDPoints)
            {
                Console.Out.WriteLine($"{twoDPoint}");
            }
#endif
            var caught = new Caught(twoDPoints, TickLast, whoDid);
            return caught;
        }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, IBattleUnitStatus whoDid)
        {
            var antiActBuff = GenABuff(pos, aim, whoDid);
            return antiActBuff;
        }

        public void PickBySomeOne(IBattleUnitStatus characterStatus)
        {
            TrickSkill.PickedBySomeOne(characterStatus);
        }
    }
}