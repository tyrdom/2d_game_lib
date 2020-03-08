using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public enum ObjType
    {
        OtherTeam,
        SameTeam,
        AllTeam
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

        public int RestTick;
        public int ResId;

        public Bullet(TwoDPoint pos, TwoDVector aim, Dictionary<BodySize, BulletBox> sizeToBulletCollision,
            ref CharacterStatus caster, IAntiActBuffConfig successAntiActBuffConfigToOpponent,
            IAntiActBuffConfig failActBuffConfigToSelf, int pauseToCaster, int pauseToOpponent,
            DamageBuffConfig[] damageBuffConfigs, ObjType objType, int tough, int restTick, int resId)
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
            RestTick = restTick;
            ResId = resId;
        }

        public bool CanGoATick()
        {
            RestTick -= 1;
            return RestTick >= 0;
        }

        public bool IsHit(CharacterBody characterBody)
        {
            var characterBodyBodySize = characterBody.BodySize;
            return SizeToBulletCollision.TryGetValue(characterBodyBodySize, out var bulletBox) &&
                   bulletBox.IsHit(characterBody.NowPos, Pos, Aim);
        }

        public bool HitBody(IIdPointShape characterBody)
        {
            switch (characterBody)
            {
                case CharacterBody characterBody1:
                    if (IsHit(characterBody1))
                    {
                        HitOne(ref characterBody1.CharacterStatus);
                    }

                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(characterBody));
            }

            return false;
        }

        public void HitOne(ref CharacterStatus characterStatus)
        {
            var protecting = characterStatus.ProtectTick > 0;

            var objTough = characterStatus.NowTough;
            var opponentCharacterStatusAntiActBuff = characterStatus.AntiActBuff;
            var isStun = opponentCharacterStatusAntiActBuff != null;
            var isActSkill = characterStatus.NowCastSkill != null;
            var twoDVector = characterStatus.CharacterBody.Sight.Aim;
            var b4 = twoDVector.Dot(Aim) <= 0;
            var b2 = isActSkill && objTough < Tough;
            var b3 = !isActSkill && Tough < TempConfig.MidTough;

            if (protecting)
            {
                return;
            }


            //AttackOk
            if (isStun || b2 || b3 || b4)
            {
                Caster.PauseTick = PauseToCaster;
                Caster.WhoLocks ??= characterStatus;
                characterStatus.Catching.AntiActBuff = TempConfig.OutCought;
                characterStatus.Combo.Reset();
                characterStatus.DamageHealStatus.TakeDamage(Damage);

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
                                characterStatus.CharacterBody.BodySize, ref Caster);
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
                            pushOnAir.UpSpeed, characterStatus.CharacterBody.BodySize, ref Caster);
                        characterStatus.AntiActBuff = antiActBuff;
                        break;
                    case PushOnEarth _:

                        if (PauseToOpponent > characterStatus.PauseTick)
                        {
                            characterStatus.PauseTick = PauseToOpponent;
                        }

                        var genBuff = SuccessAntiActBuffConfigToOpponent.GenBuff(Pos, characterStatus.GetPos(), Aim,
                            null, 0,
                            characterStatus.CharacterBody.BodySize, ref Caster);
                        characterStatus.AntiActBuff = genBuff;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(opponentCharacterStatusAntiActBuff));
                }
            }
            else
            {
                //AttackFail
                Caster.Combo.Reset();
                Caster.WhoLocks = null;
                switch (FailActBuffConfigToSelf)
                {
                    case CatchAntiActBuffConfig catchAntiActBuffConfig:
                        characterStatus.Catching = Caster;
                        characterStatus.NowCastSkill = catchAntiActBuffConfig.TrickSkill;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(FailActBuffConfigToSelf));
                }

                var antiActBuff = FailActBuffConfigToSelf.GenBuff(characterStatus.GetPos(), Caster.GetPos(), Aim, null,
                    0, Caster.CharacterBody.BodySize, ref characterStatus);
                Caster.AntiActBuff = antiActBuff;
            }
        }

        public HashSet<int> HitTeam(QSpace qSpace)
        {
            var mapToGidList = qSpace.FilterToGIdPsList((body, bullet) => bullet.HitBody(body),
                this);
            return mapToGidList.Select(x => x.GetId()).ToHashSet();
        }

        public BulletMsg GenMsg()
        {
            return new BulletMsg(Pos,Aim,ResId,Caster.GetPos());
        }
    }

    public class Damage
    {
        public int DamageValue;

        public Damage(int damageValue)
        {
            DamageValue = damageValue;
        }
    }
}