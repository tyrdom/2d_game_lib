using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    //(TwoDVector? move, IHitStuff? launchBullet, IMapInteractive? dropThing)

    public class CharacterStatus : IMoveBattleAttrModel, IBattleUnitStatus
    {
        public CharacterBody CharacterBody;
        public base_attr_id BaseAttrId { get; }
        public float MaxMoveSpeed { get; private set; }

        public float MinMoveSpeed { get; private set; }

        public float AddMoveSpeed { get; private set; }

        //move status
        public float NowMoveSpeed { get; private set; }

        public int GId { get; }

        //Snipe Status

        private SnipeAction? SnipeOnCallAct { get; set; }

        private int SnipeCallStack { get; set; }

        private SnipeAction? NowInSnipeAct { get; set; }

        private int NowSnipeStep { get; set; }

        private int NowAmmo { get; set; }

        private int GetAmmo()
        {
            return NowVehicle?.NowAmmo ?? NowAmmo;
        }

        private int MaxAmmo { get; set; }
        public IBattleUnitStatus? LockingWho { get; set; }

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

        private Dictionary<int, Weapon> Weapons { get; }

        public Dictionary<int, Weapon> GetWeapons()
        {
            return NowVehicle == null ? Weapons : NowVehicle.Weapons;
        }
        //Skill Status

        public ICharAct? NowCastAct { get; private set; }

        private (TwoDVector? Aim, Skill skill, SkillAction opAction)? NextSkill { get; set; }

        //Prop
        private Prop? Prop { get; set; }

        private int NowPropStack { get; set; }

        private int MaxPropStack { get; set; }

        //Vehicle
        public Vehicle? NowVehicle { get; private set; }

        //traps

        private uint MaxTrap { get; }

        public Queue<Trap> Traps { get; }

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

        public int NowProtectTick { get; private set; }

        //Game other Status

        private Dictionary<int, PassiveTrait> Traits { get; }
        private List<IPlayingBuff> PlayingBuffs { get; set; }


        public AttackStatus AttackStatus { get; }

        public void AttackStatusRefresh(Vector<float> atkAboutPassiveEffects)
        {
            BattleUnitStandard.AtkStatusRefresh(atkAboutPassiveEffects, this);
        }

        public void OtherStatusRefresh(Vector<float> otherAttrPassiveEffects)
        {
            BattleUnitStandard.OtherStatusRefresh(otherAttrPassiveEffects, this);
        }

        public SurvivalStatus SurvivalStatus { get; private set; }

        private float RecycleMulti { get; set; }
        public PlayingItemBag PlayingItemBag { get; }

        // for_tick_msg
        public SkillAction? SkillLaunch { get; private set; }
        public bool IsPause { get; private set; }
        public TwoDVector? IsBeHitBySomeOne { get; set; }
        public bool IsHitSome { get; set; }
        public List<TwoDPoint> MayBeSomeThing { get; }


        public CharacterStatus(int gId, int maxProtectValue, base_attr_id baseAttrId, PlayingItemBag playingItemBag)
        {
            var (max, min, add, maxAmmo, recycle, baseAtkStatus, baseSurvivalStatus) = GenAttrBaseByConfig(baseAttrId);


            MaxTrap = TempConfig.StandardMaxTrap;
            Traps = new Queue<Trap>();
            MayBeSomeThing = new List<TwoDPoint>();
            CharacterBody = null!;
            MaxMoveSpeed = max;
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
            SurvivalStatus = baseSurvivalStatus;
            NowProtectTick = 0;
            AddMoveSpeed = add;
            MinMoveSpeed = min;
            MaxProtectValue = maxProtectValue;
            Traits = new Dictionary<int, PassiveTrait>();
            AttackStatus = baseAtkStatus;
            BaseAttrId = baseAttrId;
            PlayingItemBag = playingItemBag;
            NowMoveSpeed = 0f;
            SkillLaunch = null;
            IsPause = false;
            IsBeHitBySomeOne = null;
            IsHitSome = false;
            SnipeOnCallAct = null;
            SnipeCallStack = 0;
            NowInSnipeAct = null;
            NowSnipeStep = 0;
            Prop = null;
            NowPropStack = 0;
            MaxPropStack = TempConfig.StandardPropMaxStack;
            NowVehicle = null;
            NowAmmo = 0;
            MaxAmmo = maxAmmo;
            MaxCallLongStack = TempConfig.MaxCallActTwoTick;
            NowCallLongStack = 0;
            NowMapInteractive = null;
            MaxWeaponSlot = TempConfig.StandardWeaponNum;
            RecycleMulti = recycle;
        }

        public static (float max, float min, float add, int maxAmmo, float recycle, AttackStatus attackStatus,
            SurvivalStatus
            survivalStatus)
            GenAttrBaseByConfig(base_attr_id baseAttrId)
        {
            if (!TempConfig.Configs.base_attributes.TryGetValue(baseAttrId, out var attribute))
                throw new Exception($"no such attr {baseAttrId}");
            var (baseSurvivalStatus, baseAtkStatus) = GameTools.GenStatusByAttr(attribute);
            return (attribute.MoveMaxSpeed, attribute.MoveMinSpeed, attribute.MoveAddSpeed, attribute.MaxAmmo,
                    attribute.RecycleMulti,
                    baseAtkStatus, baseSurvivalStatus
                );
        }

        private void OpChangeAim(TwoDVector? aim)
        {
            CharacterBody.Sight.OpChangeAim(aim, GetNowScope());
        }

        public void ReloadInitData(SurvivalStatus survivalStatus, float maxMoveSpeed,
                float addMoveSpeed, float minMoveSpeed)
            //todo use attr_id
        {
            CharacterBody = null!;
            MaxMoveSpeed = maxMoveSpeed;
            PauseTick = 0;
            LockingWho = null;
            CatchingWho = null;
            NowWeapon = 0;
            NowCastAct = null;
            NextSkill = null;
            StunBuff = null;
            PlayingBuffs = new List<IPlayingBuff>();
            SurvivalStatus = survivalStatus;
            NowProtectTick = 0;
            AddMoveSpeed = addMoveSpeed;
            MinMoveSpeed = minMoveSpeed;
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
            if (NowInSnipeAct == null || NowSnipeStep <= 0 || !GetWeapons().TryGetValue(NowWeapon, out var weapon))
                return NowVehicle?.Scope ?? null;
            var weaponZoomStepScope = weapon.ZoomStepScopes[NowSnipeStep - 1];
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
            NowCastAct = skill;
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
            return NowInSnipeAct != null && GetWeapons().TryGetValue(NowWeapon, out var weapon) &&
                   weapon.Snipes.TryGetValue(NowInSnipeAct.Value, out var snipe)
                ? snipe
                : null;
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
                    !weapon.Snipes.TryGetValue(snipeAction, out var snipe) || snipe.TrickTick >= SnipeCallStack) return;

                NowInSnipeAct = snipeAction;
                SnipeCallStack = 0;
                OnSnipe(snipe);
            }
            else
            {
                OffSnipe();
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
        private void OffSnipe()
        {
            if (NowInSnipeAct != null && GetWeapons().TryGetValue(NowWeapon, out var weapon) &&
                weapon.Snipes.TryGetValue(NowInSnipeAct.Value, out var snipe))
            {
                NowSnipeStep = Math.Max(0, NowSnipeStep - snipe.OffStepPerTick);
                if (NowSnipeStep == 0)
                {
                    NowInSnipeAct = null;
                }
            }
            else
            {
                NowSnipeStep = 0;
            }
        }

        //重置
        public void ResetSnipe()
        {
            SnipeOnCallAct = null;

            SnipeCallStack = 0;

            NowInSnipeAct = null;

            NowSnipeStep = 0;
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
            if (selfEffectRegenerationBase != null)
            {
                //todo:: passive and talent
                SurvivalStatus.GetRegen(selfEffectRegenerationBase.Value);
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
            var f = GetNowSnipe()?.MoveSpeedMulti[CharacterBody.GetSize()];
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
                OffSnipe();
            }

            var selfEffectFilter = SelfEffectFilter(bullet);

            if (getThing == null)
            {
                return new CharGoTickResult(move: move, launchBullet: selfEffectFilter);
            }

            var dropThings = getThing.ActWhichChar(this, interactive);


            return new CharGoTickResult(move: move, launchBullet: selfEffectFilter,
                dropThing: dropThings.ToList(),
                getThing: getThing.InWhichMapInteractive);
        }


        public void RecycleAProp(Prop prop)
        {
            NowPropStack = Math.Min(MaxPropStack, (int) (NowPropStack + prop.RecyclePropStack * (1 + RecycleMulti)));
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
            vehicle.WhoDriveOrCanDrive = this;
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
        }

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
                NowProtectTick = TempConfig.ProtectTick;
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
                var (isBroken, destroyBullet) = NowVehicle.GoATickCheckSurvival();
                if (isBroken)
                {
                    if (operate?.SpecialAction == SpecialAction.OutVehicle)
                    {
                        return OutNowVehicle();
                    }

                    if (destroyBullet == null)
                        return new CharGoTickResult();


                    NowVehicle = null;
                    return new CharGoTickResult(launchBullet: destroyBullet);
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
                    case MapInteract.ApplyCall:
                        return new CharGoTickResult(mapInteractiveAbout: (MapInteract.ApplyCall,
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
                    case MapInteract.BuyCall:
                        return CallLongTouch(MapInteract.BuyCall)
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.BuyCall,
                                CharacterBody))
                            : new CharGoTickResult();
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ResetLongInterAct();

            // 有瞄准请求
            var snipeAction = operate.GetSnipe();
            if (snipeAction != null)
            {
                CallSnipe(snipeAction.Value);
            }
            else
            {
                SnipeCallStack = 0;
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
                            NowCastAct = Prop;
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
            var multiSpeed = NowMoveSpeed * nowSnipe?.MoveSpeedMulti[CharacterBody.GetSize()] ?? NowMoveSpeed;
            var dVector = twoDVector.Multi(multiSpeed);

            return new CharGoTickResult(move: dVector);
        }

        private CharGoTickResult OutNowVehicle()
        {
            if (NowVehicle == null) return new CharGoTickResult();
            {
                NowVehicle.OutAct.Launch(0, 0);
                NowCastAct = NowVehicle.OutAct;
                NowVehicle.WhoDriveOrCanDrive = null;

                var genIMapInteractable = NowVehicle.DropAsIMapInteractable(GetPos());
                NowVehicle = null;
                return new CharGoTickResult(dropThing: new List<IMapInteractable> {genIMapInteractable});
            }
        }

        private void DestroyVehicle()
        {
            NowVehicle?.StartDestroy();
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
                (dot + TempConfig.DecreaseMinCos) / (TempConfig.DecreaseMinCos + TempConfig.NormalSpeedMinCos)
            ));
            var moveDecreaseMinMulti = TempConfig.MoveDecreaseMinMulti +
                                       (1f - TempConfig.MoveDecreaseMinMulti) * normalSpeedMinCos;
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

        public void AddProtect(int protectValueAdd)
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
            MaxMoveSpeed = moveAddSpeed * (1f + add / (add + 1f));

            var lossP = MaxPropStack - NowPropStack;

            MaxPropStack = (int) (TempConfig.StandardPropMaxStack * (1f + otherAttrPassiveEffects[3]));
            NowPropStack = MaxPropStack - lossP;
            RecycleMulti = recycleMulti * (1f + otherAttrPassiveEffects[4]);
        }

        public bool LoadInteraction(Interaction charAct)
        {
            var b = NowCastAct == null;
            if (!b) return b;
            charAct.Launch();
            NowCastAct = charAct;
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

        private Vector<float>? GetPassiveEffects<T>() where T : IPassiveTraitEffect
        {
            var passiveTraits = Traits.Values.Where(x => x.PassiveTraitEffect is T);
            var passiveTraitEffects = passiveTraits.Select(x => x.PassiveTraitEffect.GenEffect(x.Level));
            var traitEffects = passiveTraitEffects.ToList();
            var firstOrDefault = traitEffects.FirstOrDefault();
            var z = firstOrDefault?.GetZero();
            var r = traitEffects.Aggregate(z, (s, x) => s + x.GetVector());
            return r;
        }


        public void SurvivalStatusRefresh(Vector<float> survivalAboutPassiveEffects)
        {
            BattleUnitStandard.SurvivalStatusRefresh(survivalAboutPassiveEffects, this);
        }


        public void PickAPassive(PassiveTrait passiveTrait)
        {
            var passiveTraitPassId = passiveTrait.PassId;
            if (Traits.TryGetValue(passiveTraitPassId, out var trait))
            {
                trait.AddLevel(passiveTrait.Level);
            }
            else
            {
                Traits.Add(passiveTraitPassId, passiveTrait);
            }

            switch (passiveTrait.PassiveTraitEffect)
            {
                case OtherAttrPassiveEffect _:
                    var ammoPassiveEffects =
                        GetPassiveEffects<OtherAttrPassiveEffect>() ?? OtherAttrPassiveEffect.Zero();
                    OtherStatusRefresh(ammoPassiveEffects);
                    NowVehicle?.OtherStatusRefresh(ammoPassiveEffects);


                    break;
                case AtkAboutPassiveEffect _:
                    var atkAboutPassiveEffects =
                        GetPassiveEffects<AtkAboutPassiveEffect>() ?? AtkAboutPassiveEffect.Zero();
                    AttackStatusRefresh(atkAboutPassiveEffects);
                    NowVehicle?.AttackStatusRefresh(atkAboutPassiveEffects);
                    break;
                case SurvivalAboutPassiveEffect _:
                    var survivalAboutPassiveEffects =
                        GetPassiveEffects<SurvivalAboutPassiveEffect>() ??
                        SurvivalAboutPassiveEffect.Zero();
                    SurvivalStatusRefresh(survivalAboutPassiveEffects);
                    NowVehicle?.SurvivalStatusRefresh(survivalAboutPassiveEffects);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void RecyclePassive(PassiveTrait passiveTrait)
        {
            if (!TempConfig.Configs.passives.TryGetValue(passiveTrait.PassId, out var passive)) return;
            var passiveRecycleMoney =
                passive.recycle_money.Select(x => new GameItem(x.item, (uint) (x.num * (1 + this.RecycleMulti))));
            foreach (var gameItem in passiveRecycleMoney)
            {
                PlayingItemBag.Gain(gameItem);
            }
        }

        public void BaseBeHitByBulletChange(TwoDPoint pos, int protectValueAdd, IBattleUnitStatus bodyCaster,
            float damageMulti, bool back)
        {
            ResetSpeed();
            ResetSnipe();
            ResetCastAct();


            IsBeHitBySomeOne =
                TwoDVector.TwoDVectorByPt(GetPos(), pos);


            if (CatchingWho != null)
            {
                CatchingWho.StunBuff = TempConfig.OutCaught;
                CatchingWho = null;
            }


            AddProtect(protectValueAdd);
            SurvivalStatus.TakeDamage(bodyCaster.GenDamage(damageMulti, back));
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
            trap.NotOverFlow = false;
            Traps.Enqueue(genATrap);
        }
    }
}