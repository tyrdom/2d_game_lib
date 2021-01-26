using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IStunBuffConfig
    {
        public uint TickLast { get; }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, IBattleUnitStatus whoDid);
    }

    public static class StunBuffStandard
    {
        public static IStunBuffConfig GenBuffByConfig(push_buff pushBuff)
        {
            var pushType = pushBuff.PushType switch
            {
                game_config.PushType.Vector => PushType.Vector,
                game_config.PushType.Center => PushType.Center,
                _ => throw new ArgumentOutOfRangeException()
            };
            var pushAboutVector = pushBuff.FixVector.Any()
                ? GameTools.GenVectorByConfig(pushBuff.FixVector.First())
                : null;

            if (pushBuff.UpForce > 0)
            {
                return new PushAirStunBuffConfig(pushBuff.PushForce, pushType, pushBuff.UpForce, pushAboutVector,
                    CommonConfig.GetTickByTime(pushBuff.LastTime));
            }

            return new PushEarthStunBuffConfig(pushBuff.PushForce, pushType, pushAboutVector,
                CommonConfig.GetTickByTime(pushBuff.LastTime));
        }

        private static IStunBuffConfig GenBuffByConfig(caught_buff caughtBuff)
        {
            var twoDVectors = caughtBuff.CatchKeyPoints
                .ToDictionary(
                    x =>
                    {
                        Console.Out.WriteLine($"time is {CommonConfig.GetTickByTime(x.key_time)}");
                        return CommonConfig.GetTickByTime(x.key_time);
                    },
                    x => GameTools.GenVectorByConfig(x.key_point))
                .Select(pair => (pair.Key, pair.Value))
                .ToList();
            twoDVectors.Sort((x, y) => x.Key.CompareTo(y.Key));

            var vectors = new List<TwoDVector>();

            var (key, value) = twoDVectors.FirstOrDefault();
            if (key != 0 || value == null) throw new Exception($"no good key vectors at caught_buff {caughtBuff.id}");
            vectors.Add(value);
            var nk = key;
            var nowVector = value;
            if (twoDVectors.Count <= 1)
                return new CatchStunBuffConfig(vectors.ToArray(), CommonConfig.GetTickByTime(caughtBuff.LastTime),
                    Skill.GenSkillById(caughtBuff.TrickSkill));
            for (var index = 1; index < twoDVectors.Count; index++)
            {
                var (k1, v1) = twoDVectors[index];
                if (v1 != null)
                {
                    var genLinearListToAnother = nowVector.GenLinearListToAnother(v1, (int) (k1 - nk));
                    vectors.AddRange(genLinearListToAnother);
                }
                else
                {
                    throw new Exception($"no good config at caught_buff {caughtBuff.id} ");
                }
            }

            return new CatchStunBuffConfig(vectors.ToArray(), CommonConfig.GetTickByTime(caughtBuff.LastTime),
                Skill.GenSkillById(caughtBuff.TrickSkill));
        }

        public static IStunBuffConfig GenBuffByC(buff_type buffConfigToOpponentType, string configToOpponent)
        {
            switch (buffConfigToOpponentType)
            {
                case buff_type.push_buff:
                    if (LocalConfig.Configs.push_buffs.TryGetValue(configToOpponent, out var value))
                    {
                        return GenBuffByConfig(value);
                    }

                    throw new DirectoryNotFoundException($"not such p buff {configToOpponent}");

                case buff_type.caught_buff:
                    if (LocalConfig.Configs.caught_buffs.TryGetValue(configToOpponent, out var value2))
                    {
                        return GenBuffByConfig(value2);
                    }

                    throw new DirectoryNotFoundException($"not such c buff {configToOpponent}");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
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

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float friction,
            IBattleUnitStatus battleUnitStatus)
        {
            var dVector1 = unit.Multi(speed);

            var vector1 = unit.Multi(speed > 0 ? friction : -friction);

#if DEBUG
            Console.Out.WriteLine($" vector1~~~~ {vector1.ToString()}");
#endif
            if (height == null)
            {
                var pushOnEarth = new PushOnEarth(dVector1, vector1, TickLast, battleUnitStatus);
                return pushOnEarth;
            }

            {
                var pushOnAir = new PushOnAir(dVector1, height.Value, upSpeed, TickLast, battleUnitStatus);
                return pushOnAir;
            }
        }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            BodySize bodySize, IBattleUnitStatus whoDid)
        {
            var mass = LocalConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDPoint = PushFixVector != null ? pos.Move(PushFixVector) : pos;
                    var twoDVector = twoDPoint.GenVector(obPos).GetUnit();
                    var genBuffFromUnit =
                        GenBuffFromUnit(twoDVector, PushForce / mass, height, upSpeed, LocalConfig.Friction, whoDid);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var clockwiseTurn = PushFixVector != null ? aim.ClockwiseTurn(PushFixVector) : aim;
                    var unit = clockwiseTurn.GetUnit();
                    var genBuffFromUnit1 =
                        GenBuffFromUnit(unit, PushForce / mass, height, upSpeed, LocalConfig.Friction, whoDid);
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
            var mass = LocalConfig.SizeToMass[bodySize];
            switch (PushType)
            {
                case PushType.Center:
                    var twoDPoint = PushFixVector != null ? anchor.Move(PushFixVector) : anchor;
                    var twoDVector = twoDPoint.GenVector(obPos).GetUnit();
                    var genBuffFromUnit = GenBuffFromUnit(twoDVector, PushForce / mass, height, upSpeed, mass, whoDid);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var clockwiseTurn = PushFixVector != null ? aim.ClockwiseTurn(PushFixVector) : aim;
                    var unit = clockwiseTurn.GetUnit();
                    var genBuffFromUnit1 = GenBuffFromUnit(unit, PushForce / mass, height, upSpeed, mass, whoDid);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float f,
            IBattleUnitStatus battleUnitStatus)
        {
            var max = MathTools.Max(GenUp(height, f), upSpeed);
            var pushOnAir = new PushOnAir(unit.Multi(speed), height.GetValueOrDefault(0), max, TickLast,
                battleUnitStatus);
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