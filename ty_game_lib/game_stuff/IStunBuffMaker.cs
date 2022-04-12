using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IStunBuffMaker : IBuffMaker
    {
        public uint TickLast { get; }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            CharacterStatus whoTake, IBattleUnitStatus whoDid, bool canFix = true);
    }

    public static class StunBuffStandard
    {
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
            if (twoDVectors.Count <= 1)
                return new CatchStunBuffMaker(vectors.ToArray(), caughtBuff.LastTime,
                    Skill.GenSkillById(caughtBuff.TrickSkill));
            for (var index = 1; index < twoDVectors.Count; index++)
            {
                var (k0, twoDVector) = twoDVectors[index - 1];
                var (k1, v1) = twoDVectors[index];
                var dVector = v1.Minus(twoDVector);

                var genLinearListToAnother = TwoDVector.GenLinearListToAnother(dVector, (int)(k1 - k0))
                    .Select(x => x.Sum(twoDVector));
                vectors.AddRange(genLinearListToAnother);
            }

            return new CatchStunBuffMaker(vectors.ToArray(), caughtBuff.LastTime,
                Skill.GenSkillById(caughtBuff.TrickSkill));
        }

        public static IStunBuffMaker GenStunBuffMakerByC(buff_type buffConfigToOpponentType, string configToOpponent)
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

                case buff_type.pull_buff:
                    if (CommonConfig.Configs.pull_buffs.TryGetValue(configToOpponent, out var value3))
                    {
                        return GenBuffByConfig(value3);
                    }

                    throw new KeyNotFoundException($"not such pull buff {configToOpponent}");

                case buff_type.play_buff:
                    throw new KeyNotFoundException($"it is not stun buff {configToOpponent}");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IStunBuffMaker GenBuffByConfig(pull_buff pullBuff)
        {
#if DEBUG
            Console.Out.WriteLine($"pull buff {pullBuff.id}");
#endif
            var pushAboutVector = pullBuff.FixVector.Any()
                ? GameTools.GenVectorByConfig(pullBuff.FixVector.First())
                : null;
            return new PullStunBuffMaker(pullBuff.LastTime, pullBuff.PushForce, pushAboutVector);
        }
    }

    public class PushEarthStunBuffMaker : IStunBuffMaker
    {
        private float PushMomentum { get; } //推动量
        private PushType PushType { get; } //方向或者中心
        private TwoDVector? PushFixVector { get; } //修正向量，中心push为中心点，方向为方向修正
        public uint TickLast { get; }

        public PushEarthStunBuffMaker(float pushMomentum, PushType pushType, TwoDVector? pushFixVector,
            uint lastTick)
        {
            PushMomentum = pushMomentum;
            PushType = pushType;
            PushFixVector = pushFixVector;
            TickLast = lastTick;
        }

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float friction,
            IBattleUnitStatus battleUnitStatus, IBattleUnitStatus whoTake, bool canFix)
        {
            var stunFixStatus = battleUnitStatus.GetStunFixStatus();
            var fixStatus = whoTake.GetStunFixStatus();
            var makeStunForceMulti = stunFixStatus.MakeStunForceMulti * fixStatus.TakeStunForceMulti *
                                     battleUnitStatus.GetStunForceMultiFormBuff();
            var makeStunTickMulti = stunFixStatus.MakeStunTickMulti * fixStatus.TakeStunTickMulti;
            var u = canFix ? MathTools.Max(1, (uint)MathTools.Round(makeStunTickMulti * TickLast)) : TickLast;
            var dVector1 = canFix ? unit.Multi(speed * makeStunForceMulti) : unit.Multi(speed);
            var vector1 = unit.Multi(speed > 0 ? friction : -friction);

            if (height == null)
            {
                var pushOnEarth = new PushStunOnEarth(dVector1, vector1, u, battleUnitStatus);
#if DEBUG
                Console.Out.WriteLine($" vector1~~~~ {vector1} push {dVector1}");
#endif
                return pushOnEarth;
            }

            {
                var pushOnAir = new PushStunOnAir(dVector1, height.Value, upSpeed, u, battleUnitStatus);
                return pushOnAir;
            }
        }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            CharacterStatus whoTake, IBattleUnitStatus whoDid, bool canFix)
        {
            var mass = CommonConfig.Configs.bodys[whoTake.CharacterBody.GetSize()].mass;
            switch (PushType)
            {
                case PushType.Center:
                    var twoDPoint = PushFixVector != null ? pos.Move(PushFixVector.AntiClockwiseTurn(aim)) : pos;
                    var twoDVector = twoDPoint.GenVector(obPos).GetUnit();
                    var genBuffFromUnit =
                        GenBuffFromUnit(twoDVector, PushMomentum / mass, height, upSpeed,
                            CommonConfig.OtherConfig.friction, whoDid, whoTake, canFix);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var clockwiseTurn = PushFixVector != null ? aim.AntiClockwiseTurn(PushFixVector) : aim;
                    var unit = clockwiseTurn.GetUnit();
                    var genBuffFromUnit1 =
                        GenBuffFromUnit(unit, PushMomentum / mass, height, upSpeed, CommonConfig.OtherConfig.friction,
                            whoDid, whoTake, canFix);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PushAirStunBuffMaker : IStunBuffMaker
    {
        private float PushMomentum { get; }
        private PushType PushType { get; }
        private float UpMomentum { get; }

        private TwoDVector? PushFixVector { get; }
        public uint TickLast { get; }

        public PushAirStunBuffMaker(float pushMomentum, PushType pushType, float upMomentum,
            TwoDVector? pushFixVector, uint tickLast)
        {
            PushMomentum = pushMomentum;
            PushType = pushType;
            UpMomentum = upMomentum;
            PushFixVector = pushFixVector;
            TickLast = tickLast;
        }

        private float GenUp(float? air, float bodyMass)
        {
            var maxUp = GameTools.GetMaxUpSpeed(air);
            return MathTools.Min(maxUp, UpMomentum / bodyMass);
        }

        public IStunBuff GenBuff(TwoDPoint anchor, TwoDPoint obPos, TwoDVector aim, float? height
            , float upSpeed, CharacterStatus whoTake, IBattleUnitStatus whoDid, bool canFix)
        {
            var mass = CommonConfig.Configs.bodys[whoTake.CharacterBody.GetSize()].mass;

            switch (PushType)
            {
                case PushType.Center:
                    var twoDPoint = PushFixVector != null ? anchor.Move(PushFixVector.AntiClockwiseTurn(aim)) : anchor;
                    var twoDVector = twoDPoint.GenVector(obPos).GetUnit();
                    var genBuffFromUnit = GenBuffFromUnit(twoDVector, PushMomentum / mass, height, upSpeed, mass,
                        whoDid, whoTake,
                        canFix);
                    return genBuffFromUnit;


                case PushType.Vector:
                    var clockwiseTurn = PushFixVector != null ? aim.AntiClockwiseTurn(PushFixVector) : aim;
                    var unit = clockwiseTurn.GetUnit();
                    var genBuffFromUnit1 =
                        GenBuffFromUnit(unit, PushMomentum / mass, height, upSpeed, mass, whoDid, whoTake, canFix);
                    return genBuffFromUnit1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float speed, float? height, float upSpeed, float f,
            IBattleUnitStatus battleUnitStatus, IBattleUnitStatus whoTake, bool canFix)
        {
            var stunFixStatus = battleUnitStatus.GetStunFixStatus();
            var fixStatus = whoTake.GetStunFixStatus();
            var makeStunForceMulti = stunFixStatus.MakeStunForceMulti * fixStatus.TakeStunForceMulti *
                                     battleUnitStatus.GetStunForceMultiFormBuff();
            var makeStunTickMulti = stunFixStatus.MakeStunTickMulti * fixStatus.TakeStunTickMulti;
            var max = MathTools.Max(GenUp(height, f), upSpeed);
            var twoDVector = !canFix ? unit.Multi(speed) : unit.Multi(speed * makeStunForceMulti);
            var u = !canFix ? TickLast : MathTools.Max(1, (uint)(TickLast * makeStunTickMulti));
            var pushOnAir = new PushStunOnAir(twoDVector, height.GetValueOrDefault(0), max, u,
                battleUnitStatus);
            return pushOnAir;
        }
    }

    public enum PushType
    {
        Center,
        Vector
    }


    public class PullStunBuffMaker : IStunBuffMaker
    {
        public uint TickLast { get; }
        public float PullMomentum { get; }
        private TwoDVector? PullFixVector { get; }

        public PullStunBuffMaker(uint tickLast, float pullMomentum, TwoDVector? pullFixVector)
        {
            TickLast = tickLast;
            PullMomentum = pullMomentum;
            PullFixVector = pullFixVector;
        }

        public IStunBuff GenBuff(TwoDPoint pos, TwoDPoint obPos, TwoDVector aim, float? height, float upSpeed,
            CharacterStatus whoTake,
            IBattleUnitStatus whoDid, bool canFix = true)
        {
            var mass = CommonConfig.Configs.bodys[whoTake.CharacterBody.GetSize()].mass;

            var twoDPoint = PullFixVector != null ? pos.Move(PullFixVector.AntiClockwiseTurn(aim)) : pos;

            var genVector = obPos.GenVector(twoDPoint);
            var l = genVector.Norm();
            var unit = l <= 0 ? new TwoDVector(0f, 0f) : new TwoDVector(genVector.X / l, genVector.Y / l);
            var genBuffFromUnit =
                GenBuffFromUnit(unit, PullMomentum / mass,
                    CommonConfig.OtherConfig.friction, whoDid, whoTake, l, canFix);
            return genBuffFromUnit;
        }

        private IStunBuff GenBuffFromUnit(TwoDVector unit, float pullMaxSpeed,
            float otherConfigFriction, IBattleUnitStatus whoDid, IBattleUnitStatus whoTake, float length, bool canFix)
        {
            var stunFixStatus = whoDid.GetStunFixStatus();
            var fixStatus = whoTake.GetStunFixStatus();
            var stunForceMultiFormBuff = whoDid.GetStunForceMultiFormBuff();
            var makeStunForceMulti =
                stunFixStatus.MakeStunForceMulti * fixStatus.TakeStunForceMulti * stunForceMultiFormBuff;
            var makeStunTickMulti = stunFixStatus.MakeStunTickMulti * fixStatus.TakeStunTickMulti;
            var u = canFix ? MathTools.Max(1, (uint)MathTools.Round(makeStunTickMulti * TickLast)) : TickLast;
            var vector1 = unit.Multi(pullMaxSpeed > 0 ? otherConfigFriction : -otherConfigFriction);

            var sq = 2f * otherConfigFriction * length;
            if (sq > pullMaxSpeed * pullMaxSpeed)
            {
                var twoDVector = canFix ? unit.Multi(pullMaxSpeed * makeStunForceMulti) : unit.Multi(pullMaxSpeed);
                var pushOnEarth = new PushStunOnEarth(twoDVector, vector1, u, whoDid);
                return pushOnEarth;
            }
            else
            {
                var sqrt = MathTools.Sqrt(sq);
                var twoDVector = canFix ? unit.Multi(sqrt * makeStunForceMulti) : unit.Multi(sqrt);
                var pushOnEarth = new PushStunOnEarth(twoDVector, vector1, u, whoDid);
                return pushOnEarth;
            }
        }
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


        private IStunBuff GenABuff(TwoDVector aim, IBattleUnitStatus whoDid)
        {
            var twoDPoints = TwoDVectors.Select(twoDVector => twoDVector.AntiClockwiseTurn(aim.GetUnit()))
                .ToList();
#if DEBUG
            Console.Out.WriteLine($" aim {aim}");
            foreach (var twoDVector in TwoDVectors)
            {
                Console.Out.WriteLine($"gen caught by vector:{twoDVector}");
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
            CharacterStatus whoTake, IBattleUnitStatus whoDid, bool canFix = true)
        {
            var antiActBuff = GenABuff(aim, whoDid);
            return antiActBuff;
        }

        public void PickBySomeOne(IBattleUnitStatus characterStatus)
        {
            TrickSkill.PickedBySomeOne(characterStatus);
        }
    }
}