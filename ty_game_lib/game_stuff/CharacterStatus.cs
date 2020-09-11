﻿using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Security.Principal;
using collision_and_rigid;

namespace game_stuff
{
    public class CharacterStatus
    {
        public CharacterBody CharacterBody;

        private float MaxMoveSpeed { get; set; }

        private float MinMoveSpeed { get; set; }

        private float AddMoveSpeed { get; set; }

        public float NowMoveSpeed;

        public readonly int GId;

        public int PauseTick;

        public CharacterStatus? LockingWho;

        public CharacterStatus? CatchingWho;

        public int NowWeapon;

        public Dictionary<int, Weapon> Weapons { get; private set; }

        //

        public Skill? NowCastSkill { get; set; }

        public (TwoDVector? Aim, Skill skill, SkillAction opAction)? NextSkill { get; set; }


        public IAntiActBuff? AntiActBuff;


        public List<DamageBuff> DamageBuffs;

        public DamageHealStatus DamageHealStatus;

        public int ProtectTick;


        // for_tick_msg
        public SkillAction? SkillLaunch { get; private set; }

        public bool IsPause { get; private set; }

        public TwoDVector? IsBeHitBySomeOne { get; set; }


        public bool IsHitSome { get; set; }

        public CharacterStatus(float maxMoveSpeed, int gId,
            DamageHealStatus damageHealStatus, float addMoveSpeed, float minMoveSpeed)
        {
            CharacterBody = null!;
            MaxMoveSpeed = maxMoveSpeed;
            GId = gId;
            PauseTick = 0;
            LockingWho = null;
            CatchingWho = null;
            NowWeapon = 0;
            Weapons = new Dictionary<int, Weapon>();
            NowCastSkill = null;
            NextSkill = null;
            AntiActBuff = null;
            DamageBuffs = new List<DamageBuff>();
            DamageHealStatus = damageHealStatus;
            ProtectTick = 0;
            AddMoveSpeed = addMoveSpeed;
            MinMoveSpeed = minMoveSpeed;
            NowMoveSpeed = 0f;
            SkillLaunch = null;
            IsPause = false;
            IsBeHitBySomeOne = null;
            IsHitSome = false;
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

            NowCastSkill = null;
            NextSkill = null;
            AntiActBuff = null;
            DamageBuffs = new List<DamageBuff>();
            DamageHealStatus = damageHealStatus;
            ProtectTick = 0;
            AddMoveSpeed = addMoveSpeed;
            MinMoveSpeed = minMoveSpeed;
            NowMoveSpeed = 0f;
            SkillLaunch = null;
            IsPause = false;
            IsBeHitBySomeOne = null;
            IsHitSome = false;
        }

        public void LoadSkill(TwoDVector? aim, Skill skill, SkillAction skillAction)
        {
            //装载技能时，重置速度和锁定角色
            ResetSpeed();
            LockingWho = null;
            if (aim != null)
            {
                CharacterBody.Sight.OpChangeAim(aim);
            }

            skill.LaunchSkill();
            SkillLaunch = skillAction;
            NowCastSkill = skill;
        }

        public void ResetSpeed()
        {
            NowMoveSpeed = MinMoveSpeed;
        }

        private (TwoDVector? move, IHitStuff? launchBullet) ActNowSkillATick()
        {
            if (NowCastSkill == null)
            {
                return (null, null);
            }

            //在有锁定目标时，会根据与当前目标的向量调整，有一定程度防止穿模型
            var limitV
                = LockingWho == null
                    ? null
                    : TwoDVector.TwoDVectorByPt(GetPos(), LockingWho.GetPos())
                        .ClockwiseTurn(CharacterBody.Sight.Aim)
                        .AddX(-CharacterBody.GetRr() - LockingWho.CharacterBody.GetRr());
#if DEBUG
            var lockingWhoGId = LockingWho == null ? "null" : LockingWho.GId.ToString();
            Console.Out.WriteLine($"skill lock {lockingWhoGId} limitV ::{limitV?.ToString()}");
#endif
            if (limitV != null)
            {
                limitV.X = MathTools.Max(0, limitV.X);
            }

            return NowCastSkill
                .GoATick(GetPos(), CharacterBody.Sight.Aim, limitV);
        }

        private void ComboByNext(TwoDVector? operateAim)
        {
            if (NextSkill == null || NowCastSkill == null ||
                NowCastSkill.InWhichPeriod() != Skill.SkillPeriod.CanCombo)
            {
                CharacterBody.Sight.OpChangeAim(null);
                return;
            }
#if DEBUG
            Console.Out.WriteLine($"{GId} ::skill next start {NextSkill.Value.skill._nowOnTick}");
#endif
            if (NextSkill.Value.opAction == SkillAction.Switch)
            {
                NowWeapon = (NowWeapon + 1) % Weapons.Count;
            }

            var aim = operateAim ?? NextSkill.Value.Aim;

            LoadSkill(aim, NextSkill.Value.skill, NextSkill.Value.opAction);

            NextSkill = null;
        }

        public (ITwoDTwoP?, IHitStuff?) CharGoTick(Operate? operate) //角色一个tick行为
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

                return (null, null);
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
                return (twoDPoint, null);
            }

            // 当前技能结束检查
            if (NowCastSkill?.InWhichPeriod() == Skill.SkillPeriod.End) NowCastSkill = null;

            // 当前技能的释放时候
            var opAction = operate?.GetAction();
            if (NowCastSkill != null)
            {
                // 技能进行一个tick

                var (move, launchBullet) = ActNowSkillATick();
#if DEBUG
                Console.Out.WriteLine($"{GId} skill on {NowCastSkill._nowOnTick}");
                Console.Out.WriteLine($"skill move {move}");
                if (launchBullet != null)
                    Console.Out.WriteLine(
                        $"launch IHitAble::{launchBullet.GetType()}::{launchBullet.Aim}||{launchBullet.Pos}");
#endif

                var skillAim = operate?.Aim ?? operate?.Move; // 检查下一个连续技能，如果有连续技能可以切换，则切换到下一个技能,NextSkill为null
                ComboByNext(skillAim);

                //没有更多Act操作，则返回
                if (opAction == null) return (move, launchBullet);

                //有Act操作，则检测出下一个状态id

                var weaponSkillStatus = NowCastSkill.ComboInputRes();

                // 状态可用，则执行连技操作
                if (weaponSkillStatus == null) return (move, launchBullet);
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

                if (!nowWeapon.SkillGroups.TryGetValue(opAction.Value, out var skills) ||
                    !skills.TryGetValue(status, out var skill)) return (move, launchBullet);

                switch (NowCastSkill.InWhichPeriod())
                {
                    case Skill.SkillPeriod.Casting:

                        NextSkill ??= (skillAim, skill, opAction.Value);

                        break;
                    case Skill.SkillPeriod.CanCombo:
                        LoadSkill(skillAim, skill, opAction.Value);
                        NowWeapon = toUse;
                        break;
                    case Skill.SkillPeriod.End:
                        NowWeapon = toUse;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return (move, launchBullet);
            }

            // 没有技能在释放
            // 没有任何操作
            if (operate == null)
            {
                ResetSpeed();
                CharacterBody.Sight.OpChangeAim(null);
                return (null, null);
            }

            // 有操作
            // 转换视野方向
            if (operate.Aim != null)
            {
                CharacterBody.Sight.OpChangeAim(operate.Aim);
            }

            if (opAction != null)
            {
#if DEBUG
                Console.Out.WriteLine($"skill start {opAction.ToString()}");
#endif

                // 非连击状态瞬间切换
                if (opAction == SkillAction.Switch) NowWeapon = (NowWeapon + 1) % Weapons.Count;
                // 发动当前武器技能组的起始技能0
                else
                {
                    if (!Weapons.TryGetValue(NowWeapon, out var weapon) ||
                        !weapon.SkillGroups.TryGetValue(opAction.Value, out var value) ||
                        !value.TryGetValue(0, out var skill)) return (null, null);
                    LoadSkill(null, skill, opAction.Value);
                    var actNowSkillATick = ActNowSkillATick();
                    return actNowSkillATick;
                }
            }

            //其他移动操作
            var twoDVector = operate.GetMove();

            if (twoDVector == null)
            {
                ResetSpeed();
                return (null, null);
            }

            // 加速跑步运动，有上限
            if (operate.Aim == null)
            {
                CharacterBody.Sight.OpChangeAim(twoDVector);
            }


            var dot = twoDVector.Dot(CharacterBody.Sight.Aim);
            var normalSpeedMinCos = MathTools.Max(0f, MathTools.Min(1f,
                (dot + TempConfig.DecreaseMinCos) / (TempConfig.DecreaseMinCos + TempConfig.NormalSpeedMinCos)
            ));
            var moveDecreaseMinMulti = TempConfig.MoveDecreaseMinMulti +
                                       (1f - TempConfig.MoveDecreaseMinMulti) * normalSpeedMinCos;
            var maxMoveSpeed = MaxMoveSpeed * moveDecreaseMinMulti;
            NowMoveSpeed = MathTools.Max(MinMoveSpeed, MathTools.Min(maxMoveSpeed, NowMoveSpeed + AddMoveSpeed));
            var dVector = twoDVector.Multi(NowMoveSpeed);

            return (dVector, null);
        }

        public TwoDPoint GetPos()
        {
            return CharacterBody.NowPos;
        }

        public TwoDVector GetAim()
        {
            return CharacterBody.Sight.Aim;
        }
    }

    public class DamageHealStatus
    {
        private int MaxHp;
        private int NowHp;

        public DamageHealStatus(int maxHp, int nowHp)
        {
            MaxHp = maxHp;
            NowHp = nowHp;
        }

        public static DamageHealStatus StartDamageHealAbout()
        {
            return new DamageHealStatus(TempConfig.StartHp, TempConfig.StartHp);
        }

        public void TakeDamage(Damage damage)
        {
            NowHp -= damage.DamageValue;
        }

        public void GetHeal(Heal heal)
        {
        }
    }


    public class Heal
    {
        private int HealValue;
    }


    public class DamageBuff
    {
    }
}