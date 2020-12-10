using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public enum ObjType
    {
        OtherTeam,
        SameTeam,
        AllTeam
    }

    public class Bullet : IHitMedia
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public Zone RdZone { get; }

        private int AmmoAddWhenSuccess { get; }

        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public IBattleUnitStatus? Caster { get; set; }
        private Dictionary<BodySize, IStunBuffConfig> SuccessStunBuffConfigToOpponent { get; }
        public Dictionary<BodySize, IStunBuffConfig> FailActBuffConfigToSelf { get; }

        private int PauseToCaster { get; }
        private int PauseToOpponent { get; }
        private float DamageMulti { get; }
        private int Tough { get; }
        public ObjType TargetType { get; }

        private hit_type HitType { get; }
        private int RestTick { get; set; }
        private int ResId { get; }

        private int ProtectValueAdd { get; }

        public static Bullet GenByBulletId(string id)
        {
            if (TempConfig.Configs.bullets.TryGetValue(id, out var bullet))
            {
                return GenByConfig(bullet);
            }

            throw new ArgumentOutOfRangeException();
        }

        public void Sign(CharacterStatus characterStatus)
        {
            Caster = characterStatus;

            foreach (var antiActBuffConfig in SuccessStunBuffConfigToOpponent)
            {
                if (antiActBuffConfig.Value is CatchStunBuffConfig catchAntiActBuffConfig)
                {
                    catchAntiActBuffConfig
                        .PickBySomeOne(characterStatus);
                }
            }
        }

        public static Bullet GenByConfig(bullet bullet, uint pairKey = 0)
        {
            var dictionary = GameTools.GenBulletShapes(bullet.ShapeParams, bullet.LocalRotate, bullet.LocalPos,
                bullet.ShapeType);


            var bulletSuccessAntiActBuffConfigToOpponent = bullet.SuccessAntiActBuffConfigToOpponent;


            var antiActBuffConfig = GAntiActBuffConfigs(
                bulletSuccessAntiActBuffConfigToOpponent);

            var bulletFailActBuffConfigToSelf = bullet.FailActBuffConfigToSelf;

            var antiActBuffConfigs = GAntiActBuffConfigs(bulletFailActBuffConfigToSelf);

            static IStunBuffConfig GenBuffByC(buff_type? buffConfigToOpponentType, string? configToOpponent) =>
                buffConfigToOpponentType switch
                {
                    buff_type.push_buff => GameTools.GenBuffByConfig(TempConfig.Configs.push_buffs![configToOpponent]),
                    buff_type.caught_buff => GameTools.GenBuffByConfig(
                        TempConfig.Configs.caught_buffs![configToOpponent]),
                    _ => throw new ArgumentOutOfRangeException()
                };


            static Dictionary<BodySize, IStunBuffConfig>
                GAntiActBuffConfigs(IEnumerable<Buff> bulletFailActBuffConfigToSelf)
            {
                var actBuffConfigs = TempConfig.SizeToR.ToDictionary(
                    pair => pair.Key, pair =>
                    {
                        switch (pair.Key)
                        {
                            case BodySize.Small:
                                var firstOrDefault = bulletFailActBuffConfigToSelf.FirstOrDefault(x =>
                                    x.size == size.small || x.size == size.@default);
                                return GenBuffByC(firstOrDefault?.buff_type, firstOrDefault?.buff_id);
                            case BodySize.Medium:
                                var firstOrDefault2 = bulletFailActBuffConfigToSelf.FirstOrDefault(x =>
                                    x.size == size.medium || x.size == size.@default);
                                return GenBuffByC(firstOrDefault2?.buff_type, firstOrDefault2?.buff_id);
                            case BodySize.Big:
                                var firstOrDefault3 = bulletFailActBuffConfigToSelf.FirstOrDefault(x =>
                                    x.size == size.big || x.size == size.@default);
                                return GenBuffByC(firstOrDefault3?.buff_type, firstOrDefault3?.buff_id);
                            case BodySize.Tiny:
                                var firstOrDefault4 = bulletFailActBuffConfigToSelf.FirstOrDefault(x =>
                                    x.size == size.tiny || x.size == size.@default);
                                return GenBuffByC(firstOrDefault4?.buff_type, firstOrDefault4?.buff_id);

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
                return actBuffConfigs;
            }

            var objType = bullet.TargetType switch
            {
                target_type.other_team => ObjType.OtherTeam,
                _ => throw new ArgumentOutOfRangeException()
            };
            var tough = bullet.Tough;
            if (bullet.Tough == -1)
            {
                tough = (int) (pairKey * TempConfig.ToughGrowPerTick);
            }

            return new Bullet(dictionary, antiActBuffConfig, antiActBuffConfigs, bullet.PauseToCaster,
                bullet.PauseToOpponent, objType, tough, bullet.SuccessAmmoAdd, bullet.DamageMulti, bullet.ProtectValue,
                bullet.HitType);
        }

        private Bullet(Dictionary<BodySize, BulletBox> sizeToBulletCollision,
            Dictionary<BodySize, IStunBuffConfig> successStunBuffConfigToOpponent,
            Dictionary<BodySize, IStunBuffConfig> failActBuffConfigToSelf, int pauseToCaster, int pauseToOpponent,
            ObjType targetType, int tough, int ammoAddWhenSuccess, float damageMulti, int protectValueAdd,
            hit_type hitType)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = null;
            SuccessStunBuffConfigToOpponent = successStunBuffConfigToOpponent;
            FailActBuffConfigToSelf = failActBuffConfigToSelf;
            PauseToCaster = pauseToCaster;
            PauseToOpponent = pauseToOpponent;

            TargetType = targetType;
            Tough = tough;
            RestTick = 1;
            ResId = 1;
            AmmoAddWhenSuccess = ammoAddWhenSuccess;
            DamageMulti = damageMulti;
            ProtectValueAdd = protectValueAdd;
            HitType = hitType;
            RdZone = GameTools.GenRdBox(sizeToBulletCollision);
        }

        public bool CanGoNextTick()
        {
            RestTick -= 1;
            return RestTick > 0;
        }

        public bool IsHit(ICanBeHit characterBody)
        {
            return GameTools.IsHit(this, characterBody);
        }

        public bool IsHitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case CharacterBody targetCharacterBody:
                    var isHit = IsHit(targetCharacterBody);
                    if (isHit)
                    {
                        HitOne(targetCharacterBody.CharacterStatus);
                    }
#if DEBUG
                    Console.Out.WriteLine($"bullet hit::{isHit}");
#endif
                    return isHit;
                case Trap trap:
                    var isHitBody = IsHit(trap);
                    if (isHitBody)
                    {
                        HitOne(trap);
                    }


                    return isHitBody;


                default:
                    throw new ArgumentOutOfRangeException(nameof(targetBody));
            }
        }

        private void HitOne(Trap targetTrap)
        {
            switch (Caster)
            {
                case null: throw new Exception("there is a no Caster Bullet");
                case CharacterStatus characterStatusCaster:
                    HitOne(targetTrap, characterStatusCaster);
                    break;
                case Trap trapCaster:
                    HitOne(targetTrap, trapCaster);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Caster));
            }
        }


        private void HitOne(Trap targetTrap, IBattleUnitStatus caster)
        {
            //目标为trap 直接成功攻击
            caster.BaseBulletAtkOk(PauseToCaster, AmmoAddWhenSuccess, targetTrap);

            targetTrap.TakeDamage(caster.GenDamage(DamageMulti, true));
        }

        private void HitOne(CharacterStatus targetCharacterStatus)
        {
            switch (Caster)
            {
                case null: throw new Exception("there is a no Caster Bullet");
                case CharacterStatus characterStatusCaster:

                    HitOne(targetCharacterStatus, characterStatusCaster);
                    break;
                case Trap trapCaster:
                    HitOne(targetCharacterStatus, trapCaster);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Caster));
            }
        }

        private (HitCond hitCond, bool b4, IStunBuff? opponentCharacterStatusAntiActBuff, bool isActSkill) IsAtkPass(
            CharacterStatus targetCharacterStatus)
        {
            var nowCastSkill = targetCharacterStatus.NowCastAct;
            var objTough = nowCastSkill?.NowTough;
            var opponentCharacterStatusAntiActBuff = targetCharacterStatus.StunBuff;
            var opponentIsStun = opponentCharacterStatusAntiActBuff != null;
            var isActSkill = nowCastSkill != null && nowCastSkill.InWhichPeriod() == SkillPeriod.Casting;
            var twoDVector = targetCharacterStatus.CharacterBody.Sight.Aim;
            var b4 = twoDVector.Dot(Aim) >= 0; // 是否从背后攻击
            var b3 = !isActSkill && Tough < TempConfig.MidTough; //如果对手不在释放技能，攻击坚韧小于中值，攻击成功
            var tough = objTough.GetValueOrDefault(0);
            var b2 = isActSkill && tough < Tough; //如果对手正在释放技能 ，对手坚韧小于攻击坚韧，则成功
            var atkOk = opponentIsStun || b2 || b3 || b4;
            if (atkOk)
            {
                return (HitCond.Ok, b4, opponentCharacterStatusAntiActBuff, isActSkill);
            }

            var b5 = isActSkill && tough == Tough;
            if (b5)
            {
                return (HitCond.Draw, b4, opponentCharacterStatusAntiActBuff, isActSkill);
            }

#if DEBUG
            Console.Out.WriteLine(
                $"attack ~~~from back:: {b4} cast over:: {b2}  not back  ::{b3}   target is cast{isActSkill} now tough::{Tough},mid::{TempConfig.MidTough}");
#endif
            return (HitCond.Fail, b4, opponentCharacterStatusAntiActBuff, isActSkill);
        }


        private void HitOne(CharacterStatus targetCharacterStatus, IBattleUnitStatus caster)
        {
            var protecting = targetCharacterStatus.NowProtectTick > 0;
            if (protecting)
            {
                return;
            }

            var (atkOk, back, opponentCharacterStatusAntiActBuff, isActSkill) = IsAtkPass(targetCharacterStatus);

            var targetCharacterBodyBodySize = targetCharacterStatus.CharacterBody.GetSize();
            //AttackOk 攻击成功
            switch (atkOk)
            {
                case HitCond.Ok:
                    HitOk();
                    break;
                case HitCond.Fail:
                    HitFail();
                    break;
                case HitCond.Draw:
                    HitDraw();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            void HitOk()
            {
                //基本方面
                // 释放方基本状态
                caster.BaseBulletAtkOk(PauseToCaster, AmmoAddWhenSuccess, targetCharacterStatus);

                // 被击中方基本状态改变 包括伤害
                targetCharacterStatus.BaseBeHitByBulletChange(Pos, ProtectValueAdd, caster, DamageMulti, back);

                //stun buff方面
                var antiActBuffConfig = SuccessStunBuffConfigToOpponent[targetCharacterBodyBodySize];

                void InitBuff()
                {
                    targetCharacterStatus.PauseTick = Math.Max(PauseToOpponent, targetCharacterStatus.PauseTick);

                    targetCharacterStatus.StunBuff = antiActBuffConfig
                        .GenBuff(Pos,
                            targetCharacterStatus.GetPos(),
                            Aim,
                            null, 0,
                            targetCharacterBodyBodySize, caster);
                    //初始化buff时，如果是抓取技能，会触发技能
                    switch (antiActBuffConfig)
                    {
                        case CatchStunBuffConfig catchAntiActBuffConfig:
                            caster.LoadCatchTrickSkill(null, catchAntiActBuffConfig);

                            break;
                    }
                }

                // 对手动作状态buff刷新
                switch (opponentCharacterStatusAntiActBuff)
                {
                    case null:
                        InitBuff();
                        break;
                    //对手被其他人抓取时，不在添加停顿帧和改变其buff 除非是自己抓的目标
                    case Caught _:
                        if (caster.CatchingWho == targetCharacterStatus)
                        {
                            InitBuff();
                        }

                        break;
                    // 其他情况刷新buff
                    case PushOnAir pushOnAir:
                        if (PauseToOpponent > targetCharacterStatus.PauseTick)
                        {
                            targetCharacterStatus.PauseTick = PauseToOpponent;
                        }

                        var height = pushOnAir.Height;
                        var antiActBuff = antiActBuffConfig.GenBuff(Pos,
                            targetCharacterStatus.GetPos(),
                            Aim,
                            height,
                            pushOnAir.UpSpeed, targetCharacterBodyBodySize, caster);
                        targetCharacterStatus.StunBuff = antiActBuff;
                        break;
                    case PushOnEarth _:

                        InitBuff();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(opponentCharacterStatusAntiActBuff));
                }
            }

            void HitFail()
            {
                //AttackFail 
                switch (caster)
                {
                    case CharacterStatus characterStatus:
                        switch (HitType)
                        {
                            case hit_type.range:
                                
                                targetCharacterStatus.AbsorbRangeBullet(Pos,ProtectValueAdd,characterStatus,DamageMulti,back);
                                break;
                            case hit_type.melee:
                                CharAtkFail(characterStatus, targetCharacterStatus, isActSkill, targetCharacterBodyBodySize);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        CharAtkFail(characterStatus, targetCharacterStatus, isActSkill, targetCharacterBodyBodySize);
                        break;
                    case Trap trap:
                        trap.FailAtk();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(caster));
                }
            }

            void HitDraw()
            {
                //Draw
                switch (caster)
                {
                    case CharacterStatus characterStatus:
                        CharAtkDraw(characterStatus, targetCharacterStatus);
                        break;
                    case Trap trap:
                        trap.FailAtk();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(caster));
                }
            }
        }

        private void CharAtkDraw(CharacterStatus bodyCaster, CharacterStatus targetCharacterStatus)
        {
            bodyCaster.ResetSnipe();
            bodyCaster.ResetCastAct();
            targetCharacterStatus.ResetSnipe();
            targetCharacterStatus.ResetCastAct();

            var antiActBuff = TempConfig.CommonBuffConfig.GenBuff(targetCharacterStatus.GetPos(),
                bodyCaster.GetPos(), Aim,
                null, 0, bodyCaster.CharacterBody.GetSize(), targetCharacterStatus);
            bodyCaster.StunBuff = antiActBuff;
            var antiActBuff2 = TempConfig.CommonBuffConfig.GenBuff(bodyCaster.GetPos(),
                targetCharacterStatus.GetPos(), Aim,
                null, 0, targetCharacterStatus.CharacterBody.GetSize(), bodyCaster);
            targetCharacterStatus.StunBuff = antiActBuff2;
        }


        private void CharAtkFail(CharacterStatus bodyCaster, CharacterStatus targetCharacterStatus, bool isActSkill,
            BodySize targetCharacterBodyBodySize)
        {
            //清除技能数据
            bodyCaster.ResetSnipe();
            bodyCaster.ResetCastAct();
            //生成击中受击消息数据缓存
            bodyCaster.IsBeHitBySomeOne =
                TwoDVector.TwoDVectorByPt(bodyCaster.GetPos(), targetCharacterStatus.GetPos());

            targetCharacterStatus.IsHitSome = true;

            if (!isActSkill) // 说明目标不在攻击状态 通过特定配置读取
            {
                if (!FailActBuffConfigToSelf.TryGetValue(targetCharacterBodyBodySize, out var failAntiBuff)) return;
                var aim = TwoDVector.TwoDVectorByPt(targetCharacterStatus.GetPos(), Pos).GetUnit2();

                //如果为抓取技能，会马上装载抓取buff附带的触发技能
                switch (failAntiBuff)
                {
                    case CatchStunBuffConfig catchAntiActBuffConfig:
                        targetCharacterStatus.CatchingWho = bodyCaster;
                        targetCharacterStatus.LoadCatchTrickSkill(aim, catchAntiActBuffConfig);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(FailActBuffConfigToSelf));
                }

                var antiActBuff = failAntiBuff.GenBuff(targetCharacterStatus.GetPos(), bodyCaster.GetPos(),
                    targetCharacterStatus.GetAim(),
                    null,
                    0, bodyCaster.CharacterBody.GetSize(), targetCharacterStatus);
                bodyCaster.StunBuff = antiActBuff;
            }
            else //对手在攻击状态，通过通用失败buff读取
            {
#if DEBUG
                Console.Out.WriteLine($"Gen Common Back");
#endif
                var antiActBuff = TempConfig.CommonBuffConfig.GenBuff(targetCharacterStatus.GetPos(),
                    bodyCaster.GetPos(), TwoDVector.Zero(),
                    null, 0, bodyCaster.CharacterBody.GetSize(), targetCharacterStatus);
                bodyCaster.StunBuff = antiActBuff;
            }
        }

        public HashSet<int> HitTeam(IQSpace qSpace)
        {
            return HitAbleMediaStandard.HitTeam(qSpace, this);
        }

        public BulletMsg GenMsg()
        {
            return new BulletMsg(Pos, Aim, ResId, Caster?.GetPos());
        }

        public IPosMedia Active(TwoDPoint casterPos, TwoDVector casterAim)
        {
            return
                PosMediaStandard.Active(casterPos, casterAim, this);
        }
    }

    enum HitCond
    {
        Ok,
        Fail,
        Draw
    }
}