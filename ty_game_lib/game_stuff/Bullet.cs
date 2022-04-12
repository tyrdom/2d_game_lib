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

    public class Bullet : IHitMedia, IHaveAnchor
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public Zone RdZone { get; }

        private bool CanOverBulletBlock { get; }
        private bool IsFAtk { get; }
        private int AmmoAddWhenSuccess { get; }

        public float WaveCast { get; }
        public Dictionary<size, BulletBox> SizeToBulletCollision { get; }
        public IBattleUnitStatus? Caster { get; set; }
        private Dictionary<size, IStunBuffMaker> SuccessStunBuffConfigToOpponent { get; }
        private Dictionary<size, IStunBuffMaker> FailActBuffConfigToSelf { get; }

        private int PauseToCaster { get; }
        private int PauseToOpponent { get; }
        private float DamageMulti { get; }
        private int Tough { get; }

        private int StunPower { get; }

        public IEnumerable<IRelationMsg> HitTeam(IEnumerable<IQSpace> qSpaces, SightMap? blockMap)
        {
            return HitAbleMediaStandard.HitTeam(qSpaces, this, blockMap);
        }

        public ObjType TargetType { get; }

        public bool HitNumLimit(out int num)
        {
            num = HitLimitNum;
            return HitLimitNum > 0;
        }

        public int HitLimitNum { get; }
        private hit_type HitType { get; }
        private int RestTick { get; set; }
        public bullet_id BulletId { get; }
        private int ProtectValueAdd { get; }

        private PlayingBuffsMaker AddBuffToCastWhenHitPass { get; }

        public static Bullet GenById(bullet_id id, uint pairKey = 0)
        {
            if (CommonConfig.Configs.bullets.TryGetValue(id, out var bullet))
            {
                return GenByConfig(bullet, pairKey);
            }

            throw new ArgumentOutOfRangeException();
        }

        public static Bullet GenById(string id, uint pairKey = 0)
        {
            var o = (bullet_id) Enum.Parse(typeof(bullet_id), id, true);
            return GenById(o, pairKey);
        }

        public void Sign(IBattleUnitStatus characterStatus)
        {
            Caster = characterStatus;

            foreach (var antiActBuffConfig in SuccessStunBuffConfigToOpponent.Values)
            {
                if (antiActBuffConfig is CatchStunBuffMaker catchAntiActBuffConfig)
                {
                    catchAntiActBuffConfig
                        .PickBySomeOne(characterStatus);
                }
            }

            foreach (var fValue in FailActBuffConfigToSelf.Values)
            {
                if (fValue is CatchStunBuffMaker catchStunBuffMaker)
                {
                    catchStunBuffMaker.PickBySomeOne(characterStatus);
                }
            }
        }


        private static Bullet GenByConfig(bullet bullet, uint pairKey = 0)
        {
            var dictionary = GameTools.GenBulletShapes(bullet.ShapeParams, bullet.LocalRotate, bullet.LocalPos,
                bullet.ShapeType);


            var bulletSuccessAntiActBuffConfigToOpponent = bullet.SuccessAntiActBuffConfigToOpponent;


            var antiActBuffConfig = GAntiActBuffConfigs(
                bulletSuccessAntiActBuffConfigToOpponent);

            var bulletFailActBuffConfigToSelf = bullet.FailActBuffConfigToSelf;

            var antiActBuffConfigs = GAntiActBuffConfigs(bulletFailActBuffConfigToSelf);


            var objType = bullet.TargetType switch
            {
                target_type.other_team => ObjType.OtherTeam,
                target_type.all_team => ObjType.AllTeam,
                _ => throw new ArgumentOutOfRangeException()
            };
            var tough = bullet.Tough;
            var bulletIsHAtk = bullet.IsHAtk;
            if (bullet.Tough == -1)
            {
                tough = (int) (pairKey * (1 + CommonConfig.OtherConfig.tough_grow));
            }


            var valuePerSecToValuePerTick = ((float) pairKey).ValuePerSecToValuePerTick();


            var bulletDamageMulti = bullet.DamageMulti <= 0
                ? valuePerSecToValuePerTick
                : bullet.DamageMulti;

            var bulletProtectValue = bullet.ProtectValue;
            if (bulletProtectValue < 0)
            {
                bulletProtectValue = (int) (bulletDamageMulti *
                                            CommonConfig.OtherConfig.protect_standardMulti);
            }


            var bulletSuccessAmmoAdd = bullet.SuccessAmmoAdd;
            if (bulletSuccessAmmoAdd < 0)
            {
                bulletSuccessAmmoAdd =
                    (int) (bulletDamageMulti * CommonConfig.OtherConfig.melee_ammo_gain_standard_multi);
            }


            var playingBuffsMaker = new PlayingBuffsMaker(bullet.SuccessPlayBuffAdd);

            var bulletStunPower = bullet.StunPower;
            return new Bullet(dictionary, antiActBuffConfig, antiActBuffConfigs, bullet.PauseToCaster,
                bullet.PauseToOpponent, objType, tough, bulletSuccessAmmoAdd, bulletDamageMulti, bulletProtectValue,
                bullet.HitType, bulletIsHAtk, bullet.CanOverBulletBlock, bullet.id, bullet.MaxHitNum,
                bullet.SoundWaveRad, playingBuffsMaker, bulletStunPower);
        }

        public static Dictionary<size, IStunBuffMaker> GAntiActBuffConfigs(
            IEnumerable<Buff> bulletFailActBuffConfigToSelf)
        {
            var actBuffConfigs = CommonConfig.Configs.bodys.ToDictionary(pair => pair.Key, pair =>
            {
                var failActBuffConfigToSelf =
                    bulletFailActBuffConfigToSelf as Buff[] ?? bulletFailActBuffConfigToSelf.ToArray();
                var firstOrDefault = failActBuffConfigToSelf.FirstOrDefault(x => x.size == pair.Key) ??
                                     failActBuffConfigToSelf.First(a => a.size == size.@default);
                return StunBuffStandard.GenStunBuffMakerByC(firstOrDefault.buff_type, firstOrDefault.buff_id);
            });
            return actBuffConfigs;
        }

        private Bullet(Dictionary<size, BulletBox> sizeToBulletCollision,
            Dictionary<size, IStunBuffMaker> successStunBuffConfigToOpponent,
            Dictionary<size, IStunBuffMaker> failActBuffConfigToSelf, uint pauseToCaster, uint pauseToOpponent,
            ObjType targetType, int tough, int ammoAddWhenSuccess, float damageMulti, int protectValueAdd,
            hit_type hitType, bool isHAtk, bool canOverBulletBlock, bullet_id bulletId, int hitLimitNum, float waveCast,
            PlayingBuffsMaker playingBuffsMaker, int stunPower)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            SizeToBulletCollision = sizeToBulletCollision;
            Caster = null;
            SuccessStunBuffConfigToOpponent = successStunBuffConfigToOpponent;
            FailActBuffConfigToSelf = failActBuffConfigToSelf;
            PauseToCaster = (int) pauseToCaster;

            PauseToOpponent = (int) pauseToOpponent;
#if DEBUG
            Console.Out.WriteLine($"{PauseToCaster} ----- {PauseToOpponent}");
#endif
            TargetType = targetType;
            Tough = tough;
            RestTick = 1;
            BulletId = bulletId;
            HitLimitNum = hitLimitNum;
            WaveCast = waveCast;
            AmmoAddWhenSuccess = ammoAddWhenSuccess;
            DamageMulti = damageMulti;
            ProtectValueAdd = protectValueAdd;
            HitType = hitType;
            CanOverBulletBlock = canOverBulletBlock;
            IsFAtk = !isHAtk;
            RdZone = GameTools.GenRdBox(sizeToBulletCollision);
            AddBuffToCastWhenHitPass = playingBuffsMaker;
            StunPower = stunPower;
        }


        public bool CanGoNextTick()
        {
            RestTick -= 1;
            return RestTick > 0;
        }

        public bool IsHit(ICanBeAndNeedHit characterBody, SightMap? blockMap)
        {
            var isBlockSightLine =
                blockMap?.IsBlockSightLine(new TwoDVectorLine(Pos, characterBody.GetAnchor())) ?? false;
            var isHit = GameTools.IsHit(this, characterBody);
#if DEBUG
            Console.Out.WriteLine($"bullet hit? id {BulletId} ~ {isHit} and {!isBlockSightLine}");
#endif
            return isHit && (CanOverBulletBlock || !isBlockSightLine);

            //造成伤害需要不被阻挡
        }

        public IRelationMsg? HitSth(ICanBeAndNeedHit canBeAndNeedHit)
        {
            switch (canBeAndNeedHit)
            {
                case CharacterBody targetCharacterBody:


                    var kill = HitOne(targetCharacterBody.CharacterStatus);

#if DEBUG
                    Console.Out.WriteLine($"bullet hit::{kill.HasValue}");
#endif
                    return Caster != null && kill.HasValue
                        ? new BulletHit(targetCharacterBody, kill.Value,
                            Caster.GetFinalCaster().CharacterStatus, this)
                        : (IRelationMsg?) null;

                case Trap trap:
                    var dmgShow = HitOne(trap);
                    return Caster != null && dmgShow.HasValue
                        ? new BulletHit(trap, dmgShow.Value, Caster.GetFinalCaster().CharacterStatus, this)
                        : (IRelationMsg?) null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(canBeAndNeedHit));
            }
        }

        private DmgShow? HitOne(Trap targetTrap)
        {
            return Caster switch
            {
                null => throw new Exception($"there is a no Caster Bullet {BulletId}"),
                CharacterStatus characterStatusCaster => HitOne(targetTrap, characterStatusCaster),
                Trap trapCaster => HitOne(targetTrap, trapCaster),
                _ => throw new ArgumentOutOfRangeException(nameof(Caster))
            };
        }


        private DmgShow? HitOne(Trap targetTrap, IBattleUnitStatus caster)
        {
            //目标为trap 直接成功攻击
            caster.BaseBulletAtkOk(PauseToCaster, AmmoAddWhenSuccess, targetTrap);

            var genDamage = caster.GenDamage(DamageMulti, true);
            var takeDamage = targetTrap.TakeDamage(genDamage);
            return takeDamage;
        }

        private DmgShow? HitOne(CharacterStatus targetCharacterStatus)
        {
            switch (Caster)
            {
                case null: throw new Exception("there is a no Caster Bullet");
                case CharacterStatus characterStatusCaster:

                    return HitOne(targetCharacterStatus, characterStatusCaster);
                case Trap trapCaster:
                    trapCaster.StartTrick();
                    return HitOne(targetCharacterStatus, trapCaster);
                default:
                    throw new ArgumentOutOfRangeException(nameof(Caster));
            }
        }

        private (HitCond hitCond, bool backAtk, IStunBuff? opponentCharacterStatusAntiActBuff, bool isActingSkill)
            IsAtkPass(CharacterStatus targetCharacterStatus, out bool isStunChangeToDaze)
        {
            var nowCastSkill = targetCharacterStatus.NowCastAct;
            var opponentCharacterStatusAntiActBuff = targetCharacterStatus.StunBuff;
            var opponentIsStun = opponentCharacterStatusAntiActBuff != null;
            var isActingSkill = nowCastSkill != null && nowCastSkill.InWhichPeriod() == SkillPeriod.Casting;
            var twoDVector = targetCharacterStatus.CharacterBody.Sight.Aim;
            var b4 = twoDVector.Dot(Aim) >= 0; // 是否从背后攻击
            var tough = targetCharacterStatus.GetNowTough();
            var b = tough < Tough;
            var b3 = !isActingSkill && IsFAtk && (HitType == hit_type.melee || b); //如果对手不在释放技能，并且是快攻击子弹


            var b2 = isActingSkill && b; //如果对手正在释放技能 ，对手坚韧小于攻击坚韧，则成功

            var characterBodyBodySize = targetCharacterStatus.CharacterBody.BodySize;
            var steady = CommonConfig.Configs.bodys[characterBodyBodySize].steady;
            var aStunPower = opponentIsStun ? 9 : b4 ? StunPower + 1 : StunPower;

            isStunChangeToDaze = steady > aStunPower;

#if DEBUG
            Console.Out.WriteLine($"StunToDaze {isStunChangeToDaze} : {steady} vs {aStunPower}");
#endif
            var atkOk = opponentIsStun || b4 || b3 || b2;
            if (atkOk)
            {
                if (!(Caster is CharacterStatus character) ||
                    !character.BuffTrick.TryGetValue(TrickCond.MyAtkOk, out var hashSet))
                    return (HitCond.Ok, b4, opponentCharacterStatusAntiActBuff, isActingSkill);


                var playingBuffs = hashSet.Select(x => x.Copy());
                character.AddPlayingBuff(playingBuffs);

                return (HitCond.Ok, b4, opponentCharacterStatusAntiActBuff, isActingSkill);
            }


            if (isActingSkill)
            {
                var toughBuffs = targetCharacterStatus.GetAndUseExistBuffs<ToughBuff>();
                if (Caster is CharacterStatus caster)
                {
                    var breakBuffs = caster.GetAndUseExistBuffs<BreakBuff>();

                    if (breakBuffs)
                    {
                        if (!toughBuffs)
                        {
                            return (HitCond.Ok, b4, opponentCharacterStatusAntiActBuff, isActingSkill);
                        }
                    }
                    else if (toughBuffs)
                    {
                        return (HitCond.Fail, b4, opponentCharacterStatusAntiActBuff, isActingSkill);
                    }
                }
                else if (toughBuffs)
                {
                    targetCharacterStatus.UseBuff(CommonConfig.OtherConfig.defPassBuffId);
                    return (HitCond.Fail, b4, opponentCharacterStatusAntiActBuff, isActingSkill);
                }
            }

            var b5 = isActingSkill && tough == Tough;
            if (b5)
            {
                return (HitCond.Draw, b4, opponentCharacterStatusAntiActBuff, isActingSkill);
            }

#if DEBUG
            Console.Out.WriteLine(
                $"attack ~~~from back:: {b4} cast over:: {b2}  not back  ::{b3}   target is cast{isActingSkill} now tough::{Tough},mid::{CommonConfig.OtherConfig.mid_tough}");
#endif

            if (targetCharacterStatus.BuffTrick.TryGetValue(TrickCond.OpponentAtkFail, out var value))
            {
                targetCharacterStatus.AddPlayingBuff(value.Select(x => x.Copy()));
            }

            return (HitCond.Fail, b4, opponentCharacterStatusAntiActBuff, isActingSkill);
        }


        private DmgShow? HitOne(CharacterStatus targetCharacterStatus, IBattleUnitStatus caster)
        {
            var protecting = targetCharacterStatus.NowProtectTick > 0;
            if (protecting)
            {
                return null;
            }

            var (atkOk, back, opponentCharacterStatusAntiActBuff, isActSkill) =
                IsAtkPass(targetCharacterStatus, out var changeToDaze);

            var targetCharacterBodyBodySize = targetCharacterStatus.CharacterBody.GetSize();

            //AttackOk 攻击成功
            switch (atkOk)
            {
                case HitCond.Ok:
                    return HitOk(changeToDaze);
                case HitCond.Fail:
                    HitFail();
                    return null;
                case HitCond.Draw:
                    HitDraw(changeToDaze);
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DmgShow? HitOk(bool b)
            {
                //基本方面
                // 释放方基本状态
                caster.BaseBulletAtkOk(PauseToCaster, AmmoAddWhenSuccess, targetCharacterStatus);

                if (!b && caster is CharacterStatus characterStatus)
                {
                    var playingBuffs = AddBuffToCastWhenHitPass.GenBuffs(targetCharacterStatus).ToArray();
#if DEBUG
                    Console.Out.WriteLine(
                        $"{characterStatus.GetId()}:playing buffs add when atk ok {playingBuffs.Length}");
#endif
                    characterStatus.AddPlayingBuff(playingBuffs);
                }


                // 被击中方基本状态改变 包括伤害
                var dmgShow =
                    targetCharacterStatus.BaseBeHitByBulletChange(Pos, ProtectValueAdd, caster, DamageMulti, back,
                        BulletId);

                //stun buff方面
                if (b)
                {
                    targetCharacterStatus.ResetSpeed();

                    var genById = PlayBuffStandard.GenById(CommonConfig.OtherConfig.dazeBuffId);
                    targetCharacterStatus.AddAPlayingBuff(genById);
                }
                else
                {
                    var antiActBuffConfig = SuccessStunBuffConfigToOpponent[targetCharacterBodyBodySize];

                    // 对手动作状态buff刷新
                    SetStunBuffToTarget(targetCharacterStatus, caster, opponentCharacterStatusAntiActBuff,
                        antiActBuffConfig, PauseToOpponent, Pos, Aim);
                }

                return dmgShow;
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
                                targetCharacterStatus.AbsorbRangeBullet(Pos, ProtectValueAdd, characterStatus,
                                    DamageMulti, back, BulletId);
                                // targetCharacterStatus.TrickBuffsToBothSide(characterStatus);
                                if (!IsFAtk)
                                {
                                    targetCharacterStatus.TrickBlockSkill(Pos);
                                }


                                break;
                            case hit_type.melee:
                                CharMeleeAtkFail(characterStatus, targetCharacterStatus, isActSkill,
                                    targetCharacterBodyBodySize);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        targetCharacterStatus.NowSkillTryTrickSkill(Pos);
                        break;
                    case Trap trap:
                        trap.FailAtk();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(caster));
                }
            }

            void HitDraw(bool b)
            {
                //Draw
                switch (caster)
                {
                    case CharacterStatus characterStatus:
                        CharAtkDraw(characterStatus, targetCharacterStatus, b);
                        break;
                    case Trap trap:
                        trap.FailAtk();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(caster));
                }
            }
        }

        public static void NoMediaHit(CharacterStatus targetCharacterStatus, IBattleUnitStatus caster,
            IStunBuff? opponentCharacterStatusAntiActBuff, IStunBuffMaker antiActBuffConfig, int pauseToOpponent,
            TwoDPoint pos
            , TwoDVector aim
        )
        {
            targetCharacterStatus.DirectStunBuffChange(pos);


            // 对手动作状态buff刷新
            SetStunBuffToTarget(targetCharacterStatus, caster, opponentCharacterStatusAntiActBuff,
                antiActBuffConfig, pauseToOpponent, pos, aim);
        }

        public static void SetStunBuffToTarget(CharacterStatus targetCharacterStatus, IBattleUnitStatus caster,
            IStunBuff? opponentCharacterStatusAntiActBuff, IStunBuffMaker antiActBuffConfig, int pauseToOpponent,
            TwoDPoint pos
            , TwoDVector aim)
        {
            switch (opponentCharacterStatusAntiActBuff)
            {
                case null:

                    InitBuff(targetCharacterStatus, antiActBuffConfig, caster, pauseToOpponent, pos, aim);
                    break;
                //对手被其他人抓取时，不在添加停顿帧和改变其buff 除非是自己抓的目标
                case Caught _:
                    if (caster.CatchingWho == targetCharacterStatus)
                    {
                        InitBuff(targetCharacterStatus, antiActBuffConfig, caster, pauseToOpponent, pos, aim);
                    }

                    break;
                // 其他情况刷新buff
                case PushStunOnAir pushOnAir:

                    if (pauseToOpponent > targetCharacterStatus.PauseTick)
                    {
                        targetCharacterStatus.SetPauseTick(pauseToOpponent);
                    }

                    var height = pushOnAir.Height;
                    var antiActBuff = antiActBuffConfig.GenBuff(pos,
                        targetCharacterStatus.GetPos(),
                        aim,
                        height,
                        pushOnAir.UpSpeed, targetCharacterStatus, caster);
                    targetCharacterStatus.SetStunBuff(antiActBuff);
                    break;
                case PushStunOnEarth _:
                    InitBuff(targetCharacterStatus, antiActBuffConfig, caster, pauseToOpponent, pos, aim);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(opponentCharacterStatusAntiActBuff));
            }
        }

        private static void InitBuff(CharacterStatus targetCharacterStatus, IStunBuffMaker antiActBuffConfig,
            IBattleUnitStatus caster, int pauseToOpponent, TwoDPoint pos, TwoDVector aim)
        {
            targetCharacterStatus.SetPauseTick(MathTools.Max(pauseToOpponent, targetCharacterStatus.PauseTick));
#if DEBUG
            Console.Out.WriteLine($"bullet hit!! target pause tick {targetCharacterStatus.PauseTick}");
#endif
            var stunBuff = antiActBuffConfig.GenBuff(pos, targetCharacterStatus.GetPos(), aim, null, 0,
                targetCharacterStatus, caster);
            targetCharacterStatus.SetStunBuff(stunBuff);
            //初始化buff时，如果是抓取技能，会触发技能
            if (antiActBuffConfig is CatchStunBuffMaker catchAntiActBuffConfig)
                caster.LoadCatchTrickSkill(null, catchAntiActBuffConfig);
        }

        private void CharAtkDraw(CharacterStatus bodyCaster, CharacterStatus targetCharacterStatus, bool b)
        {
            bodyCaster.ResetSnipe();
            bodyCaster.ResetCastAct();
            targetCharacterStatus.ResetSnipe();
            targetCharacterStatus.ResetCastAct();

            var antiActBuff = StuffLocalConfig.CommonBuffMaker.GenBuff(targetCharacterStatus.GetPos(),
                bodyCaster.GetPos(), Aim,
                null, 0, bodyCaster, targetCharacterStatus);
            bodyCaster.SetStunBuff(antiActBuff);
            if (b)
            {
                targetCharacterStatus.ResetSpeed();
            }
            else
            {
                var antiActBuff2 = StuffLocalConfig.CommonBuffMaker.GenBuff(bodyCaster.GetPos(),
                    targetCharacterStatus.GetPos(), Aim,
                    null, 0, targetCharacterStatus, bodyCaster);
                targetCharacterStatus.SetStunBuff(antiActBuff2);
            }
        }


        private void CharMeleeAtkFail(CharacterStatus bodyCaster, CharacterStatus targetCharacterStatus,
            bool isActSkill,
            size targetCharacterBodyBodySize)
        {
            //清除技能数据
            bodyCaster.ResetSnipe();
            bodyCaster.ResetCastAct();
            bodyCaster.ResetProtect();
            //生成击中受击消息数据缓存

            bodyCaster.SetHitMark(
                TwoDVector.TwoDVectorByPt(bodyCaster.GetPos(), targetCharacterStatus.GetPos()), BulletId);


            if (!isActSkill) // 说明目标不在攻击状态 通过特定配置读取
            {
                if (!FailActBuffConfigToSelf.TryGetValue(targetCharacterBodyBodySize, out var failAntiBuff)) return;
                var aim = TwoDVector.TwoDVectorByPt(targetCharacterStatus.GetPos(), Pos).GetUnit2();

                //如果为抓取技能，会马上装载抓取buff附带的触发技能
                if (failAntiBuff is CatchStunBuffMaker catchAntiActBuffConfig)
                {
                    targetCharacterStatus.CatchingWho = bodyCaster;
                    catchAntiActBuffConfig.PickBySomeOne(targetCharacterStatus);
                    targetCharacterStatus.LoadCatchTrickSkill(aim, catchAntiActBuffConfig);
                }

                var antiActBuff = failAntiBuff.GenBuff(targetCharacterStatus.GetPos(), bodyCaster.GetPos(),
                    targetCharacterStatus.GetAim(),
                    null,
                    0, bodyCaster, targetCharacterStatus, false);
                bodyCaster.SetStunBuff(antiActBuff);
            }
            else //对手在攻击状态，通过通用失败buff读取
            {
#if DEBUG
                Console.Out.WriteLine($"Gen Common Back");
#endif
                var antiActBuff = StuffLocalConfig.CommonBuffMaker.GenBuff(targetCharacterStatus.GetPos(),
                    bodyCaster.GetPos(), TwoDVector.Zero(),
                    null, 0, bodyCaster, targetCharacterStatus, false);
                bodyCaster.SetStunBuff(antiActBuff);
            }
        }

        public BulletMsg GenMsg()
        {
            return new BulletMsg(Pos, Aim, BulletId, Caster?.GetPos());
        }

        public IPosMedia Active(TwoDPoint casterPos, TwoDVector casterAim)
        {
            return
                PosMediaStandard.Active(casterPos, casterAim, this);
        }

        public TwoDPoint GetAnchor()
        {
            return Pos;
        }
    }

    internal enum HitCond
    {
        Ok,
        Fail,
        Draw
    }
}