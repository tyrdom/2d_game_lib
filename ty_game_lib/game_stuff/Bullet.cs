using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public enum ObjType
    {
        Self,
        OtherTeam,
        SameTeam
    }

    public class Bullet
    {
        public TwoDPoint Pos;
        public TwoDVector Aim;
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision;
        public CharacterStatus Caster;
        public IAntiActBuffConfig SuccessAntiActBuffConfigToOpponent;
        public IAntiActBuffConfig FailActBuffConfigToSelf;
        public int PauseToCaster;
        public int PauseToOpponent;
        public Damage Damage;
        public DamageBuffConfig[] DamageBuffConfigs;
        public int Tough;
        public ObjType ObjType;

        public BType BType;

        public Bullet(TwoDPoint pos, TwoDVector aim, Dictionary<BodySize, BulletBox> sizeToBulletCollision,
            ref CharacterStatus caster, IAntiActBuffConfig successAntiActBuffConfigToOpponent,
            IAntiActBuffConfig failActBuffConfigToSelf, int pauseToCaster, int pauseToOpponent,
            DamageBuffConfig[] damageBuffConfigs, ObjType objType, int tough, BType bType)
        {
            Pos = pos;
            Aim = aim;
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = caster;
            SuccessAntiActBuffConfigToOpponent = successAntiActBuffConfigToOpponent;
            FailActBuffConfigToSelf = failActBuffConfigToSelf;

            PauseToCaster = pauseToCaster;
            PauseToOpponent = pauseToOpponent;

            DamageBuffConfigs = damageBuffConfigs;
            ObjType = objType;
            Tough = tough;
            BType = bType;
        }

        public bool IsHit(CharacterBody characterBody)
        {
            var characterBodyBodySize = characterBody.BodySize;
            return SizeToBulletCollision.TryGetValue(characterBodyBodySize, out var bulletBox) &&
                   bulletBox.IsHit(characterBody.NowPos, Pos, Aim);
        }

        public void HitOne(ref CharacterStatus characterStatus)
        {
            var protecting = characterStatus.ProtectTick > 0;

            var objTough = characterStatus.NowTough;
            var opponentCharacterStatusAntiActBuff = characterStatus.AntiActBuff;
            var isStun = opponentCharacterStatusAntiActBuff != null;
            var isActSkill = characterStatus.NowCast != null;

            var b2 = isActSkill && objTough < Tough;
            var b3 = !isActSkill && Tough < TempConfig.MidTough;

            if (protecting)
            {
                return;
            }


            //AttackOk
            if (isStun || b2 || b3)
            {
                Caster.PauseTick = PauseToCaster;
                characterStatus.DamageHealAbout.TakeDamage(Damage);
                switch (opponentCharacterStatusAntiActBuff)
                {
                    case null:
                        if (PauseToOpponent > characterStatus.PauseTick)
                        {
                            characterStatus.PauseTick = PauseToOpponent;
                        }

                        break;
                    case Caught _:
                        if (Caster.Catching == characterStatus)
                        {
                            if (PauseToOpponent > characterStatus.PauseTick)
                            {
                                characterStatus.PauseTick = PauseToOpponent;
                            }

                            var genBuff1 = SuccessAntiActBuffConfigToOpponent.GenBuff(Pos, characterStatus.GetPos(),
                                Aim,
                                null, 0,
                                characterStatus.CharacterBody.BodySize,ref Caster);
                            characterStatus.AntiActBuff = genBuff1;
                        }

                        break;
                    case PushOnAir pushOnAir:
                        if (PauseToOpponent > characterStatus.PauseTick)
                        {
                            characterStatus.PauseTick = PauseToOpponent;
                        }

                        var height = pushOnAir.Height;

                        var antiActBuff = SuccessAntiActBuffConfigToOpponent.GenBuff(Pos, characterStatus.GetPos(), Aim,
                            height,
                            pushOnAir.UpSpeed, characterStatus.CharacterBody.BodySize,ref Caster);
                        characterStatus.AntiActBuff = antiActBuff;
                        break;
                    case PushOnEarth _:

                        if (PauseToOpponent > characterStatus.PauseTick)
                        {
                            characterStatus.PauseTick = PauseToOpponent;
                        }

                        var genBuff = SuccessAntiActBuffConfigToOpponent.GenBuff(Pos, characterStatus.GetPos(), Aim,
                            null, 0,
                            characterStatus.CharacterBody.BodySize,ref Caster);
                        characterStatus.AntiActBuff = genBuff;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(opponentCharacterStatusAntiActBuff));
                }
            }
            else
            {
                //AttackFail
                characterStatus.Catching = FailActBuffConfigToSelf switch
                {
                    CatchAntiActBuffConfig _ => Caster,
                    _ => throw new ArgumentOutOfRangeException(nameof(FailActBuffConfigToSelf))
                };

                var antiActBuff = FailActBuffConfigToSelf.GenBuff(characterStatus.GetPos(), Caster.GetPos(), Aim, null,
                    0, Caster.CharacterBody.BodySize,ref characterStatus);
                Caster.AntiActBuff = antiActBuff;
            }
        }

        public void HitTeam(QSpace qSpace)
        {
        }
    }

    public class Damage
    {
        public int DamageValue;
    }
}