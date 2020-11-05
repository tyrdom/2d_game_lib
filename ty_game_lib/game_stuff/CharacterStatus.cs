using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Security.Principal;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    //(TwoDVector? move, IHitStuff? launchBullet, IMapInteractive? dropThing)

    public class CharacterStatus : IBattleUnit
    {
        public CharacterBody CharacterBody;

        public float MaxMoveSpeed { get; set; }

        public float MinMoveSpeed { get; set; }

        public float AddMoveSpeed { get; set; }
        public base_attr_id BaseAttrId { get; }

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

        private int MaxAmmo { get; }
        public CharacterStatus? LockingWho { get; set; }

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

        public (TwoDVector? Aim, Skill skill, SkillAction opAction)? NextSkill { get; set; }

        //Prop
        private Prop? Prop { get; set; }

        private int NowPropStack { get; set; }

        private int MaxPropStack { get; }

        //Vehicle
        public Vehicle? NowVehicle { get; private set; }

        //InterAct CallLong status

        private MapInteract? NowMapInteractive { get; set; }
        private uint NowCallLongStack { get; set; }

        private uint MaxCallLongStack { get; }

        //be hit status
        public int PauseTick { get; set; }

        public IAntiActBuff? AntiActBuff { get; set; }

        //protect status
        private int NowProtectValue { get; set; }
        private int MaxProtectValue { get; }

        public int NowProtectTick { get; private set; }

        //Game other Status

        private Dictionary<uint, PassiveTrait> Traits { get; }
        private List<IPlayingBuff> PlayingBuffs { get; set; }

        public AttackStatus AttackStatus { get; }
        public SurvivalStatus SurvivalStatus { get; private set; }


        // for_tick_msg
        public SkillAction? SkillLaunch { get; private set; }
        public bool IsPause { get; private set; }
        public TwoDVector? IsBeHitBySomeOne { get; set; }
        public bool IsHitSome { get; set; }


        public CharacterStatus(float maxMoveSpeed, int gId,
            SurvivalStatus survivalStatus, float addMoveSpeed, float minMoveSpeed, int maxProtectValue,
            AttackStatus attackStatus, base_attr_id baseAttrId)
        {
            CharacterBody = null!;
            MaxMoveSpeed = maxMoveSpeed;
            GId = gId;
            PauseTick = 0;
            LockingWho = null;
            CatchingWho = null;
            NowWeapon = 0;
            Weapons = new Dictionary<int, Weapon>();
            NowCastAct = null;
            NextSkill = null;
            AntiActBuff = null;
            PlayingBuffs = new List<IPlayingBuff>();
            SurvivalStatus = survivalStatus;
            NowProtectTick = 0;
            AddMoveSpeed = addMoveSpeed;
            MinMoveSpeed = minMoveSpeed;
            MaxProtectValue = maxProtectValue;
            Traits = new Dictionary<uint, PassiveTrait>();
            AttackStatus = attackStatus;
            BaseAttrId = baseAttrId;
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
            MaxPropStack = TempConfig.StandardMaxStack;
            NowVehicle = null;
            NowAmmo = 0;
            MaxAmmo = TempConfig.StandardMaxAmmo;
            MaxCallLongStack = TempConfig.GetTickByTime(TempConfig.MaxRecycleTime);
            NowCallLongStack = 0;
            NowMapInteractive = null;
            MaxWeaponSlot = TempConfig.StandardWeaponNum;
        }

        private void OpChangeAim(TwoDVector? aim)
        {
            CharacterBody.Sight.OpChangeAim(aim, GetNowScope());
        }

        public void ReloadInitData(SurvivalStatus survivalStatus, float maxMoveSpeed,
            float addMoveSpeed, float minMoveSpeed)
        {
            CharacterBody = null!;
            MaxMoveSpeed = maxMoveSpeed;
            PauseTick = 0;
            LockingWho = null;
            CatchingWho = null;
            NowWeapon = 0;
            NowCastAct = null;
            NextSkill = null;
            AntiActBuff = null;
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

        public void LoadSkill(TwoDVector? aim, Skill skill, SkillAction skillAction)
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

        public void ResetSpeed()
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
                    if (aInteraction.InCage.InWhichMapInteractive != null)
                        aInteraction.InCage.InWhichMapInteractive.NowInterCharacterBody = null;
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
            if (actNowActATick.LaunchBullet != null)
                switch (actNowActATick.LaunchBullet)
                {
                    case null:
                        break;
                    case SelfEffect selfEffect:
                        HitBySelfEffect(selfEffect);
                        actNowActATick.LaunchBullet = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return actNowActATick;
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
                        .AddX(-CharacterBody.GetRr() - LockingWho.CharacterBody.GetRr())
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
            var lockingWhoGId = LockingWho == null ? "null" : LockingWho.GId.ToString();
            Console.Out.WriteLine($"skill lock {lockingWhoGId} limitV ::{limitV}");
#endif

            var (move, bullet, snipeOff, getThing, interactive) = charAct
                .GoATick(GetPos(), GetAim(), fixMove, limitV);
            if (snipeOff)
            {
                OffSnipe();
            }


            if (getThing == null)
                return new CharGoTickResult(move, bullet);

            var dropThings = getThing.ActWhichChar(this, interactive);


            return new CharGoTickResult(move, bullet, dropThings.ToList(), getThing?.InWhichMapInteractive);
        }

        public void RecycleAProp(Prop prop)
        {
            NowPropStack = Math.Min(MaxPropStack, NowPropStack + prop.RecyclePropStack);
        }

        public IMapInteractable? PicAWeapon(Weapon weapon)
        {
            return weapon.PickedBySomebody(this);
        }


        private List<IMapInteractable> DropWeapon(BodySize bodySize)
        {
            return GameTools.DropWeapon(Weapons, bodySize, GetPos());
        }

        public IEnumerable<IMapInteractable> GetInAVehicle(Vehicle vehicle)
        {
            if (NowVehicle != null) throw new Exception("have in a vehicle");
            var mapIntractable = DropWeapon(vehicle.Size);
            NowVehicle = vehicle;
            vehicle.WhoDrive = this;
            return mapIntractable;
        }

        public IMapInteractable? PickAProp(Prop prop)
        {
            if (Prop != null) return prop.GenIMapInteractable(GetPos());
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

        public CharGoTickResult
            CharGoTick(Operate? operate) //角色一个tick行为
        {
            //清理消息缓存
            if (SkillLaunch != null) SkillLaunch = null;
            if (IsBeHitBySomeOne != null) IsBeHitBySomeOne = null;
            if (IsHitSome) IsHitSome = false;

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
            if (AntiActBuff != null)
            {
#if DEBUG
                Console.Out.WriteLine(
                    $"{GId}  {AntiActBuff.GetType()} ::anti v::{AntiActBuff.GetItp().ToString()}::anti buff :: {AntiActBuff.RestTick}");
#endif
                var (twoDPoint, antiActBuff) = AntiActBuff.GoTickDrivePos(dPoint);
                AntiActBuff = antiActBuff;

#if DEBUG
                Console.Out.WriteLine(
                    $"{GId} {AntiActBuff?.GetType()}  ::IPt {twoDPoint.ToString()} ::anti buff :: {AntiActBuff?.RestTick}");
#endif
                return new CharGoTickResult(twoDPoint, null, null, null);
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
                        if (NowVehicle == null) return new CharGoTickResult();
                    {
                        NowVehicle.OutAct.Launch(0, 0);
                        NowCastAct = NowVehicle.OutAct;
                        NowVehicle.WhoDrive = null;
                        var genIMapInteractable = NowVehicle.GenIMapInteractable(GetPos());
                        return new CharGoTickResult(null, null, new List<IMapInteractable> {genIMapInteractable});
                    }


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

            return new CharGoTickResult(dVector);
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
            var maxMoveSpeed = MaxMoveSpeed * moveDecreaseMinMulti;
            var nowMoveSpeed = MathTools.Max(MinMoveSpeed, MathTools.Min(maxMoveSpeed, NowMoveSpeed + AddMoveSpeed));
            return nowMoveSpeed;
        }

        public TwoDPoint GetPos()
        {
            return CharacterBody.NowPos;
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

        public bool LoadInteraction(Interaction charAct)
        {
            var b = NowCastAct == null;
            if (!b) return b;
            charAct.Launch();
            NowCastAct = charAct;
            return b;
        }

        public void RecycleWeapon(Weapon weapon)
        {
            throw new NotImplementedException();
        }

        public void AddPlayingBuff(IEnumerable<IPlayingBuff> playingBuffs)
        {
            PlayBuffStandard.AddBuffs(this.PlayingBuffs, playingBuffs);
        }
    }
}