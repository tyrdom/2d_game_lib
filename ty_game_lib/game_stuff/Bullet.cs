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
        public bool IsActive;
        public TwoDPoint Pos;
        public TwoDVector Aim;
       
        public readonly Dictionary<BodySize, BulletBox> SizeToBulletCollision;
        public readonly CharacterStatus Caster;
        public readonly IAntiActBuffConfig SuccessAntiActBuffConfigToOpponent;
        public readonly IAntiActBuffConfig FailActBuffConfigToSelf;
        public readonly int PauseToCaster;
        public readonly int PauseToOpponent;
        public Damage Damage;
        public int Tough;
        public readonly ObjType TargetType;

        public int RestTick;
        public int ResId;

        public Bullet(TwoDPoint pos, TwoDVector aim, Dictionary<BodySize, BulletBox> sizeToBulletCollision,
            ref CharacterStatus caster, IAntiActBuffConfig successAntiActBuffConfigToOpponent,
            IAntiActBuffConfig failActBuffConfigToSelf, int pauseToCaster, int pauseToOpponent,
            ObjType targetType, int tough, int restTick, int resId)
        {
            Pos = pos;
            Aim = aim;
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = caster;
            SuccessAntiActBuffConfigToOpponent = successAntiActBuffConfigToOpponent;
            FailActBuffConfigToSelf = failActBuffConfigToSelf;

            PauseToCaster = pauseToCaster;
            PauseToOpponent = pauseToOpponent;
            Damage = new Damage(1);
            TargetType = targetType;
            Tough = tough;
            RestTick = restTick;
            ResId = resId;
         
            IsActive = false;
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

        public bool HitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case CharacterBody characterBody1:
                    var isHit = IsHit(characterBody1);
                    if (isHit)
                    {
                        HitOne(characterBody1.CharacterStatus);
                    }

                    return isHit;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetBody));
            }
        }

        public void HitOne(CharacterStatus targetCharacterStatus)
        {
            var protecting = targetCharacterStatus.ProtectTick > 0;
            var nowCastSkill = targetCharacterStatus.NowCastSkill;
            var objTough = nowCastSkill?.NowTough;
            var opponentCharacterStatusAntiActBuff = targetCharacterStatus.AntiActBuff;
            var isStun = opponentCharacterStatusAntiActBuff != null;
            var isActSkill = nowCastSkill != null || nowCastSkill?.InWhichPeriod() != Skill.SkillPeriod.Casting;
            var twoDVector = targetCharacterStatus.CharacterBody.Sight.Aim;
            var b4 = twoDVector.Dot(Aim) <= 0;
            var b2 = isActSkill && objTough.GetValueOrDefault(0) < Tough;
            var b3 = !isActSkill && Tough < TempConfig.MidTough;

            if (protecting)
            {
                return;
            }

            //AttackOk
            if (isStun || b2 || b3 || b4)
            {
                Caster.PauseTick = PauseToCaster;
                Caster.LockingWho ??= targetCharacterStatus;
                if (targetCharacterStatus.CatchingWho != null)
                    targetCharacterStatus.CatchingWho.AntiActBuff = TempConfig.OutCought;
                // characterStatus.Combo.Reset();
                targetCharacterStatus.DamageHealStatus.TakeDamage(Damage);

                switch (opponentCharacterStatusAntiActBuff)
                {
                    case null:
                        if (PauseToOpponent > targetCharacterStatus.PauseTick)
                        {
                            targetCharacterStatus.PauseTick = PauseToOpponent;
                        }

                        break;
                    case Caught _:
                        if (Caster.CatchingWho == targetCharacterStatus)
                        {
                            if (PauseToOpponent > targetCharacterStatus.PauseTick)
                            {
                                targetCharacterStatus.PauseTick = PauseToOpponent;
                            }

                            var genBuff1 = SuccessAntiActBuffConfigToOpponent.GenBuff(Pos,
                                targetCharacterStatus.GetPos(),
                                Aim,
                                null, 0,
                                targetCharacterStatus.CharacterBody.BodySize, Caster);
                            targetCharacterStatus.AntiActBuff = genBuff1;
                        }

                        break;
                    case PushOnAir pushOnAir:
                        if (PauseToOpponent > targetCharacterStatus.PauseTick)
                        {
                            targetCharacterStatus.PauseTick = PauseToOpponent;
                        }

                        var height = pushOnAir.Height;

                        var antiActBuff = SuccessAntiActBuffConfigToOpponent.GenBuff(Pos,
                            targetCharacterStatus.GetPos(),
                            Aim,
                            height,
                            pushOnAir.UpSpeed, targetCharacterStatus.CharacterBody.BodySize, Caster);
                        targetCharacterStatus.AntiActBuff = antiActBuff;
                        break;
                    case PushOnEarth _:

                        if (PauseToOpponent > targetCharacterStatus.PauseTick)
                        {
                            targetCharacterStatus.PauseTick = PauseToOpponent;
                        }

                        var genBuff = SuccessAntiActBuffConfigToOpponent.GenBuff(Pos, targetCharacterStatus.GetPos(),
                            Aim,
                            null, 0,
                            targetCharacterStatus.CharacterBody.BodySize, Caster);
                        targetCharacterStatus.AntiActBuff = genBuff;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(opponentCharacterStatusAntiActBuff));
                }
            }
            else
            {
                //AttackFail
                Caster.NowCastSkill = null;
                Caster.LockingWho = null;
                switch (FailActBuffConfigToSelf)
                {
                    case CatchAntiActBuffConfig catchAntiActBuffConfig:
                        targetCharacterStatus.CatchingWho = Caster;
                        targetCharacterStatus.NowCastSkill = catchAntiActBuffConfig.TrickSkill;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(FailActBuffConfigToSelf));
                }

                var antiActBuff = FailActBuffConfigToSelf.GenBuff(targetCharacterStatus.GetPos(), Caster.GetPos(), Aim,
                    null,
                    0, Caster.CharacterBody.BodySize, targetCharacterStatus);
                Caster.AntiActBuff = antiActBuff;
            }
        }

        public HashSet<int> HitTeam(IQSpace qSpace)
        {
            var mapToGidList = qSpace.FilterToGIdPsList((body, bullet) => bullet.HitBody(body),
                this);
            return mapToGidList.Select(x => x.GetId()).ToHashSet();
        }

        public BulletMsg GenMsg()
        {
            return new BulletMsg(Pos, Aim, ResId, Caster.GetPos());
        }

        public Bullet? ActiveBullet(TwoDPoint casterPos, TwoDVector casterAim)
        {
            IsActive = true;
            Pos = casterPos;
            Aim = casterAim;
            return this;
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