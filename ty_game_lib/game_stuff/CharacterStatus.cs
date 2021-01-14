﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Numerics;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    //(TwoDVector? move, IHitStuff? launchBullet, IMapInteractive? dropThing)

    public class CharacterStatus : IMoveBattleAttrModel, IBattleUnitStatus
    {
        public LevelUps LevelUps { get; }
        public CharacterBody CharacterBody;
        public int BaseAttrId { get; }
        public float MaxMoveSpeed { get; private set; }

        public float MinMoveSpeed { get; private set; }

        public float AddMoveSpeed { get; private set; }

        //move status
        public float NowMoveSpeed { get; private set; }

        public int GId { get; }

        //Snipe Status

        private SnipeAction? SnipeOnCallAct { get; set; }

        private int SnipeCallStack { get; set; }

        private SnipeAction? NowOnSnipeAct { get; set; }

        private int NowSnipeStep { get; set; }

        public Scope GetStandardScope()
        {
            return NowVehicle?.Scope ?? CharacterBody.Sight.StandardScope;
        }

        //Ammo
        public int NowAmmo { get; set; }

        private int GetAmmo()
        {
            return NowVehicle?.NowAmmo ?? NowAmmo;
        }

        private int MaxAmmo { get; set; }
        public IBattleUnitStatus? LockingWho { get; set; }

        public CharacterBody GetFinalCaster()
        {
            return CharacterBody;
        }

        public List<TwoDPoint> GetMayBeSomeThing()
        {
            return MayBeSomeThing;
        }

        public CharacterStatus? CatchingWho { get; set; }

        public int NowWeapon { get; private set; }

        private int MaxWeaponSlot { get; }

        public int GetNowMaxWeaponSlotNum()
        {
            return NowVehicle?.WeaponCarryMax ?? MaxWeaponSlot;
        }

        public Dictionary<int, Weapon> Weapons { get; }

        public Dictionary<int, Weapon> GetWeapons()
        {
            return NowVehicle == null ? Weapons : NowVehicle.Weapons;
        }
        //Skill Status

        public ICharAct? NowCastAct { get; private set; }

        private void SetAct(ICharAct charAct)
        {
            NowProtectValue = 0;
            NowCastAct = charAct;
        }

        private (TwoDVector? Aim, Skill skill, SkillAction opAction)? NextSkill { get; set; }

        //Prop
        public Prop? Prop { get; private set; }

        private int NowPropStack { get; set; }

        private int MaxPropStack { get; set; }

        //Vehicle
        public Vehicle? NowVehicle { get; private set; }

        //traps

        private uint MaxTrap { get; }

        public Queue<Trap> Traps { get; }

        public float TrapSurvivalMulti { get; set; }

        public float TrapAtkMulti { get; set; }

        //InterAct CallLong status

        private MapInteract? NowMapInteractive { get; set; }
        private uint NowCallLongStack { get; set; }

        private uint MaxCallLongStack { get; }

        //be hit status
        public int PauseTick { get; set; }

        public IStunBuff? StunBuff { get; set; }

        //protect status
        private int NowProtectValue { get; set; }
        private int MaxProtectValue { get; }

        private float ProtectTickMultiAdd { get; set; }
        public int NowProtectTick { get; private set; }

        //Game other Status

        public Dictionary<int, PassiveTrait> PassiveTraits { get; }
        private List<IPlayingBuff> PlayingBuffs { get; set; }


        public AttackStatus AttackStatus { get; }

        public void AttackStatusRefresh(Vector<float> atkAboutPassiveEffects)
        {
            BattleUnitMoverStandard.AtkStatusRefresh(atkAboutPassiveEffects, this);
        }

        public void RegenStatusRefresh(Vector<float> regenAttrPassiveEffects)
        {
            BattleUnitMoverStandard.RegenStatusRefresh(regenAttrPassiveEffects, this);
        }

        public void OtherStatusRefresh(Vector<float> otherAttrPassiveEffects)
        {
            BattleUnitMoverStandard.OtherStatusRefresh(otherAttrPassiveEffects, this);
        }


        public SurvivalStatus SurvivalStatus { get; }

        public RegenEffectStatus RegenEffectStatus { get; }

        public AbsorbStatus AbsorbStatus { get; }
        private float RecycleMulti { get; set; }
        public PlayingItemBag PlayingItemBag { get; }

        // for_tick_msg
        public bool HaveChange { get; set; }
        public SkillAction? SkillLaunch { get; private set; }
        public bool IsPause { get; private set; }
        public TwoDVector? IsBeHitBySomeOne { get; set; }
        public bool IsHitSome { get; set; }
        public List<TwoDPoint> MayBeSomeThing { get; }

        public ICharRuleData CharRuleData { get; }

        public CharacterStatus(int gId, int baseAttrId, PlayingItemBag playingItemBag,
            LevelUps playRuler, Dictionary<int, PassiveTrait>? passiveTraits = null)
        {
            LevelUps = playRuler;
            HaveChange = false;
            CharRuleData = new CharKillData();
            var genBaseAttrById = GameTools.GenBaseAttrById(baseAttrId);
            SurvivalStatus = SurvivalStatus.GenByConfig(genBaseAttrById);
            AttackStatus = AttackStatus.GenByConfig(genBaseAttrById);
            RegenEffectStatus = RegenEffectStatus.GenBaseByAttr(genBaseAttrById);
            AbsorbStatus = AbsorbStatus.GenBaseByAttr(genBaseAttrById);
            ProtectTickMultiAdd = 0;
            TrapAtkMulti = genBaseAttrById.TrapAtkMulti;
            TrapSurvivalMulti = genBaseAttrById.TrapSurvivalMulti;
            MaxTrap = genBaseAttrById.MaxTrapNum;
            Traps = new Queue<Trap>();
            MayBeSomeThing = new List<TwoDPoint>();
            CharacterBody = null!;
            MaxMoveSpeed = genBaseAttrById.MoveMaxSpeed;
            GId = gId;
            PauseTick = 0;
            LockingWho = null;
            CatchingWho = null;
            NowWeapon = 0;
            Weapons = new Dictionary<int, Weapon>();
            NowCastAct = null;
            NextSkill = null;
            StunBuff = null;
            PlayingBuffs = new List<IPlayingBuff>();

            NowProtectTick = 0;
            AddMoveSpeed = genBaseAttrById.MoveAddSpeed;
            MinMoveSpeed = genBaseAttrById.MoveMinSpeed;
            MaxProtectValue = LocalConfig.ProtectTick;
            PassiveTraits = passiveTraits ?? new Dictionary<int, PassiveTrait>();

            BaseAttrId = baseAttrId;
            PlayingItemBag = playingItemBag;
            NowMoveSpeed = 0f;
            SkillLaunch = null;
            IsPause = false;
            IsBeHitBySomeOne = null;
            IsHitSome = false;
            ResetSnipe();
            Prop = null;
            NowPropStack = 0;
            MaxPropStack = LocalConfig.StandardPropMaxStack;
            NowVehicle = null;
            NowAmmo = 0;
            MaxAmmo = genBaseAttrById.MaxAmmo;
            MaxCallLongStack = LocalConfig.MaxCallActTwoTick;
            NowCallLongStack = 0;
            NowMapInteractive = null;
            MaxWeaponSlot = LocalConfig.StandardWeaponNum;
            RecycleMulti = genBaseAttrById.RecycleMulti;
            StartPassiveInitRefresh();
        }

        public void StartPassiveInit(Dictionary<int, PassiveTrait> passiveTraits)
        {
        }

        private void StartPassiveInitRefresh()
        {
            var groupBy = PassiveTraits.Values.GroupBy(x => x.GetType());

            foreach (var passiveTraits in groupBy)
            {
                var firstOrDefault = passiveTraits.FirstOrDefault();
                if (firstOrDefault == null)
                {
                    continue;
                }

                var enumerable = passiveTraits.Select(x => x);
                var aggregate = enumerable.Aggregate(Vector<float>.Zero,
                    (s, x) => s + x.PassiveTraitEffect.GenEffect(x.Level).GetVector());

                RefreshStatusByAKindOfPass(firstOrDefault, aggregate);
            }
        }

        private void OpChangeAim(TwoDVector? aim)
        {
            CharacterBody.Sight.OpChangeAim(aim, GetNowScope());
        }

        public void Reborn()
        {
            PauseTick = 0;
            LockingWho = null;
            CatchingWho = null;
            NowCastAct = null;
            NextSkill = null;
            StunBuff = null;
            PlayingBuffs = new List<IPlayingBuff>();
            SurvivalStatus.Full();
            NowProtectTick = 30;
            NowMoveSpeed = 0f;
            SkillLaunch = null;
            IsPause = false;
            IsBeHitBySomeOne = null;
            IsHitSome = false;
            ResetSnipe();
            Prop = null;
            NowPropStack = 0;
        }

        public Scope? GetNowScope()
        {
            if (NowOnSnipeAct == null || NowSnipeStep < 0 || !GetWeapons().TryGetValue(NowWeapon, out var weapon))
                return NowVehicle?.Scope ?? null;
            var weaponZoomStepScope = weapon.ZoomStepScopes[NowSnipeStep];
            return weaponZoomStepScope;
        }


        private void LoadSkill(TwoDVector? aim, Skill skill, SkillAction skillAction)
        {
            //装载技能时，重置速度和锁定角色
            ResetSpeed();
            LockingWho = null;
            if (aim != null)
            {
                OpChangeAim(aim);
            }

            if (!skill.Launch(NowSnipeStep, GetAmmo())) return;
            SkillLaunch = skillAction;
            SetAct(skill);
        }

        private void ResetSpeed()
        {
            NowMoveSpeed = MinMoveSpeed;
        }


        public void ResetCastAct()
        {
            switch (NowCastAct)
            {
                case null:
                    break;
                case Interaction aInteraction:
                    if (aInteraction.InMapInteractable.InWhichMapInteractive != null)
                        aInteraction.InMapInteractable.InWhichMapInteractive.NowInterCharacterBody = null;
                    break;
                case Prop _:
                    break;
                case Skill _:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NowCastAct));
            }

            NowCastAct = null;
            NextSkill = null;
            LockingWho = null;
        }


        private Snipe? GetNowSnipe()
        {
            return NowTempSnipe;
        }

        private void CallSnipe(SnipeAction snipeAction)
        {
            if (snipeAction != SnipeAction.SnipeOff)
            {
                if (SnipeOnCallAct == snipeAction)
                {
                    SnipeCallStack += 1;
                }
                else
                {
                    SnipeOnCallAct = snipeAction;
                    SnipeCallStack = 1;
                }


                if (!GetWeapons().TryGetValue(NowWeapon, out var weapon) ||
                    !weapon.Snipes.TryGetValue(snipeAction, out var snipe))
                {
                    if (NowTempSnipe == null) return;
                    NowOnSnipeAct = SnipeAction.SnipeOff;
                    OffSnipe(NowTempSnipe);
                    return;
                }

                if (snipe.TrickTick >= SnipeCallStack)
                {
                    return;
                }


                NowOnSnipeAct = snipeAction;
                SnipeCallStack = 0;
                NowTempSnipe = snipe;
                OnSnipe(NowTempSnipe);
            }
            else
            {
                if (NowTempSnipe != null)
                {
                    NowOnSnipeAct = snipeAction;
                    OffSnipe(NowTempSnipe);
                }
                else ResetSnipe();
            }
        }

        private Snipe? NowTempSnipe { get; set; }

        private void GoSnipe()
        {
            if (NowTempSnipe == null) return;
            switch (NowOnSnipeAct)
            {
                case SnipeAction.SnipeOn1:
                    OnSnipe(NowTempSnipe);
                    break;

                case SnipeAction.SnipeOn2:
                    OnSnipe(NowTempSnipe);
                    break;

                case SnipeAction.SnipeOff:
                    OffSnipe(NowTempSnipe);
                    break;
                case SnipeAction.SnipeOn3:
                    OnSnipe(NowTempSnipe);
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //开镜
        private void OnSnipe(Snipe snipe)
        {
            NowSnipeStep = NowSnipeStep < snipe.MaxStep
                ? Math.Min(NowSnipeStep + snipe.AddStepPerTick, snipe.MaxStep)
                : Math.Max(NowSnipeStep - snipe.OffStepPerTick, snipe.MaxStep);
        }

        //关镜
        private void OffSnipe(Snipe snipe)
        {
            var snipeOffStepPerTick = NowSnipeStep - snipe.OffStepPerTick;
            if (snipeOffStepPerTick < 0)
            {
                ResetSnipe();
            }

            NowSnipeStep = snipeOffStepPerTick;
        }

        //重置
        public void ResetSnipe()
        {
            SnipeOnCallAct = null;

            SnipeCallStack = 0;
            NowTempSnipe = null;
            NowOnSnipeAct = null;

            NowSnipeStep = -1;
        }

        private CharGoTickResult ActNowActATick(
            TwoDVector? moveOp)
        {
            var actNowActATick = NowCastAct switch
            {
                null => new CharGoTickResult(),
                Prop prop => GoNowActATick(prop, moveOp),
                Skill skill => GoNowActATick(skill, moveOp),
                _ => throw new ArgumentOutOfRangeException(nameof(NowCastAct))
            };


            return actNowActATick;
        }

        private IPosMedia? SelfEffectFilter(IEffectMedia? effectMedia)
        {
            switch (effectMedia)
            {
                case null:
                    return null;
                case IPosMedia posMedia:
                    return posMedia;
                case SelfEffect selfEffect:
                    HitBySelfEffect(selfEffect);
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effectMedia));
            }
        }

        private void HitBySelfEffect(SelfEffect selfEffect)
        {
            List<IPlayingBuff> playingBuffs = PlayBuffStandard.AddBuffs(PlayingBuffs, selfEffect.PlayingBuffToAdd);
            PlayingBuffs = playingBuffs;

            var selfEffectRegenerationBase = selfEffect.RegenerationBase;
            if (selfEffectRegenerationBase == null) return;
            var effectRegenerationBase = selfEffectRegenerationBase.Value;
            if (NowVehicle == null)
            {
                SurvivalStatus.GetRegen(effectRegenerationBase, RegenEffectStatus);
            }
            else
            {
                NowVehicle.SurvivalStatus.GetRegen(effectRegenerationBase, NowVehicle.RegenEffectStatus);
                NowVehicle.ReloadAmmo(effectRegenerationBase.ReloadMulti);
            }
        }

        private CharGoTickResult GoNowActATick(ICharAct charAct,
            TwoDVector? moveOp)
        {
            var limitV = charAct switch
            {
                Interaction _ => null,
                Prop _ => GetAim().Multi(GetStandardSpeed(GetAim())),
                Skill _ => LockingWho == null
                    ? null
                    : TwoDVector.TwoDVectorByPt(GetPos(), LockingWho.GetPos())
                        .ClockwiseTurn(CharacterBody.Sight.Aim)
                        .AddX(-GetRr() - LockingWho.GetRr())
                        .MaxFixX(0), //在有锁定目标时，会根据与当前目标的向量调整，有一定程度防止穿模型
                _ => throw new ArgumentOutOfRangeException(nameof(charAct))
            };
            var f = GetNowSnipe()?.GetSpeedMulti(CharacterBody.GetSize());
            var fixMove = charAct switch
            {
                Interaction _ => null,
                Prop _ => moveOp?.Multi(GetStandardSpeed(moveOp)),
                Skill _ => f == null
                    ? moveOp?.Multi(GetStandardSpeed(moveOp))
                    : moveOp?.Multi(GetStandardSpeed(moveOp))
                        .Multi(f.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(charAct))
            };


#if DEBUG
            var lockingWhoGId = LockingWho == null ? "null" : LockingWho.GetId().ToString();
            Console.Out.WriteLine($"skill lock {lockingWhoGId} limitV ::{limitV}");
#endif

            var (move, bullet, snipeOff, getThing, interactive) = charAct
                .GoATick(GetPos(), GetAim(), fixMove, limitV);
            if (snipeOff)
            {
                CallSnipe(SnipeAction.SnipeOff);
            }

            var selfEffectFilter = SelfEffectFilter(bullet);

            if (getThing == null)
            {
                return new CharGoTickResult(move: move, launchBullet: selfEffectFilter);
            }

            var dropThings = getThing.ActWhichChar(this, interactive);

            HashSet<IAaBbBox>? dropThings1DropSet = null;
            int? t = null;
            switch (dropThings)
            {
                case null:
                    break;
                case DropThings dropThings1:
                    dropThings1DropSet = SomeTools.EnumerableToHashSet(dropThings1.DropSet.OfType<IAaBbBox>());
                    break;
                case TelePort telePort:
                    t = telePort.GMid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dropThings));
            }


            return new CharGoTickResult(move: move, launchBullet: selfEffectFilter,
                dropThing: dropThings1DropSet,
                getThing: getThing.InWhichMapInteractive, teleTo: t);
        }


        public void RecycleAProp(Prop prop)
        {
            NowPropStack = Math.Min(MaxPropStack,
                (int) (NowPropStack + prop.RecyclePropStack * (1 + GetRecycleMulti())));
        }

        private float GetRecycleMulti()
        {
            return NowVehicle?.RecycleMulti ?? RecycleMulti;
        }

        public IMapInteractable? PicAWeapon(Weapon weapon)
        {
            return weapon.PickedBySomebody(this);
        }


        private IEnumerable<IMapInteractable> DropWeapon(BodySize bodySize)
        {
            return GameTools.DropWeapon(Weapons, bodySize, GetPos());
        }

        public IEnumerable<IMapInteractable> GetInAVehicle(Vehicle vehicle)
        {
            if (NowVehicle != null) throw new Exception("have in a vehicle");

            var mapIntractable = DropWeapon(vehicle.Size);
            NowVehicle = vehicle;
            vehicle.Sign(this);
            return mapIntractable;
        }

        public IMapInteractable? PickAProp(Prop prop)
        {
            prop.Sign(this);
            if (Prop != null) return prop.DropAsIMapInteractable(GetPos());
            Prop = prop;
            return null;
        }

        private void ComboByNext(TwoDVector? operateAim)
        {
            if (NextSkill == null || NowCastAct == null ||
                NowCastAct.InWhichPeriod() != SkillPeriod.CanCombo)
            {
                OpChangeAim(null);
                return;
            }
#if DEBUG
            Console.Out.WriteLine($"{GId} ::skill next start {NextSkill.Value.skill.NowOnTick}");
#endif
            if (NextSkill.Value.opAction == SkillAction.Switch)
            {
                NowWeapon = (NowWeapon + 1) % GetWeapons().Count;
            }

            var aim = operateAim ?? NextSkill.Value.Aim;

            LoadSkill(aim, NextSkill.Value.skill, NextSkill.Value.opAction);

            NextSkill = null;
        }

        private void TempMsgClear()
        {
            if (SkillLaunch != null) SkillLaunch = null;
            if (IsBeHitBySomeOne != null) IsBeHitBySomeOne = null;
            if (IsHitSome) IsHitSome = false;
            MayBeSomeThing.Clear();
            CharRuleData.ClearTemp();
        }


        /// <summary>
        /// char main fuc 
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CharGoTickResult
            CharGoTick(Operate? operate) //角色一个tick行为
        {
            //清理消息缓存
            TempMsgClear();

            // 命中停帧 输入无效
            var b1 = PauseTick > 0;
            IsPause = b1;
            if (b1)
            {
                PauseTick -= 1;

                return new CharGoTickResult();
            }

            //  检查保护 进入保护
            if (NowProtectValue > MaxProtectValue)
            {
                NowProtectTick = (int) (LocalConfig.ProtectTick * (1 + ProtectTickMultiAdd));
                NowProtectValue = 0;
            }

            //  被硬直状态 输入无效
            var dPoint = GetPos();
            if (StunBuff != null)
            {
#if DEBUG
                Console.Out.WriteLine(
                    $"{GId}  {StunBuff.GetType()} ::anti v::{StunBuff.GetItp().ToString()}::anti buff :: {StunBuff.RestTick}");
#endif
                var (twoDPoint, antiActBuff) = StunBuff.GoTickDrivePos(dPoint);
                StunBuff = antiActBuff;

#if DEBUG
                Console.Out.WriteLine(
                    $"{GId} {StunBuff?.GetType()}  ::IPt {twoDPoint.ToString()} ::anti buff :: {StunBuff?.RestTick}");
#endif
                return new CharGoTickResult(move: twoDPoint);
            }

//
            if (NowVehicle != null)
            {
                var (isBroken, destroyBullet, weapons) = NowVehicle.GoATickCheckSurvival();
                if (isBroken)
                {
                    if (operate?.SpecialAction == SpecialAction.OutVehicle)
                    {
                        return OutNowVehicle();
                    }

                    if (destroyBullet == null)
                        return new CharGoTickResult();


                    NowVehicle = null;
                    NowProtectTick = 0;

                    var mapInteractables = weapons.Select(x => x.DropAsIMapInteractable(GetPos()));
                    return new CharGoTickResult(launchBullet: destroyBullet,
                        dropThing: SomeTools.EnumerableToHashSet(mapInteractables.OfType<IAaBbBox>()));
                }
            }


            if (!SurvivalStatus.GoATickAndCheckAlive())
            {
                return new CharGoTickResult(stillActive: false);
            }

            //
            if (NowProtectTick > 0)
            {
                NowProtectTick -= 1;
            }

            // 当前技能结束检查
            if (NowCastAct?.InWhichPeriod() == SkillPeriod.End) NowCastAct = null;

            // 当前技能的释放时候
            var opAction = operate?.GetAction();
            if (NowCastAct != null)
            {
                // 当前动作进行一个tick

                var actNowActATick = ActNowActATick(operate?.Move);

#if DEBUG
                Console.Out.WriteLine($"{GId} skill on {NowCastAct.NowOnTick}");
                Console.Out.WriteLine($"skill move {actNowActATick.Move}");
                var launchBullet = actNowActATick.LaunchBullet;
                if (launchBullet != null)
                    Console.Out.WriteLine(
                        $"launch IHitAble::{launchBullet.GetType()}::{launchBullet.Aim}||{launchBullet.Pos}");
#endif

                var skillAim = operate?.Aim ?? operate?.Move; // 检查下一个连续技能，如果有连续技能可以切换，则切换到下一个技能,NextSkill为null
                ComboByNext(skillAim);

                //没有更多Act操作，则返回
                if (opAction == null) return actNowActATick;

                //有Act操作，则检测出下一个状态id

                var weaponSkillStatus = NowCastAct.ComboInputRes();


                if (weaponSkillStatus == null) return actNowActATick;
                // 状态可用，则执行连技操作
                var status = weaponSkillStatus.Value;
                var b = opAction == SkillAction.Switch;
                var toUse = NowWeapon;
                if (b)
                {
                    status = 0;
                    toUse = (toUse + 1) % GetWeapons().Count;
                    CallSnipe(SnipeAction.SnipeOff);
                }

                var nowWeapon = GetWeapons().TryGetValue(toUse, out var weapon)
                    ? weapon
                    : GetWeapons().First().Value;

                if (!nowWeapon.SkillGroups.TryGetValue(CharacterBody.GetSize(), out var immutableDictionary) ||
                    !immutableDictionary.TryGetValue(opAction.Value, out var skills) ||
                    !skills.TryGetValue(status, out var skill)) return actNowActATick;

                switch (NowCastAct.InWhichPeriod())
                {
                    case SkillPeriod.Casting:
                        NextSkill ??= (skillAim, skill, opAction.Value);
                        break;
                    case SkillPeriod.CanCombo:
                        LoadSkill(skillAim, skill, opAction.Value);
                        NowWeapon = toUse;
                        break;
                    case SkillPeriod.End:
                        NowWeapon = toUse;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return actNowActATick;
            }

            // 没有技能在释放
            // 没有任何操作
            if (operate == null)
            {
                ResetSpeed();
                OpChangeAim(null);
                return new CharGoTickResult();
            }

            // 有各种操作

            // 与地图上物品互动
            var mapInteractive = operate.GetMapInteractive();

            if (mapInteractive != null)
            {
                switch (mapInteractive)
                {
                    case MapInteract.PickCall:
                        return new CharGoTickResult(mapInteractiveAbout: (MapInteract.PickCall,
                            CharacterBody));
                    case MapInteract.InVehicleCall:
                        return NowVehicle == null
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.InVehicleCall,
                                CharacterBody))
                            : new CharGoTickResult();
                    case MapInteract.GetInfoCall:
                        return new CharGoTickResult(mapInteractiveAbout: (MapInteract.GetInfoCall,
                            CharacterBody));
                    case MapInteract.RecycleCall:
                        return CallLongTouch(MapInteract.RecycleCall)
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.RecycleCall,
                                CharacterBody))
                            : new CharGoTickResult();
                    case MapInteract.KickVehicleCall:
                        return CallLongTouch(MapInteract.KickVehicleCall) && NowVehicle == null
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.KickVehicleCall,
                                CharacterBody))
                            : new CharGoTickResult();
                    case MapInteract.BuyOrApplyCall:
                        return CallLongTouch(MapInteract.BuyOrApplyCall)
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.BuyOrApplyCall,
                                CharacterBody))
                            : new CharGoTickResult();
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ResetLongInterAct();

            // 瞄准请求
            var snipeAction = operate.GetSnipe();
            if (snipeAction != null)
            {
                CallSnipe(snipeAction.Value);
            }
            else
            {
                GoSnipe();
            }

            // 转换视野方向
            if (operate.Aim != null)
            {
                OpChangeAim(operate.Aim);
            }

            //启动技能
            if (opAction != null)
            {
#if DEBUG
                Console.Out.WriteLine($"skill start {opAction.ToString()}");
#endif

                // 非连击状态切换武器
                if (opAction == SkillAction.Switch)
                {
                    NowWeapon = (NowWeapon + 1) % GetWeapons().Count;

                    SkillLaunch = SkillAction.Switch;
                    CallSnipe(SnipeAction.SnipeOff);
                }
                // 发动当前武器技能组的起始技能0
                else
                {
                    if (!GetWeapons().TryGetValue(NowWeapon, out var weapon) ||
                        !weapon.SkillGroups.TryGetValue(CharacterBody.GetSize(), out var value1) ||
                        !value1.TryGetValue(opAction.Value, out var value) ||
                        !value.TryGetValue(0, out var skill)) return new CharGoTickResult();
                    LoadSkill(null, skill, opAction.Value);
                    var actNowSkillATick = ActNowActATick(operate.Move);
                    return actNowSkillATick;
                }
            }


            var specialAction = operate.GetSpecialAction();
            if (specialAction != null)
            {
                switch (specialAction)
                {
                    case SpecialAction.UseProp:
                        if (Prop == null) return new CharGoTickResult();
                        if (Prop.Launch(NowPropStack))
                        {
                            NowPropStack -= Prop.StackCost;
                            SetAct(Prop);
                        }

                        break;
                    case SpecialAction.OutVehicle:
                        return OutNowVehicle();


                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //其他移动操作
            var twoDVector = operate.GetMove();

            if (twoDVector == null)
            {
                ResetSpeed();
                return new CharGoTickResult();
            }

            // 加速跑步运动，有上限
            if (operate.Aim == null)
            {
                OpChangeAim(twoDVector);
            }

            NowMoveSpeed = GetStandardSpeed(twoDVector);
            var nowSnipe = GetNowSnipe();
            var multiSpeed = NowMoveSpeed * nowSnipe?.GetSpeedMulti(CharacterBody.GetSize()) ?? NowMoveSpeed;
            var dVector = twoDVector.Multi(multiSpeed);

            return new CharGoTickResult(move: dVector);
        }

        private CharGoTickResult OutNowVehicle()
        {
            if (NowVehicle == null) return new CharGoTickResult();
            {
                NowVehicle.OutAct.Launch(0, 0);
                SetAct(NowVehicle.OutAct);
                NowVehicle.WhoDriveOrCanDrive = null;

                var genIMapInteractable = NowVehicle.DropAsIMapInteractable(GetPos());
                NowVehicle = null;
                return new CharGoTickResult(dropThing: new HashSet<IAaBbBox> {genIMapInteractable});
            }
        }

        private void ResetLongInterAct()
        {
            NowMapInteractive = null;
            NowCallLongStack = 0;
        }

        private bool CallLongTouch(MapInteract kickVehicleCall)
        {
            var b1 = NowMapInteractive == kickVehicleCall;
            if (b1)
            {
                NowCallLongStack++;
            }

            var b = NowCallLongStack > MaxCallLongStack;
            if (b || !b1)
            {
                ResetLongInterAct();
            }

            return b;
        }

        private float GetStandardSpeed(TwoDVector move)
        {
            var dot = move.Dot(CharacterBody.Sight.Aim);
            var normalSpeedMinCos = MathTools.Max(0f, MathTools.Min(1f,
                (dot + LocalConfig.DecreaseMinCos) / (LocalConfig.DecreaseMinCos + LocalConfig.NormalSpeedMinCos)
            ));
            var moveDecreaseMinMulti = LocalConfig.MoveDecreaseMinMulti +
                                       (1f - LocalConfig.MoveDecreaseMinMulti) * normalSpeedMinCos;
            var maxMoveSpeed = GetMaxMoveSpeed() * moveDecreaseMinMulti;
            var nowMoveSpeed = MathTools.Max(GetMinMoveSpeed(), MathTools.Min(maxMoveSpeed,
                NowMoveSpeed + GetAddMoveSpeed()));
            return nowMoveSpeed;
        }

        private float GetAddMoveSpeed()
        {
            return NowVehicle?.AddMoveSpeed ?? AddMoveSpeed;
        }

        private float GetMinMoveSpeed()
        {
            return NowVehicle?.MinMoveSpeed ?? MinMoveSpeed;
        }

        private float GetMaxMoveSpeed()
        {
            return NowVehicle?.MaxMoveSpeed ?? MaxMoveSpeed;
        }

        public TwoDPoint GetPos()
        {
            return CharacterBody.NowPos;
        }

        public int GetId()
        {
            return GId;
        }

        public void BaseBulletAtkOk(int pauseToCaster, int ammoAddWhenSuccess, IBattleUnitStatus targetCharacterStatus)
        {
            PauseTick = pauseToCaster;
            AddAmmo(ammoAddWhenSuccess);
            //如果没有锁定目标，则锁定当前命中的目标
            LockingWho ??= targetCharacterStatus;
            IsHitSome = true;
        }

        public TwoDVector GetAim()
        {
            return CharacterBody.Sight.Aim;
        }

        public Damage GenDamage(float damageMulti, bool b4)
        {
            var nowVehicleAttackStatus = NowVehicle?.AttackStatus ?? AttackStatus;
            return nowVehicleAttackStatus.GenDamage(damageMulti, b4);
        }

        public void LoadCatchTrickSkill(TwoDVector? aim, CatchStunBuffConfig catchAntiActBuffConfig)
        {
            LoadSkill(aim, catchAntiActBuffConfig.TrickSkill, SkillAction.CatchTrick);
            NextSkill = null;
        }

        public float GetRr()
        {
            return CharacterBody.GetRr();
        }

        public void AddAKillScore(CharacterBody characterBody)
        {
            CharRuleData.AddAKill();
        }

        private void AddProtect(int protectValueAdd)
        {
            NowProtectValue += protectValueAdd;
        }

        public void AddAmmo(int ammoAdd)
        {
            if (NowVehicle == null)
            {
                NowAmmo = Math.Min(MaxAmmo, NowAmmo + ammoAdd);
            }
            else
            {
                NowVehicle.AddAmmo(ammoAdd);
            }
        }


        public void PassiveEffectChangeOther(Vector<float> otherAttrPassiveEffects,
            (int MaxAmmo, float MoveMaxSpeed, float MoveMinSpeed, float MoveAddSpeed, int StandardPropMaxStack, float
                RecycleMulti) otherBaseStatus)
        {
            var lossAmmo = MaxAmmo - NowAmmo;
            var (maxAmmo, moveMaxSpeed, _, moveAddSpeed, _, recycleMulti) = otherBaseStatus;
            MaxAmmo = (int) (maxAmmo * (1f + otherAttrPassiveEffects[0]));
            NowAmmo = Math.Max(0, MaxAmmo - lossAmmo);
            var max = otherAttrPassiveEffects[1];
            MaxMoveSpeed = moveMaxSpeed * (1f + max / (max + 1f));
            var add = otherAttrPassiveEffects[2];
            AddMoveSpeed = moveAddSpeed * (1f + add / (add + 1f));
            var lossP = MaxPropStack - NowPropStack;
            MaxPropStack = (int) (LocalConfig.StandardPropMaxStack * (1f + otherAttrPassiveEffects[3]));
            NowPropStack = MaxPropStack - lossP;
            RecycleMulti = recycleMulti * (1f + otherAttrPassiveEffects[4]);
        }


        public bool LoadInteraction(Interaction charAct)
        {
            var b = NowCastAct == null;
            if (!b) return b;
            charAct.Launch();
            SetAct(charAct);
            return b;
        }

        public void RecycleWeapon()
        {
            NowAmmo = MaxAmmo;
            NowVehicle?.FullAmmo();
        }

        public void AddPlayingBuff(IEnumerable<IPlayingBuff> playingBuffs)
        {
            PlayBuffStandard.AddBuffs(PlayingBuffs, playingBuffs);
        }


        private Vector<float> GetPassiveEffects<T>() where T : IPassiveTraitEffect
        {
            var passiveTraits = PassiveTraits.Values.Where(x => x.PassiveTraitEffect is T);
            var passiveTraitEffects = passiveTraits.Select(x => x.PassiveTraitEffect.GenEffect(x.Level));
            var traitEffects = passiveTraitEffects.ToList();
            var firstOrDefault = traitEffects.FirstOrDefault();
            if (firstOrDefault == null) return Vector<float>.Zero;
            var r = traitEffects.Aggregate(Vector<float>.Zero, (s, x) => s + x.GetVector());
            return r;
        }


        public void SurvivalStatusRefresh(Vector<float> survivalAboutPassiveEffects)
        {
            BattleUnitMoverStandard.SurvivalStatusRefresh(survivalAboutPassiveEffects, this);
        }


        private void RefreshStatusByAKindOfPass(PassiveTrait passiveTrait, Vector<float> vector)
        {
            var v = vector;
            switch (passiveTrait.PassiveTraitEffect)
            {
                case OtherAttrPassiveEffect _:
                    OtherStatusRefresh(v);
                    NowVehicle?.OtherStatusRefresh(v);
                    break;
                case RegenPassiveEffect _:
                    RegenStatusRefresh(v);
                    NowVehicle?.RegenStatusRefresh(v);
                    break;
                case AtkAboutPassiveEffect _:
                    AttackStatusRefresh(v);
                    NowVehicle?.AttackStatusRefresh(v);
                    break;
                case SurvivalAboutPassiveEffect _:
                    SurvivalStatusRefresh(v);
                    NowVehicle?.SurvivalStatusRefresh(v);
                    break;
                case AddItem _:
                    var itemId = (int) v[0];
                    var num = (int) v[1];
                    var gameItem = new GameItem(itemId, num);
                    PlayingItemBag.Gain(gameItem);
                    break;
                case AbsorbAboutPassiveEffect _:
                    AbsorbStatusRefresh(v);
                    NowVehicle?.AbsorbStatusRefresh(v);
                    break;
                case TrapEffect _:
                    TrapAboutRefresh(v);
                    NowVehicle?.TrapAboutRefresh(v);
                    break;
                case TickAddEffect _:
                    TickAboutRefresh(v);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AbsorbStatusRefresh(Vector<float> vector)
        {
            BattleUnitMoverStandard.AbsorbStatusRefresh(vector, this);
        }

        public void PickAPassive(PassiveTrait passiveTrait)
        {
            var passiveTraitPassId = passiveTrait.PassId;
            if (PassiveTraits.TryGetValue(passiveTraitPassId, out var trait))
            {
                trait.AddLevel(passiveTrait.Level);
            }
            else
            {
                PassiveTraits.Add(passiveTraitPassId, passiveTrait);
            }

            Vector<float> v;
            if (passiveTrait.PassiveTraitEffect is AddItem addItem)
            {
                v = addItem.GetVector();
            }
            else
            {
                var passiveTraits =
                    PassiveTraits.Values.Where(x =>
                        x.PassiveTraitEffect.GetType() == passiveTrait.GetType());
                v = passiveTraits.Aggregate(Vector<float>.Zero,
                    (s, x) => s + x.PassiveTraitEffect.GenEffect(x.Level).GetVector());
            }


            RefreshStatusByAKindOfPass(passiveTrait, v);
        }

        private void TickAboutRefresh(Vector<float> tickAdds)
        {
            ProtectTickMultiAdd = tickAdds[0];
        }

        private void TrapAboutRefresh(Vector<float> trapAdd)
        {
            BattleUnitMoverStandard.TrapAboutRefresh(trapAdd, this);
        }


        public void RecyclePassive(PassiveTrait passiveTrait)
        {
            if (!LocalConfig.Configs.passives.TryGetValue(passiveTrait.PassId, out var passive)) return;
            var passiveRecycleMoney =
                passive.recycle_money.Select(x => new GameItem(x.item, (int) (x.num * (1 + GetRecycleMulti()))));
            foreach (var gameItem in passiveRecycleMoney)
            {
                PlayingItemBag.Gain(gameItem);
            }
        }

        public void AbsorbRangeBullet(TwoDPoint pos, int protectValueAdd, IBattleUnitStatus bodyCaster,
            float damageMulti, bool back)
        {
            IsBeHitBySomeOne =
                TwoDVector.TwoDVectorByPt(GetPos(), pos);
            var pa = GetProtectAbsorb();
            var valueAdd = (int) (protectValueAdd * (1 + pa));
            AddProtect(valueAdd);

            var genDamage = bodyCaster.GenDamage(damageMulti, back);
            var genDamageShardedDamage = genDamage.MainDamage + genDamage.ShardedDamage * genDamage.ShardedNum;
            var genDamageShardedNum = genDamage.ShardedNum + 1;

            var ammoAbsorb = GetAmmoAbsorb();
            AddAmmo((int) (genDamageShardedDamage * ammoAbsorb));
            if (NowVehicle != null)
                NowVehicle.AbsorbDamage(genDamageShardedDamage, genDamageShardedNum, genDamage.ShardedDamage);
            else
                AbsorbDamage(genDamageShardedDamage, genDamageShardedNum, genDamage.ShardedDamage);
        }

        private float GetAmmoAbsorb()
        {
            return NowVehicle?.AbsorbStatus.AmmoAbs ?? AbsorbStatus.AmmoAbs;
        }

        private void AbsorbDamage(uint genDamageShardedDamage, uint genDamageShardedNum, uint shardedDamage)
        {
            SurvivalStatus.AbsorbDamage(genDamageShardedDamage, genDamageShardedNum, AbsorbStatus, shardedDamage);
        }

        public bool BaseBeHitByBulletChange(TwoDPoint pos, int protectValueAdd, IBattleUnitStatus bodyCaster,
            float damageMulti, bool back)
        {
            ResetSpeed();
            ResetSnipe();
            ResetCastAct();
            IsBeHitBySomeOne =
                TwoDVector.TwoDVectorByPt(GetPos(), pos);


            if (CatchingWho != null)
            {
                CatchingWho.StunBuff = LocalConfig.OutCaught;
                CatchingWho = null;
            }


            AddProtect(protectValueAdd);
            var takeDamage = TakeDamage(bodyCaster.GenDamage(damageMulti, back));
            var b = bodyCaster.GetFinalCaster().Team != CharacterBody.Team;
            if (takeDamage & b)
            {
                bodyCaster.AddAKillScore(CharacterBody);
            }

            return takeDamage;
        }

        private float GetProtectAbsorb()
        {
            var absorbStatusProtectAbs = NowVehicle?.AbsorbStatus.ProtectAbs ?? AbsorbStatus.ProtectAbs;
            return absorbStatusProtectAbs;
        }

        private bool TakeDamage(Damage genDamage)
        {
            if (NowVehicle == null) return SurvivalStatus.TakeDamage(genDamage);
            NowVehicle.SurvivalStatus.TakeDamage(genDamage);
            return false;
        }

        public bool CheckCanBeHit()
        {
            return !SurvivalStatus.IsDead() || StunBuff != null;
        }

        public void AddTrap(Trap genATrap)
        {
            if (Traps.Count < MaxTrap)
            {
                Traps.Enqueue(genATrap);
            }

            var trap = Traps.Dequeue();
            //NotOverFlow false 会在下一时刻回收
            trap.NotOverFlow = false;
            Traps.Enqueue(genATrap);
        }

        public void PassiveEffectChangeTrap(Vector<float> trapAdd,
            (float TrapAtkMulti, float TrapSurvivalMulti) trapBaseAttr)
        {
            BattleUnitMoverStandard.PassiveEffectChangeTrap(trapAdd, trapBaseAttr, this);
        }

        public LevelUpsData GetNowLevelUpData()
        {
            return LevelUps.NowLevelUpsData;
        }
    }

    public class LevelUps
    {
        public static LevelUps NoneLevelUps()
        {
            var levelUpsData = new LevelUpsData(null, 0, new GameItem[] { }, new int[] { });
            var levelUpsDatas = new Dictionary<int, LevelUpsData>() {{1, levelUpsData}};
            return new LevelUps(levelUpsDatas.ToImmutableDictionary());
        }

        public LevelUps(ImmutableDictionary<int, LevelUpsData> levelUpDict)
        {
            LevelUpDict = levelUpDict;
            NowLevel = 1;
            NowExp = 0;
            NowLevelUpsData = levelUpDict.TryGetValue(NowLevel, out var levelUpsData)
                ? levelUpsData
                : throw new DirectoryNotFoundException($"not level {NowLevel}");
        }

        public ImmutableDictionary<int, LevelUpsData> LevelUpDict { get; }
        public int NowLevel { get; set; }

        private int NowExp { get; set; }
        public LevelUpsData NowLevelUpsData { get; private set; }

        private IEnumerable<int> GainExp(int expValue)
        {
            var nextExp = NowLevelUpsData.NextExp;
            if (nextExp == null)
            {
                return new int[] { };
            }

            NowExp += expValue;
            var res = new List<int>();
            while (NowExp >= nextExp)

            {
                NowExp -= nextExp.Value;
                res.AddRange(NowLevelUpsData.PassAddId);
                NowLevel++;
                if (LevelUpDict.TryGetValue(NowLevel, out var levelUpsData))
                {
                    NowLevelUpsData = levelUpsData;
                }
            }

            return res;
        }
    }

    public readonly struct LevelUpsData
    {
        public LevelUpsData(int? nextExp, int reBornAboutTick, GameItem[] rebornCost, int[] passAddId)
        {
            NextExp = nextExp;
            ReBornAboutTick = reBornAboutTick;
            RebornCost = rebornCost;
            PassAddId = passAddId;
        }

        public int? NextExp { get; }
        public int ReBornAboutTick { get; }
        public GameItem[] RebornCost { get; }
        public int[] PassAddId { get; }
    }

    public struct AbsorbStatus
    {
        private AbsorbStatus(float hpAbs, float armorAbs, float shieldAbs, float ammoAbs, float protectAbs)
        {
            HpAbs = hpAbs;
            ArmorAbs = armorAbs;
            ShieldAbs = shieldAbs;
            ProtectAbs = protectAbs;
            AmmoAbs = ammoAbs;
        }

        public float HpAbs { get; private set; }
        public float ArmorAbs { get; private set; }
        public float ShieldAbs { get; private set; }
        public float AmmoAbs { get; private set; }
        public float ProtectAbs { get; private set; }


        public static AbsorbStatus GenBaseByAttr(base_attribute genBaseAttrById)
        {
            return new AbsorbStatus(genBaseAttrById.HPAbsorb, genBaseAttrById.ArmorAbsorb, genBaseAttrById.ShieldAbsorb,
                genBaseAttrById.AmmoAbsorb, genBaseAttrById.ProtectAbsorb
            );
        }

        public void PassiveEffectChange(Vector<float> vector, AbsorbStatus regenBaseAttr)
        {
            HpAbs = regenBaseAttr.HpAbs * (1 + vector[0]);
            ArmorAbs = regenBaseAttr.ArmorAbs * (1 + vector[1]);
            ShieldAbs = regenBaseAttr.ShieldAbs * (1 + vector[2]);
            AmmoAbs = regenBaseAttr.AmmoAbs * (1 + vector[3]);
            ProtectAbs = regenBaseAttr.ProtectAbs * (1 + vector[4]);
        }
    }
}