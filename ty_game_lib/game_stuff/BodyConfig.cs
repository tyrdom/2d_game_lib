using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class BodyConfig
    {
    }

    public class WeaponConfig
    {
    }

    public class SkillConfig

    {
    }

    public class BulletConfig
    {
        private IShape RawShape;
        private AntiActBuffConfig SuccessActBuffConfigToOpponent;

        private AntiActBuffConfig FailActBuffConfigToSelf;
        private DamageBuffConfig[] DamageBuffConfigs;
    }

    public abstract class AntiActBuffConfig
    {
    }

    public class PushEarthAntiActBuffConfig : AntiActBuffConfig
    {
        private float RawTwoDVectorX;
        private PushType PushType;
        private int TickLast;

        AntiActBuff GenBuffFromUnit(TwoDVector unit, float speed, float? air, float upSpeed, float friction)
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

        public AntiActBuff GenBuff(TwoDPoint anchor, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize)
        {
            var f = TempConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDVector = anchor.GenVector(obPos).GetUnit();
                    var genBuffFromUnit =
                        GenBuffFromUnit(twoDVector, RawTwoDVectorX / f, height, upSpeed, TempConfig.Friction);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var unit = aim.GetUnit();
                    var genBuffFromUnit1 =
                        GenBuffFromUnit(unit, RawTwoDVectorX / f, height, upSpeed, TempConfig.Friction);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PushAirAntiActBuffConfig : AntiActBuffConfig
    {
        private float RawTwoDVectorX;
        private PushType PushType;
        private float UpForce;
        private int TickLast;

        float GenUp(float? air, float bodyMass)
        {
            var maxUp = GameTools.GetMaxUp(air);
            return MathF.Min(maxUp, UpForce / bodyMass);
        }

        public AntiActBuff GenBuff(TwoDPoint anchor, TwoDPoint obPos, TwoDVector aim, float? height
            , float upSpeed, BodySize bodySize)
        {
            var f = TempConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDVector = anchor.GenVector(obPos).GetUnit();
                    var genBuffFromUnit = GenBuffFromUnit(twoDVector, RawTwoDVectorX / f, height, upSpeed, f);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var unit = aim.GetUnit();
                    var genBuffFromUnit1 = GenBuffFromUnit(unit, RawTwoDVectorX / f, height, upSpeed, f);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AntiActBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float f)
        {
            var max = MathF.Max(GenUp(height, f), upSpeed);
            var pushOnAir = new PushOnAir(unit.Multi(speed), height.GetValueOrDefault(0), max, TickLast);
            return pushOnAir;
        }
    }

    internal enum PushType
    {
        Center,
        Vector
    }

    public class CatchAntiActBuffConfig : AntiActBuffConfig
    {
        private TwoDVector[] TwoDVectors;
        private int LastTick;

        public AntiActBuff GenBuff(TwoDPoint anchor, TwoDVector aim, int gid)
        {
            var twoDPoints = TwoDVectors.Select(twoDVector => twoDVector.AntiClockwiseTurn(aim.GetUnit()))
                .Select(anchor.Move).ToList();
            var caught = new Caught(gid, twoDPoints, LastTick);
            return caught;
        }
    }


    public class DamageBuffConfig
    {
    }
}