﻿using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Security.Principal;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    //(TwoDVector? move, IHitStuff? launchBullet, IMapInteractive? dropThing)

    public class CharacterStatus
    {
        public CharacterBody CharacterBody;

        private float MaxMoveSpeed { get; set; }

        private float MinMoveSpeed { get; set; }

        private float AddMoveSpeed { get; set; }

        //move status
        public float NowMoveSpeed { get; private set; }

        public int GId { get; }

        //Snipe Status

        private SnipeAction? SnipeOnCallAct { get; set; }

        private int SnipeCallStack { get; set; }

        private SnipeAction? NowInSnipeAct { get; set; }

        private int NowSnipeStep { get; set; }


        public int NowAmmo
        {
            get => NowVehicle?.NowAmmo ?? NowAmmo;
            set => NowAmmo = value;
        }

        public int MaxAmmo { get; }
        public CharacterStatus? LockingWho { get; set; }

        public CharacterStatus? CatchingWho { get; set; }

        public int NowWeapon { get; private set; }

        public Dictionary<int, Weapon> Weapons
        {
            get => NowVehicle == null ? Weapons : NowVehicle.Weapons;
            private set => Weapons = value ?? throw new ArgumentNullException(nameof(value));
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

        //Recycle status

        private uint NowRecycleStack { get; set; }

        private uint MaxRecycleStack { get; }

        //be hit status
        public int PauseTick { get; set; }

        public IAntiActBuff? AntiActBuff { get; set; }
        private int NowProtectValue { get; set; }
        private int MaxProtectValue { get; }
        public List<DamageBuff> DamageBuffs { get; set; }

        public DamageHealStatus DamageHealStatus;
        public int NowProtectTick { get; private set; }

        // for_tick_msg
        public SkillAction? SkillLaunch { get; private set; }
        public bool IsPause { get; private set; }
        public TwoDVector? IsBeHitBySomeOne { get; set; }
        public bool IsHitSome { get; set; }


        public CharacterStatus(float maxMoveSpeed, int gId,
            DamageHealStatus damageHealStatus, float addMoveSpeed, float minMoveSpeed, int maxProtectValue)
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
            DamageBuffs = new List<DamageBuff>();
            DamageHealStatus = damageHealStatus;
            NowProtectTick = 0;
            AddMoveSpeed = addMoveSpeed;
            MinMoveSpeed = minMoveSpeed;
            MaxProtectValue = maxProtectValue;
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
            MaxRecycleStack = TempConfig.GetTickByTime(TempConfig.MaxRecycleTime);
            NowRecycleStack = 0;
        }

        private void OpChangeAim(TwoDVector? aim)
        {
            CharacterBody.Sight.OpChangeAim(aim, GetNowScope());
        }

        public void ReloadInitData(DamageHealStatus damageHealStatus, float maxMoveSpeed,
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
            DamageBuffs = new List<DamageBuff>();
            DamageHealStatus = damageHealStatus;
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
            if (NowInSnipeAct == null || NowSnipeStep <= 0 || !Weapons.TryGetValue(NowWeapon, out var weapon))
                return null;
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

            if (!skill.Launch(NowSnipeStep, NowAmmo)) return;
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
            return NowInSnipeAct != null && Weapons.TryGetValue(NowWeapon, out var weapon) &&
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

                if (!Weapons.TryGetValue(NowWeapon, out var weapon) ||
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
            if (NowInSnipeAct != null && Weapons.TryGetValue(NowWeapon, out var weapon) &&
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
            return NowCastAct switch
            {
                null => new CharGoTickResult(null, null, null, null),
                Prop prop => GoNowActATick(prop, moveOp),
                Skill skill => GoNowActATick(skill, moveOp),
                _ => throw new ArgumentOutOfRangeException(nameof(NowCastAct))
            };
        }


        public void HitBySelfBullet(Bullet bullet)
        {
            //todo
        }

        private CharGoTickResult GoNowActATick(ICharAct skill,
            TwoDVector? moveOp)
        {
            var limitV = skill switch
            {
                Interaction _ => null,
                Prop _ => null,
                Skill _ => LockingWho == null
                    ? null
                    : TwoDVector.TwoDVectorByPt(GetPos(), LockingWho.GetPos())
                        .ClockwiseTurn(CharacterBody.Sight.Aim)
                        .AddX(-CharacterBody.GetRr() - LockingWho.CharacterBody.GetRr()),
                _ => throw new ArgumentOutOfRangeException(nameof(skill))
            };
            var f = GetNowSnipe()?.MoveSpeedMulti[CharacterBody.BodySize];
            var fixMove = skill switch
            {
                Interaction _ => null,
                Prop _ => moveOp?.Multi(GetStandardSpeed(moveOp)),
                Skill _ => f == null
                    ? moveOp?.Multi(GetStandardSpeed(moveOp))
                    : moveOp?.Multi(GetStandardSpeed(moveOp))
                        .Multi(f.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(skill))
            };
            //在有锁定目标时，会根据与当前目标的向量调整，有一定程度防止穿模型

#if DEBUG
            var lockingWhoGId = LockingWho == null ? "null" : LockingWho.GId.ToString();
            Console.Out.WriteLine($"skill lock {lockingWhoGId} limitV ::{limitV}");
#endif
            if (limitV != null)
            {
                limitV.X = MathTools.Max(0, limitV.X);
            }

            var (move, bullet, snipeOff, inCage, interactive) = skill
                .GoATick(GetPos(), CharacterBody.Sight.Aim, fixMove, limitV);

            if (snipeOff)
            {
                OffSnipe();
            }

            IMapInteractable? mapInteractable = null;
            if (inCage == null)
                return new CharGoTickResult(move, bullet, mapInteractable, inCage?.InWhichMapInteractive);
            switch (inCage)
            {
                case Prop prop:
                    switch (interactive)
                    {
                        case MapInteractive.RecycleCall:
                            RecycleAProp(prop);
                            break;
                        case MapInteractive.PickOrInVehicle:
                            mapInteractable = PickAProp(prop);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case Vehicle vehicle:
                    GetInAVehicle(vehicle);
                    break;
                case Weapon weapon:
                    mapInteractable = PicAWeapon(weapon);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inCage));
            }

            return new CharGoTickResult(move, bullet, mapInteractable, inCage?.InWhichMapInteractive);
        }

        private void RecycleAProp(Prop prop)
        {
            NowPropStack = Math.Min(MaxPropStack, NowPropStack + prop.RecyclePropStack);
        }

        private IMapInteractable? PicAWeapon(Weapon weapon)
        {
            return weapon.PickedBySomebody(this);
        }

        private void GetInAVehicle(Vehicle vehicle)
        {
            if (NowVehicle != null) throw new Exception("have in a vehicle");
            NowVehicle = vehicle;
            vehicle.WhoDrive = this;
        }

        private IMapInteractable? PickAProp(Prop prop)
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
                NowWeapon = (NowWeapon + 1) % Weapons.Count;
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
                    toUse = (toUse + 1) % Weapons.Count;
                }

                var nowWeapon = Weapons.TryGetValue(toUse, out var weapon)
                    ? weapon
                    : Weapons.First().Value;

                if (!nowWeapon.SkillGroups.TryGetValue(CharacterBody.BodySize, out var immutableDictionary) ||
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
                return new CharGoTickResult(null, null, null, null);
            }

            // 有各种操作

            // 与地图上物品互动
            var mapInteractive = operate.GetMapInteractive();

            if (mapInteractive != null)
            {
                switch (mapInteractive)
                {
                    case MapInteractive.RecycleCall:
                        var callRecycle = CallRecycle();
                        return callRecycle
                            ? new CharGoTickResult(null, null, whoRecycleCageCall: this.CharacterBody)
                            : new CharGoTickResult(null, null);
                    case MapInteractive.PickOrInVehicle:
                        return new CharGoTickResult(null, null, whoPickCageCall: CharacterBody);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ResetReCycle();

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
                    NowWeapon = (NowWeapon + 1) % Weapons.Count;
                    SkillLaunch = SkillAction.Switch;
                }
                // 发动当前武器技能组的起始技能0
                else
                {
                    if (!Weapons.TryGetValue(NowWeapon, out var weapon) ||
                        !weapon.SkillGroups.TryGetValue(CharacterBody.BodySize, out var value1) ||
                        !value1.TryGetValue(opAction.Value, out var value) ||
                        !value.TryGetValue(0, out var skill)) return new CharGoTickResult(null, null, null, null);
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
                        if (Prop == null) return new CharGoTickResult(null, null);
                        if (Prop.Launch(NowPropStack))
                        {
                            NowPropStack -= Prop.StackCost;
                            NowCastAct = Prop;
                        }

                        break;
                    case SpecialAction.OutVehicle:
                        if (NowVehicle == null) return new CharGoTickResult(null, null);
                    {
                        NowVehicle.OutAct.Launch(0, 0);
                        NowCastAct = NowVehicle.OutAct;
                        NowVehicle.WhoDrive = null;
                        var genIMapInteractable = NowVehicle.GenIMapInteractable(GetPos());
                        return new CharGoTickResult(null, null, genIMapInteractable);
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
                return new CharGoTickResult(null, null);
            }

            // 加速跑步运动，有上限
            if (operate.Aim == null)
            {
                OpChangeAim(twoDVector);
            }

            NowMoveSpeed = GetStandardSpeed(twoDVector);
            var nowSnipe = GetNowSnipe();
            var multiSpeed = NowMoveSpeed * nowSnipe?.MoveSpeedMulti[CharacterBody.BodySize] ?? NowMoveSpeed;
            var dVector = twoDVector.Multi(multiSpeed);

            return new CharGoTickResult(dVector, null);
        }

        private void ResetReCycle()
        {
            NowRecycleStack = 0;
        }

        private bool CallRecycle()
        {
            NowRecycleStack += 1;
            var b = NowRecycleStack > MaxRecycleStack;
            if (b)
            {
                ResetReCycle();
            }

            return b;
        }


        // private float GetAndSetStandardSpeed()

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

        public static Damage GenDamage(float damageMulti)
        {
            return new Damage(0);
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
    }

    public class DamageBuff
    {
    }
}