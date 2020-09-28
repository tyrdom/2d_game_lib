using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public enum ObjType
    {
        OnlyMyself,
        OtherTeam,
        SameTeam,
        AllTeam
    }

    public class Bullet : IHitStuff
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public Zone RdZone { get; }

        public int AmmoAddWhenSuccess { get; }


        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus? Caster { get; set; }
        private Dictionary<BodySize, IAntiActBuffConfig> SuccessAntiActBuffConfigToOpponent { get; }
        public Dictionary<BodySize, IAntiActBuffConfig> FailActBuffConfigToSelf { get; }

        private int PauseToCaster { get; }
        private int PauseToOpponent { get; }
        private float DamageMulti { get; }
        private int Tough { get; }
        public ObjType TargetType { get; }

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

        public void PickedBySomeOne(CharacterStatus characterStatus)
        {
            Caster = characterStatus;
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

            static IAntiActBuffConfig GenBuffByC(buff_type? buffConfigToOpponentType, string? configToOpponent) =>
                buffConfigToOpponentType switch
                {
                    buff_type.push_buff => GameTools.GenBuffByConfig(TempConfig.Configs.push_buffs![configToOpponent]),
                    buff_type.caught_buff => GameTools.GenBuffByConfig(
                        TempConfig.Configs.caught_buffs![configToOpponent]),
                    _ => throw new ArgumentOutOfRangeException()
                };


            static Dictionary<BodySize, IAntiActBuffConfig>
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
                bullet.PauseToOpponent, objType, tough, bullet.SuccessAmmoAdd, bullet.DamageMulti, bullet.ProtectValue);
        }

        private Bullet(Dictionary<BodySize, BulletBox> sizeToBulletCollision,
            Dictionary<BodySize, IAntiActBuffConfig> successAntiActBuffConfigToOpponent,
            Dictionary<BodySize, IAntiActBuffConfig> failActBuffConfigToSelf, int pauseToCaster, int pauseToOpponent,
            ObjType targetType, int tough, int ammoAddWhenSuccess, float damageMulti, int protectValueAdd)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = null;
            SuccessAntiActBuffConfigToOpponent = successAntiActBuffConfigToOpponent;
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
            RdZone = GameTools.GenRdBox(sizeToBulletCollision);
        }

        public bool CanGoNextTick()
        {
            RestTick -= 1;
            return RestTick > 0;
        }

        public bool IsHit(CharacterBody characterBody)
        {
            return GameTools.IsHit(this, characterBody);
        }

        private bool IsHitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case CharacterBody characterBody1:
                    var isHit = Caster != null && IsHit(characterBody1);
                    if (isHit)
                    {
                        HitOne(characterBody1.CharacterStatus);
                    }

// #if DEBUG
//                     Console.Out.WriteLine($"bullet hit::{isHit}");
// #endif
                    return isHit;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetBody));
            }
        }

        private void HitOne(CharacterStatus targetCharacterStatus)
        {
            if (Caster == null)
            {
                throw new Exception("there is a no Caster Bullet");
            }

            var protecting = targetCharacterStatus.NowProtectTick > 0;
            var nowCastSkill = targetCharacterStatus.NowCastAct;
            var objTough = nowCastSkill?.NowTough;
            var opponentCharacterStatusAntiActBuff = targetCharacterStatus.AntiActBuff;
            var opponentIsStun = opponentCharacterStatusAntiActBuff != null;
            var isActSkill = nowCastSkill != null && nowCastSkill.InWhichPeriod() == SkillPeriod.Casting;
            var twoDVector = targetCharacterStatus.CharacterBody.Sight.Aim;
            var b4 = twoDVector.Dot(Aim) >= 0; // 是否从背后攻击
            var b2 = isActSkill && objTough.GetValueOrDefault(0) < Tough; //如果对手正在释放技能 ，对手坚韧小于攻击坚韧，则成功
            var b3 = !isActSkill && Tough < TempConfig.MidTough; //如果对手不在释放技能，攻击坚韧小于中值，攻击成功
#if DEBUG
            Console.Out.WriteLine(
                $"attack ~~~from back:: {b4} cast over:: {b2}  not back  ::{b3}   target is cast{isActSkill} now tough::{Tough},mid::{TempConfig.MidTough}");
#endif
            if (protecting)
            {
                return;
            }

            var targetCharacterBodyBodySize = targetCharacterStatus.CharacterBody.BodySize;
            //AttackOk 攻击成功
            if (opponentIsStun || b2 || b3 || b4)
            {
                // 目标速度瞄准重置
                targetCharacterStatus.ResetSpeed();
                targetCharacterStatus.ResetSnipe();
                targetCharacterStatus.ResetCastAct();
                // 我方按配置添加攻击停帧,攻击产生效果
                Caster.PauseTick = PauseToCaster;
                Caster.AddAmmo(AmmoAddWhenSuccess);
                //如果没有锁定目标，则锁定当前命中的目标
                Caster.LockingWho ??= targetCharacterStatus;
                //生成击中受击消息数据缓存

                targetCharacterStatus.IsBeHitBySomeOne =
                    TwoDVector.TwoDVectorByPt(targetCharacterStatus.GetPos(), Pos);

                Caster.IsHitSome = true;
                //如果对手有抓取对象
                if (targetCharacterStatus.CatchingWho != null)
                {
                    //抓取脱手
                    targetCharacterStatus.CatchingWho.AntiActBuff = TempConfig.OutCaught;
                    targetCharacterStatus.CatchingWho = null;
                }

                //对手承受伤害
                targetCharacterStatus.AddProtect(ProtectValueAdd);
                targetCharacterStatus.DamageHealStatus.TakeDamage(CharacterStatus.GenDamage(DamageMulti));


                var antiActBuffConfig = SuccessAntiActBuffConfigToOpponent[targetCharacterBodyBodySize];

                void InitBuff()
                {
                    if (PauseToOpponent > targetCharacterStatus.PauseTick)
                    {
                        targetCharacterStatus.PauseTick = PauseToOpponent;
                    }


                    targetCharacterStatus.AntiActBuff = antiActBuffConfig
                        .GenBuff(Pos,
                            targetCharacterStatus.GetPos(),
                            Aim,
                            null, 0,
                            targetCharacterBodyBodySize, Caster);
                    //初始化buff时，如果是抓取技能，会触发技能
                    switch (antiActBuffConfig)
                    {
                        case CatchAntiActBuffConfig catchAntiActBuffConfig:
                            Caster.LoadSkill(null, catchAntiActBuffConfig.TrickSkill, SkillAction.CatchTrick);
                            Caster.NextSkill = null;
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
                        if (Caster.CatchingWho == targetCharacterStatus)
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
                            pushOnAir.UpSpeed, targetCharacterBodyBodySize, Caster);
                        targetCharacterStatus.AntiActBuff = antiActBuff;
                        break;
                    case PushOnEarth _:

                        InitBuff();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(opponentCharacterStatusAntiActBuff));
                }
            }
            else
            {
                //AttackFail 需要分两种情况 
                //清除技能数据
                Caster.ResetSnipe();
                Caster.ResetCastAct();
                //生成击中受击消息数据缓存
                Caster.IsBeHitBySomeOne = TwoDVector.TwoDVectorByPt(Caster.GetPos(), targetCharacterStatus.GetPos());
                targetCharacterStatus.IsHitSome = true;

                if (!isActSkill) // 说明目标不在攻击状态 通过特定配置读取
                {
                    if (!FailActBuffConfigToSelf.TryGetValue(targetCharacterBodyBodySize, out var failAntiBuff)) return;
                    var aim = TwoDVector.TwoDVectorByPt(targetCharacterStatus.GetPos(), Pos).GetUnit2();

                    //如果为抓取技能，会马上装载抓取buff附带的触发技能
                    switch (failAntiBuff)
                    {
                        case CatchAntiActBuffConfig catchAntiActBuffConfig:
                            targetCharacterStatus.CatchingWho = Caster;

                            targetCharacterStatus.LoadSkill(aim, catchAntiActBuffConfig.TrickSkill,
                                SkillAction.CatchTrick);
                            targetCharacterStatus.NextSkill = null;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(FailActBuffConfigToSelf));
                    }

                    var antiActBuff = failAntiBuff.GenBuff(targetCharacterStatus.GetPos(), Caster.GetPos(),
                        targetCharacterStatus.GetAim(),
                        null,
                        0, Caster.CharacterBody.BodySize, targetCharacterStatus);
                    Caster.AntiActBuff = antiActBuff;

                    // var antiActBuff = failAntiBuff.GenBuff(targetCharacterStatus.GetPos(), Caster.GetPos(),
                    //     targetCharacterStatus.GetAim(),
                    //     null,
                    //     0, Caster.CharacterBody.BodySize, targetCharacterStatus);
                    // Caster.AntiActBuff = antiActBuff;
                }
                else //在攻击状态，通过通用失败buff读取
                {
#if DEBUG
                    Console.Out.WriteLine($"Gen Common Back");
#endif
                    var antiActBuff = TempConfig.CommonBuffConfig.GenBuff(targetCharacterStatus.GetPos(),
                        Caster.GetPos(), Aim,
                        null, 0, Caster.CharacterBody.BodySize, targetCharacterStatus);
                    Caster.AntiActBuff = antiActBuff;
                }
            }
        }

        public HashSet<int> HitTeam(IQSpace qSpace)
        {
            var mapToGidList = qSpace.FilterToGIdPsList((body, bullet) => bullet.IsHitBody(body),
                this, RdZone.MoveToAnchor(Pos));
            return SomeTools.EnumerableToHashSet(mapToGidList.Select(x => x.GetId()));
        }

        public BulletMsg GenMsg()
        {
            return new BulletMsg(Pos, Aim, ResId, Caster?.GetPos());
        }

        public Bullet ActiveBullet(TwoDPoint casterPos, TwoDVector casterAim)
        {
            Pos = casterPos;
            Aim = casterAim;
            return this;
        }
    }

    public class Damage
    {
        public int StandardDamageValue;

        public Damage(int standardDamageValue)
        {
            StandardDamageValue = standardDamageValue;
        }
    }
}