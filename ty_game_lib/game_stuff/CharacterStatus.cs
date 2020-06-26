using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        P5,
        Switch
    }

    public class CharacterStatus
    {
        public CharacterBody CharacterBody;

        private readonly float _maxMoveSpeed;

        private readonly float _minMoveSpeed;

        private readonly float _addMoveSpeed;

        public float NowMoveSpeed;

        public readonly int GId;

        public int PauseTick;

        public CharacterStatus? LockingWho;

        public CharacterStatus? CatchingWho;

        public int NowWeapon;

        public Dictionary<int, Weapon> Weapons { get; }

        //
        public Skill? NowCastSkill { get; set; }

        public (TwoDVector? Aim, Skill skill, bool is_switch)? NextSkill { get; set; }

        

        public IAntiActBuff? AntiActBuff;


        public List<DamageBuff> DamageBuffs;

        public DamageHealStatus DamageHealStatus;

        public int ProtectTick;

        public CharacterStatus(float maxMoveSpeed, int gId, int pauseTick, CharacterStatus? lockingWho,
            CharacterStatus? catchingWho, Dictionary<int, Weapon> weapons,
            DamageHealStatus damageHealStatus, int protectTick, float addMoveSpeed, float minMoveSpeed)
        {
            CharacterBody = null!;
            _maxMoveSpeed = maxMoveSpeed;
            GId = gId;
            PauseTick = pauseTick;
            LockingWho = lockingWho;
            CatchingWho = catchingWho;
            NowWeapon = 0;
            Weapons = weapons;
            NowCastSkill = null;
            NextSkill = null;
            AntiActBuff = null;
            DamageBuffs = new List<DamageBuff>();
            DamageHealStatus = damageHealStatus;
            ProtectTick = protectTick;
            _addMoveSpeed = addMoveSpeed;
            _minMoveSpeed = minMoveSpeed;
            NowMoveSpeed = 0f;
        }

        private void LoadSkill(TwoDVector? aim, Skill skill)
        {
            if (aim != null)
            {
                CharacterBody.Sight.OpChangeAim(aim);
            }

            skill.LaunchSkill(LockingWho != null);
            NowCastSkill = skill;
        }

        private (TwoDVector? move, Bullet? launchBullet) ActNowSkillATick()
        {
            if (NowCastSkill == null)
            {
                return (null, null);
            }

            return NowCastSkill
                .GoATick(GetPos(), CharacterBody.Sight.Aim, this, LockingWho?.GetPos());
        }

        private void ComboByNext()
        {
            if (NextSkill == null || NowCastSkill == null ||
                NowCastSkill.InWhichPeriod() != Skill.SkillPeriod.CanCombo) return;
            if (NextSkill.Value.is_switch)
            {
                NowWeapon = (NowWeapon + 1) % Weapons.Count;
            }

            var aim = NextSkill.Value.Aim;
            LoadSkill(aim, NextSkill.Value.skill);
            NextSkill = null;
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

            if (NowCastSkill?.InWhichPeriod() == Skill.SkillPeriod.End) NowCastSkill = null;

            if (NowCastSkill != null)
            {
                var (move, launchBullet) = ActNowSkillATick();
                ComboByNext();
                if (operate?.Action == null) return (move, launchBullet);

                var weaponSkillStatus = NowCastSkill.ComboInputRes();

                if (weaponSkillStatus != null)
                {
                    var status = weaponSkillStatus.Value;
                    var b = operate.Action == SkillAction.Switch;
                    var toUse = NowWeapon;
                    if (b)
                    {
                        status = WeaponSkillStatus.Switch;
                        toUse = (toUse + 1) % Weapons.Count;
                    }

                    var nowWeapon = Weapons.TryGetValue(toUse, out var weapon)
                        ? weapon
                        : Weapons.First().Value;

                    var operateAction = operate.Action.Value;


                    if (!nowWeapon.SkillGroups.TryGetValue(operateAction, out var skills) ||
                        !skills.TryGetValue(status, out var skill)) return (move, launchBullet);
                    switch (NowCastSkill.InWhichPeriod())
                    {
                        case Skill.SkillPeriod.Casting:
                            NextSkill = (operate.Aim, skill, b);
                            break;
                        case Skill.SkillPeriod.CanCombo:
                            LoadSkill(operate.Aim, skill);
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
            }

            if (operate == null)
            {
                return (null, null);
            }


            if (operate.Aim != null)
            {
                CharacterBody.Sight.OpChangeAim(operate.Aim);
            }

            if (operate.Action == null) return (null, null);
            {
                if (operate.Action.Value == SkillAction.Switch) NowWeapon = (NowWeapon + 1) % Weapons.Count;
                else
                {
                    if (!Weapons.TryGetValue(NowWeapon, out var weapon) ||
                        !weapon.SkillGroups.TryGetValue(operate.Action.Value, out var value) ||
                        !value.TryGetValue(WeaponSkillStatus.Normal, out var skill)) return (null, null);
                    LoadSkill(null, skill);
                    var actNowSkillATick = ActNowSkillATick();
                    return actNowSkillATick;
                }
            }


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