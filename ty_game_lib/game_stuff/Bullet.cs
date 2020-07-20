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

    public class Bullet : IHitStuff
    {
        public bool IsActive { get; set; }
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus Caster { get; }
        public readonly IAntiActBuffConfig SuccessAntiActBuffConfigToOpponent;
        public readonly Skill FromSkill;
        public readonly Dictionary<BodySize, IAntiActBuffConfig> FailActBuffConfigToSelf;

        public readonly int PauseToCaster;
        public readonly int PauseToOpponent;
        public Damage Damage;
        public int Tough;
        public readonly ObjType TargetType;

        public int RestTick;
        public int ResId;


        // public Bullet GenByConfig(game_config.bullet bullet)
        // {
        //     
        // }

        public Bullet(Dictionary<BodySize, BulletBox> sizeToBulletCollision,
            CharacterStatus caster, IAntiActBuffConfig successAntiActBuffConfigToOpponent,
            Dictionary<BodySize, IAntiActBuffConfig> failActBuffConfigToSelf, int pauseToCaster, int pauseToOpponent,
            ObjType targetType, int tough, int restTick, int resId, Skill fromSkill)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
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
            FromSkill = fromSkill;
            IsActive = false;
        }

        public bool CanGoATick()
        {
            RestTick -= 1;
            return RestTick >= 0;
        }

        public bool IsHit(CharacterBody characterBody)
        {
            return GameTools.IsHit(this, characterBody);
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
            var opponentIsStun = opponentCharacterStatusAntiActBuff != null;
            var isActSkill = nowCastSkill != null && nowCastSkill.InWhichPeriod() == Skill.SkillPeriod.Casting;
            var twoDVector = targetCharacterStatus.CharacterBody.Sight.Aim;
            var b4 = twoDVector.Dot(Aim) <= 0; // 是否从背后攻击
            var b2 = isActSkill && objTough.GetValueOrDefault(0) < Tough; //如果对手正在释放技能
            var b3 = !isActSkill && Tough < TempConfig.MidTough; //如果对手不在释放技能

            if (protecting)
            {
                return;
            }

            //AttackOk 攻击成功
            var characterBodyBodySize = targetCharacterStatus.CharacterBody.BodySize;
            if (opponentIsStun || b2 || b3 || b4)
            {
                Caster.PauseTick = PauseToCaster;
                FromSkill.IsHit = true;
                Caster.LockingWho ??= targetCharacterStatus;
                if (targetCharacterStatus.CatchingWho != null)
                    targetCharacterStatus.CatchingWho.AntiActBuff = TempConfig.OutCaught;

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
                                characterBodyBodySize, Caster);
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
                            pushOnAir.UpSpeed, characterBodyBodySize, Caster);
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
                            characterBodyBodySize, Caster);
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
                Caster.NextSkill = null;
                Caster.LockingWho = null;
                if (!FailActBuffConfigToSelf.TryGetValue(characterBodyBodySize, out IAntiActBuffConfig? failAntiBuff))
                {
                    failAntiBuff = null;
                }

                switch (failAntiBuff)
                {
                    case CatchAntiActBuffConfig catchAntiActBuffConfig:
                        targetCharacterStatus.CatchingWho = Caster;
                        targetCharacterStatus.NowCastSkill = catchAntiActBuffConfig.TrickSkill;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(FailActBuffConfigToSelf));
                }

                var antiActBuff = failAntiBuff.GenBuff(targetCharacterStatus.GetPos(), Caster.GetPos(), Aim,
                    null,
                    0, Caster.CharacterBody.BodySize, targetCharacterStatus);
                Caster.AntiActBuff = antiActBuff;
            }
        }

        public HashSet<int> HitTeam(IQSpace qSpace)
        {
            var mapToGidList = qSpace.FilterToGIdPsList((body, bullet) => bullet.HitBody(body),
                this);
            return SomeTools.ListToHashSet(mapToGidList.Select(x => x.GetId()));
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