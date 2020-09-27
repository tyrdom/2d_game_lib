using System;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public interface IAntiActBuffConfig
    {
        public uint TickLast { get; }

        public IAntiActBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, CharacterStatus whoDid);

        public void PickBySomeOne(CharacterStatus characterStatus);
    }


    public class PushEarthAntiActBuffConfig : IAntiActBuffConfig
    {
        private float PushForce; //推力
        private PushType PushType; //方向或者中心
        private TwoDVector? PushFixVector; //修正向量，中心push为中心点，方向为方向修正
        public uint TickLast { get; }

        public PushEarthAntiActBuffConfig(float pushForce, PushType pushType, TwoDVector? pushFixVector,
            uint lastTick)
        {
            PushForce = pushForce;
            PushType = pushType;
            PushFixVector = pushFixVector;
            TickLast = lastTick;
        }

        private IAntiActBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float friction)
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

        public void PickBySomeOne(CharacterStatus characterStatus)
        {
        }
    }

    public class PushAirAntiActBuffConfig : IAntiActBuffConfig
    {
        private float PushForce;
        private PushType PushType;
        private float UpForce;

        private TwoDVector? PushFixVector;
        public uint TickLast { get; }

        public PushAirAntiActBuffConfig(float pushForce, PushType pushType, float upForce,
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

        public IAntiActBuff GenBuff(TwoDPoint anchor, TwoDPoint obPos, TwoDVector aim, float? height
            , float upSpeed, BodySize bodySize, CharacterStatus whoDid)
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

        public void PickBySomeOne(CharacterStatus characterStatus)
        {
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
        public CatchAntiActBuffConfig(TwoDVector[] twoDVectors, uint tickLast, Skill trickSkill
        )
        {
            TwoDVectors = twoDVectors;
            TickLast = tickLast;
            TrickSkill = trickSkill;
        }

        private TwoDVector[] TwoDVectors { get; }
        public uint TickLast { get; }
        public Skill TrickSkill { get; }


        private IAntiActBuff GenABuff(TwoDPoint anchor, TwoDVector aim, CharacterStatus whoDid)
        {
            var twoDPoints = TwoDVectors.Select(twoDVector => twoDVector.AntiClockwiseTurn(aim.GetUnit()))
                .Select(anchor.Move)
                .ToList();
#if DEBUG
            Console.Out.WriteLine($"pos {anchor.ToString()} aim {aim.ToString()}");
            foreach (var twoDVector in TwoDVectors)
            {
                Console.Out.WriteLine($"{twoDVector.ToString()}");
            }

            foreach (var twoDPoint in twoDPoints)
            {
                Console.Out.WriteLine($"{twoDPoint.ToString()}");
            }
#endif
            var caught = new Caught(twoDPoints, TickLast, whoDid);
            return caught;
        }

        public IAntiActBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, CharacterStatus whoDid)
        {
            var antiActBuff = GenABuff(pos, aim, whoDid);
            return antiActBuff;
        }

        public void PickBySomeOne(CharacterStatus characterStatus)
        {
            TrickSkill.PickedBySomeOne(characterStatus);
        }
    }
}