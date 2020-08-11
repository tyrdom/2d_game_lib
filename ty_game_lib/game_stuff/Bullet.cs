using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
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

    public class Bullet : IHitStuff
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public Dictionary<BodySize, BulletBox> SizeToBulletCollision { get; }
        public CharacterStatus? Caster { get; set; }
        public readonly Dictionary<BodySize, IAntiActBuffConfig> SuccessAntiActBuffConfigToOpponent;
        public readonly Dictionary<BodySize, IAntiActBuffConfig> FailActBuffConfigToSelf;

        public readonly int PauseToCaster;
        public readonly int PauseToOpponent;
        public Damage Damage;
        public int Tough;
        public ObjType TargetType { get; }

        public int RestTick;
        public int ResId;


        public static Bullet GenByBulletId(string id)
        {
            if (TempConfig.Configs.bullets.TryGetValue(id, out var bullet))
            {
                return GenByConfig(bullet);
            }

            throw new ArgumentOutOfRangeException();
        }

        public void PickBySomeOne(CharacterStatus characterStatus)
        {
            Caster = characterStatus;
        }

        public static Bullet GenByConfig(bullet bullet)
        {
            var dictionary = GameTools.GenBulletShapes(bullet.ShapeParams, bullet.LocalRotate, bullet.LocalPos,
                bullet.ShapeType);


            var bulletSuccessAntiActBuffConfigToOpponent = bullet.SuccessAntiActBuffConfigToOpponent;


            var antiActBuffConfig = GAntiActBuffConfigs(
                bulletSuccessAntiActBuffConfigToOpponent);

            var bulletFailActBuffConfigToSelf = bullet.FailActBuffConfigToSelf;

            var antiActBuffConfigs = GAntiActBuffConfigs(bulletFailActBuffConfigToSelf);

            static IAntiActBuffConfig GenBuffByC(buff_type buffConfigToOpponentType, string configToOpponent) =>
                buffConfigToOpponentType switch
                {
                    buff_type.push_buff => GameTools.GenBuffByConfig(TempConfig.Configs.push_buffs[configToOpponent]),
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
                                return GenBuffByC(firstOrDefault.buff_type, firstOrDefault.buff_id);

                            case BodySize.Medium:
                                var firstOrDefault2 = bulletFailActBuffConfigToSelf.FirstOrDefault(x =>
                                    x.size == size.medium || x.size == size.@default);
                                return GenBuffByC(firstOrDefault2.buff_type, firstOrDefault2.buff_id);
                            case BodySize.Big:
                                var firstOrDefault3 = bulletFailActBuffConfigToSelf.FirstOrDefault(x =>
                                    x.size == size.big || x.size == size.@default);
                                return GenBuffByC(firstOrDefault3.buff_type, firstOrDefault3.buff_id);
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

            return new Bullet(dictionary, antiActBuffConfig, antiActBuffConfigs, bullet.PauseToCaster,
                bullet.PauseToOpponent, objType, bullet.Tough, 1, 1);
        }

        public Bullet(Dictionary<BodySize, BulletBox> sizeToBulletCollision,
            Dictionary<BodySize, IAntiActBuffConfig> successAntiActBuffConfigToOpponent,
            Dictionary<BodySize, IAntiActBuffConfig> failActBuffConfigToSelf, int pauseToCaster, int pauseToOpponent,
            ObjType targetType, int tough, int restTick, int resId)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = null;
            SuccessAntiActBuffConfigToOpponent = successAntiActBuffConfigToOpponent;
            FailActBuffConfigToSelf = failActBuffConfigToSelf;
            PauseToCaster = pauseToCaster;
            PauseToOpponent = pauseToOpponent;
            Damage = new Damage(0);
            TargetType = targetType;
            Tough = tough;
            RestTick = restTick;
            ResId = resId;
        }

        public bool CanGoNextTick()
        {
            RestTick -= 1;
            return RestTick >= 0;
        }

        public bool IsHit(CharacterBody characterBody)
        {
            return GameTools.IsHit(this, characterBody);
        }

        public bool IsHitBody(IIdPointShape targetBody)
        {
            switch (targetBody)
            {
                case CharacterBody characterBody1:
                    var isHit = Caster != null && IsHit(characterBody1);
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
            var b2 = isActSkill && objTough.GetValueOrDefault(0) < Tough; //如果对手正在释放技能 ，对手坚韧小于攻击坚韧，则成功
            var b3 = !isActSkill && Tough < TempConfig.MidTough; //如果对手不在释放技能，攻击坚韧小于中值，攻击成功

            if (protecting)
            {
                return;
            }

            var targetCharacterBodyBodySize = targetCharacterStatus.CharacterBody.BodySize;
            //AttackOk 攻击成功
            if (opponentIsStun || b2 || b3 || b4)
            {
                // 目标速度重置
                targetCharacterStatus.ResetSpeed();
                // 我方按配置添加攻击停帧
                Caster!.PauseTick = PauseToCaster;

                //如果没有锁定目标，则锁定当前命中的目标
                Caster.LockingWho ??= targetCharacterStatus;
                targetCharacterStatus.IsOnHitBySomeOne = true;
                //如果对手有抓取对象
                if (targetCharacterStatus.CatchingWho != null)
                {
                    //抓取脱手
                    targetCharacterStatus.CatchingWho.AntiActBuff = TempConfig.OutCaught;
                    targetCharacterStatus.CatchingWho = null;
                }

                //对手承受伤害
                targetCharacterStatus.DamageHealStatus.TakeDamage(Damage);


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
                            Caster.LoadSkill(null, catchAntiActBuffConfig.TrickSkill);
                            Caster.NextSkill = null;
                            break;

                        default:
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
                Caster!.NowCastSkill = null;
                Caster.NextSkill = null;
                Caster.LockingWho = null;

                if (!isActSkill) // 说明目标不在攻击状态 通过特定配置读取
                {
                    if (!FailActBuffConfigToSelf.TryGetValue(targetCharacterBodyBodySize, out var failAntiBuff))
                    {
                        failAntiBuff = null;
                    }

                    //如果为抓取技能，会马上装载抓取buff附带的触发技能
                    switch (failAntiBuff)
                    {
                        case CatchAntiActBuffConfig catchAntiActBuffConfig:
                            targetCharacterStatus.CatchingWho = Caster;
                            targetCharacterStatus.LoadSkill(null, catchAntiActBuffConfig.TrickSkill);
                            targetCharacterStatus.NextSkill = null;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(FailActBuffConfigToSelf));
                    }

                    var antiActBuff = failAntiBuff.GenBuff(targetCharacterStatus.GetPos(), Caster.GetPos(), Aim,
                        null,
                        0, Caster.CharacterBody.BodySize, targetCharacterStatus);
                    Caster.AntiActBuff = antiActBuff;
                }
                else //在攻击状态，通过通用buff读取
                {
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
                this);
            return SomeTools.ListToHashSet(mapToGidList.Select(x => x.GetId()));
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
        public int DamageValue;

        public Damage(int damageValue)
        {
            DamageValue = damageValue;
        }
    }
}