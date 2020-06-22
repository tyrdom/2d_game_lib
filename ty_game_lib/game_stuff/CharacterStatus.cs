﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using collision_and_rigid;

namespace game_stuff
{
    public enum WeaponSkillStatus
    {
        Normal,
        Catching,
        P1,
        P2,
        P3,
        P4,
        P5
    }

    public class CharacterStatus
    {
        public CharacterBody CharacterBody;

        private readonly float _maxMoveSpeed;
        public readonly float MinMoveSpeed;

        public readonly float AddMoveSpeed;

        public float NowMoveSpeed;

        public int GId;

        public int PauseTick;

        public CharacterStatus? LockingWho;

        public CharacterStatus? CatchingWho;

        public int NowWeapon;

        public Dictionary<int, Weapon> WeaponConfigs;

        //
        public Queue<Skill> NowCastSkillQueue;


        public int NowTough;

        public IAntiActBuff? AntiActBuff;


        public List<DamageBuff> DamageBuffs;

        public DamageHealStatus DamageHealStatus;

        public int ProtectTick;

        public CharacterStatus(float maxMoveSpeed, int gId, int pauseTick, CharacterStatus? lockingWho,
            CharacterStatus? catchingWho, int nowWeapon, Dictionary<int, Weapon> weaponConfigs,
            int nowTough,
            DamageHealStatus damageHealStatus, int protectTick, float addMoveSpeed, float minMoveSpeed)
        {
            CharacterBody = null!;
            _maxMoveSpeed = maxMoveSpeed;
            GId = gId;
            PauseTick = pauseTick;
            LockingWho = lockingWho;
            CatchingWho = catchingWho;
            NowWeapon = nowWeapon;
            WeaponConfigs = weaponConfigs;
            NowCastSkillQueue = new Queue<Skill>();

            NowTough = nowTough;
            AntiActBuff = null;
            DamageBuffs = new List<DamageBuff>();
            DamageHealStatus = damageHealStatus;
            ProtectTick = protectTick;
            AddMoveSpeed = addMoveSpeed;
            MinMoveSpeed = minMoveSpeed;
            NowMoveSpeed = 0f;
        }


        public (ITwoDTwoP?, Bullet?) CharGoTick(Operate? operate)
        {
            if (PauseTick > 0)
            {
                PauseTick -= 1;
                return (null, null);
            }

            if (AntiActBuff != null)
            {
                var (twoDPoint, antiActBuff) = AntiActBuff.GoTickDrivePos(GetPos());
                AntiActBuff = antiActBuff;
                return (twoDPoint, null);
            }

            if (NowCastSkillQueue.Count > 0)
            {
                var (twoDVector, bullet, item3) =
                    NowCastSkillQueue.First().GoATick(GetPos(), CharacterBody.Sight.Aim, this, LockingWho?.GetPos());
                if (item3)
                {
                    NowCastSkillQueue.Dequeue();
                }
                return (twoDVector, bullet);
            }

            if (operate == null)
            {
                return (null, null);
            }

            if (operate.Aim != null)
            {
                CharacterBody.Sight.OpChangeAim(operate.Aim);
            }

            // switch (operate.Action)
            // {
            //     case SkillAction.A1:
            //         if (WeaponConfigs.TryGetValue(NowWeapon, out var w))
            //         {
            //             var valueSkillGroup1
            //                 = w.SkillGroup1;
            //             if (valueSkillGroup1.TryGetValue(Combo.GetWStatus(), out var skillConfig))
            //             {
            //                 var genSkill = skillConfig.LaunchSkill(LockingWho != null);
            //                 Combo.WeaponSkillStatus = skillConfig.NextCombo;
            //                 NowCastSkillQueue.Enqueue(genSkill);
            //                 return CharGoTick(null);
            //             }
            //         }
            //
            //         break;
            //     case SkillAction.A2:
            //         if (WeaponConfigs.TryGetValue(NowWeapon, out var w1))
            //         {
            //             var valueSkillGroup2
            //                 = w1.SkillGroup2;
            //             if (valueSkillGroup2.TryGetValue(Combo.GetWStatus(), out var skill))
            //             {
            //                 var genSkill = skill.LaunchSkill(LockingWho != null);
            //                 Combo.WeaponSkillStatus = skill.NextCombo;
            //                 NowCastSkillQueue = genSkill;
            //                 return CharGoTick(null);
            //             }
            //         }
            //
            //         break;
            //     case SkillAction.A3:
            //         NowWeapon = (NowWeapon + 1) % TempConfig.WeaponNum;
            //         if (WeaponConfigs.TryGetValue(NowWeapon, out var w2))
            //         {
            //             var valueSkillGroup2
            //                 = w2.SkillGroup3;
            //             if (valueSkillGroup2.TryGetValue(Combo.GetWStatus(), out var skillConfig))
            //             {
            //                 var genSkill = skillConfig.LaunchSkill(LockingWho != null);
            //                 Combo.WeaponSkillStatus = skillConfig.NextCombo;
            //                 NowCastSkillQueue = genSkill;
            //                 return CharGoTick(null);
            //             }
            //         }
            //
            //         break;
            //     case null:
            //         var twoDVector = operate.Move?.Multi(_maxMoveSpeed);
            //         return (twoDVector, null);
            //
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }

            return (null, null);
        }

        public TwoDPoint GetPos()
        {
            return CharacterBody.NowPos;
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


    public class Combo
    {
        public WeaponSkillStatus WeaponSkillStatus;

        public int? ComboTick;

        public Combo(WeaponSkillStatus weaponSkillStatus, int? comboTick)
        {
            WeaponSkillStatus = weaponSkillStatus;
            ComboTick = comboTick;
        }

        public static Combo NewZeroCombo()
        {
            return new Combo(WeaponSkillStatus.Normal, null);
        }

        public void Reset()
        {
            WeaponSkillStatus = WeaponSkillStatus.Normal;
            ComboTick = null;
        }

        public WeaponSkillStatus GetWStatus()
        {
            if (ComboTick == null || ComboTick <= 0)
            {
                return WeaponSkillStatus.Normal;
            }

            return WeaponSkillStatus;
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