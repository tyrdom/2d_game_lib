using System;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public interface IAntiActBuffConfig
    {
        public IAntiActBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, CharacterInBattle? whoDid);
    }


    public class PushEarthAntiActBuffConfig : IAntiActBuffConfig
    {
        private float RawTwoDVectorX;
        private PushType PushType;
        private TwoDVector pushAboutVector;
        private int TickLast;

        IAntiActBuff GenBuffFromUnit(TwoDVector unit, float speed, float? air, float upSpeed, float friction)
        {
            var dVector1 = unit.Multi(speed);

            var vector1 = unit.Multi(speed > 0 ? friction : -friction);
            if (air == null)
            {
                var pushOnEarth = new PushOnEarth(dVector1, vector1, TickLast);
                return pushOnEarth;
            }

            {
                var pushOnAir = new PushOnAir(dVector1, air.Value, upSpeed, TickLast);
                return pushOnAir;
            }
        }

        public IAntiActBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, CharacterInBattle? whoDid)
        {
            var f = TempConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDVector = pos.Move(pushAboutVector).GenVector(obPos).GetUnit();
                    var genBuffFromUnit =
                        GenBuffFromUnit(twoDVector, RawTwoDVectorX / f, height, upSpeed, TempConfig.Friction);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var unit = aim.ClockwiseTurn(pushAboutVector).GetUnit();
                    var genBuffFromUnit1 =
                        GenBuffFromUnit(unit, RawTwoDVectorX / f, height, upSpeed, TempConfig.Friction);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PushAirAntiActBuffConfig : IAntiActBuffConfig
    {
        private float RawTwoDVectorX;
        private PushType PushType;
        private float UpForce;

        private TwoDVector pushAboutVector;
        private int TickLast;

        public PushAirAntiActBuffConfig(float rawTwoDVectorX, PushType pushType, float upForce,
            TwoDVector pushAboutVector, int tickLast)
        {
            RawTwoDVectorX = rawTwoDVectorX;
            PushType = pushType;
            UpForce = upForce;
            this.pushAboutVector = pushAboutVector;
            TickLast = tickLast;
        }

        float GenUp(float? air, float bodyMass)
        {
            var maxUp = GameTools.GetMaxUpSpeed(air);
            return MathF.Min(maxUp, UpForce / bodyMass);
        }

        public IAntiActBuff GenBuff(TwoDPoint anchor, TwoDPoint obPos, TwoDVector aim, float? height
            , float upSpeed, BodySize bodySize, CharacterInBattle? whoDid)
        {
            var f = TempConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDVector = anchor.Move(pushAboutVector).GenVector(obPos).GetUnit();
                    var genBuffFromUnit = GenBuffFromUnit(twoDVector, RawTwoDVectorX / f, height, upSpeed, f);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var unit = aim.ClockwiseTurn(pushAboutVector).GetUnit();
                    var genBuffFromUnit1 = GenBuffFromUnit(unit, RawTwoDVectorX / f, height, upSpeed, f);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IAntiActBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float f)
        {
            var max = MathF.Max(GenUp(height, f), upSpeed);
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
        public CatchAntiActBuffConfig(TwoDVector[] twoDVectors, int lastTick, Skill? trickSkill)
        {
            TwoDVectors = twoDVectors;
            LastTick = lastTick;
            TrickSkill = trickSkill;
        }

        private TwoDVector[] TwoDVectors;
        private int LastTick;
        public Skill? TrickSkill;

        public IAntiActBuff GenABuff(TwoDPoint anchor, TwoDVector aim,  CharacterInBattle whoDid)
        {
            var twoDPoints = TwoDVectors.Select(twoDVector => twoDVector.AntiClockwiseTurn(aim.GetUnit()))
                .Select(anchor.Move).ToList();
            var caught = new Caught(twoDPoints, LastTick, ref whoDid);
            return caught;
        }

        public IAntiActBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, CharacterInBattle? whoDid)
        {
            var antiActBuff = GenABuff(pos, aim,  whoDid);
            return antiActBuff;
        }
    }
}