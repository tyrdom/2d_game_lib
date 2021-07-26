using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IStunBuffMaker
    {
        public uint TickLast { get; }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            size bodySize, IBattleUnitStatus whoDid, bool canFix = true);
    }

    public static class StunBuffStandard
    {
        public static void FixByTaker(ICanFixStunBuff canFixStunBuff, StunFixStatus stunFixStatus)
        {
            if (canFixStunBuff.ForceFixMulti > 0)
            {
                var twoDVector =
                    canFixStunBuff.PushVector.Multi(stunFixStatus.TakeStunForceMulti * canFixStunBuff.ForceFixMulti);
                canFixStunBuff.PushVector = twoDVector;
            }

            if (canFixStunBuff.TickFixMulti > 0)
            {
                canFixStunBuff.RestTick = (uint) MathTools.Max(1,
                    canFixStunBuff.RestTick * canFixStunBuff.TickFixMulti * stunFixStatus.TakeStunTickMulti);
            }
        }

        public static IStunBuffMaker GenBuffByConfig(push_buff pushBuff)
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
                return new PushAirStunBuffMaker(pushBuff.PushForce, pushType, pushBuff.UpForce, pushAboutVector,
                    pushBuff.LastTime);
            }

            return new PushEarthStunBuffMaker(pushBuff.PushForce, pushType, pushAboutVector,
                pushBuff.LastTime);
        }

        private static IStunBuffMaker GenBuffByConfig(caught_buff caughtBuff)
        {
            var twoDVectors = caughtBuff.CatchKeyPoints
                .ToDictionary(
                    x =>
                    {
#if DEBUG
                        // Console.Out.WriteLine($"time is {CommonConfig.GetTickByTime(x.key_time)}");
#endif

                        return x.key_time;
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
                return new CatchStunBuffMaker(vectors.ToArray(), caughtBuff.LastTime,
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

            return new CatchStunBuffMaker(vectors.ToArray(), caughtBuff.LastTime,
                Skill.GenSkillById(caughtBuff.TrickSkill));
        }

        public static IStunBuffMaker GenBuffByC(buff_type buffConfigToOpponentType, string configToOpponent)
        {
            switch (buffConfigToOpponentType)
            {
                case buff_type.push_buff:
                    if (CommonConfig.Configs.push_buffs.TryGetValue(configToOpponent, out var value))
                    {
                        return GenBuffByConfig(value);
                    }

                    throw new KeyNotFoundException($"not such p buff {configToOpponent}");

                case buff_type.caught_buff:
                    if (CommonConfig.Configs.caught_buffs.TryGetValue(configToOpponent, out var value2))
                    {
                        return GenBuffByConfig(value2);
                    }

                    throw new KeyNotFoundException($"not such c buff {configToOpponent}");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PushEarthStunBuffMaker : IStunBuffMaker
    {
        private float PushForce { get; } //推动量
        private PushType PushType { get; } //方向或者中心
        private TwoDVector? PushFixVector { get; } //修正向量，中心push为中心点，方向为方向修正
        public uint TickLast { get; }

        public PushEarthStunBuffMaker(float pushForce, PushType pushType, TwoDVector? pushFixVector,
            uint lastTick)
        {
            PushForce = pushForce;
            PushType = pushType;
            PushFixVector = pushFixVector;
            TickLast = lastTick;
        }

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float friction,
            IBattleUnitStatus battleUnitStatus, bool canFix)
        {
            var stunFixStatus = battleUnitStatus.GetStunFixStatus();
            var makeStunForceMulti = stunFixStatus.MakeStunForceMulti;
            var makeStunTickMulti = stunFixStatus.MakeStunTickMulti;
            var u = canFix ? MathTools.Max(1, (uint) MathTools.Round(makeStunTickMulti * TickLast)) : TickLast;
            var dVector1 = canFix ? unit.Multi(speed * makeStunForceMulti) : unit.Multi(speed);
            var vector1 = unit.Multi(speed > 0 ? friction : -friction);

            if (height == null)
            {
                var pushOnEarth = new PushOnEarth(dVector1, vector1, TickLast, battleUnitStatus,
                    canFix ? makeStunForceMulti : -1f, canFix ? makeStunTickMulti : -1f);
#if DEBUG
                Console.Out.WriteLine($" vector1~~~~ {vector1} push {dVector1}");
#endif
                return pushOnEarth;
            }

            {
                var pushOnAir = new PushOnAir(dVector1, height.Value, upSpeed, TickLast, battleUnitStatus,
                    canFix ? makeStunForceMulti : -1f, canFix ? makeStunTickMulti : -1f);
                return pushOnAir;
            }
        }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            size bodySize, IBattleUnitStatus whoDid, bool canFix)
        {
            var mass = CommonConfig.Configs.bodys[bodySize].mass;
            switch (PushType)
            {
                case PushType.Center:
                    var twoDPoint = PushFixVector != null ? pos.Move(PushFixVector) : pos;
                    var twoDVector = twoDPoint.GenVector(obPos).GetUnit();
                    var genBuffFromUnit =
                        GenBuffFromUnit(twoDVector, PushForce / mass, height, upSpeed,
                            CommonConfig.OtherConfig.friction, whoDid, canFix);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var clockwiseTurn = PushFixVector != null ? aim.ClockwiseTurn(PushFixVector) : aim;
                    var unit = clockwiseTurn.GetUnit();
                    var genBuffFromUnit1 =
                        GenBuffFromUnit(unit, PushForce / mass, height, upSpeed, CommonConfig.OtherConfig.friction,
                            whoDid, canFix);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PushAirStunBuffMaker : IStunBuffMaker
    {
        private float PushForce { get; }
        private PushType PushType { get; }
        private float UpForce { get; }

        private TwoDVector? PushFixVector { get; }
        public uint TickLast { get; }

        public PushAirStunBuffMaker(float pushForce, PushType pushType, float upForce,
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
            , float upSpeed, size bodySize, IBattleUnitStatus whoDid, bool canFix)
        {
            var mass = CommonConfig.Configs.bodys[bodySize].mass;

            switch (PushType)
            {
                case PushType.Center:
                    var twoDPoint = PushFixVector != null ? anchor.Move(PushFixVector) : anchor;
                    var twoDVector = twoDPoint.GenVector(obPos).GetUnit();
                    var genBuffFromUnit = GenBuffFromUnit(twoDVector, PushForce / mass, height, upSpeed, mass, whoDid,
                        canFix);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var clockwiseTurn = PushFixVector != null ? aim.ClockwiseTurn(PushFixVector) : aim;
                    var unit = clockwiseTurn.GetUnit();
                    var genBuffFromUnit1 =
                        GenBuffFromUnit(unit, PushForce / mass, height, upSpeed, mass, whoDid, canFix);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float f,
            IBattleUnitStatus battleUnitStatus, bool canFix)
        {
            var stunFixStatus = battleUnitStatus.GetStunFixStatus();
            var makeStunForceMulti = stunFixStatus.MakeStunForceMulti;
            var makeStunTickMulti = stunFixStatus.MakeStunTickMulti;
            var max = MathTools.Max(GenUp(height, f), upSpeed);
            var twoDVector = unit.Multi(speed);
            var pushOnAir = new PushOnAir(twoDVector, height.GetValueOrDefault(0), max, TickLast,
                battleUnitStatus, canFix ? makeStunForceMulti : -1f, canFix ? makeStunTickMulti : -1f);
            return pushOnAir;
        }
    }

    public enum PushType
    {
        Center,
        Vector
    }

    public class CatchStunBuffMaker : IStunBuffMaker
    {
        public CatchStunBuffMaker(TwoDVector[] twoDVectors, uint tickLast, Skill trickSkill
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
            size bodySize, IBattleUnitStatus whoDid, bool canFix = false)
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