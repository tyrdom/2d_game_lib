using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class CharacterStatus
    {
        public CharacterBody CharacterBody;

        private readonly float _maxMoveSpeed;

        private readonly float _minMoveSpeed;

        private readonly float _addMoveSpeed;

        public float NowMoveSpeed;

        // OpBuffer
        public Operate? Op1;

        public Operate? Op2;
        //
        
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

        public CharacterStatus(float maxMoveSpeed, int gId, int pauseTick, Dictionary<int, Weapon> weapons,
            DamageHealStatus damageHealStatus, int protectTick, float addMoveSpeed, float minMoveSpeed)
        {
            CharacterBody = null!;
            _maxMoveSpeed = maxMoveSpeed;
            GId = gId;
            PauseTick = pauseTick;
            LockingWho = null;
            CatchingWho = null;
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

            skill.LaunchSkill();
            NowCastSkill = skill;
        }

        private (TwoDVector? move, IHitStuff? launchBullet) ActNowSkillATick()
        {
            if (NowCastSkill == null)
            {
                return (null, null);
            }

            //在有锁定目标时，会根据与当前目标的向量调整有一定程度的防止穿模型
            var limitV
                = LockingWho == null
                    ? null
                    : TwoDVector.TwoDVectorByPt(LockingWho.GetPos(), GetPos())
                        .ClockwiseTurn(CharacterBody.Sight.Aim)
                        .AddX(-CharacterBody.GetRr() - LockingWho.CharacterBody.GetRr());

            return NowCastSkill
                .GoATick(GetPos(), CharacterBody.Sight.Aim, limitV);
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

        public (ITwoDTwoP?, IHitStuff?) CharGoTick(Operate? operate) //角色一个tick行为
        {
            // 命中停帧 输入无效
            if (PauseTick > 0)
            {
                PauseTick -= 1;
                return (null, null);
            }

            //  被持续攻击状态 输入无效
            var dPoint = GetPos();
            if (AntiActBuff != null)
            {
                var (twoDPoint, antiActBuff) = AntiActBuff.GoTickDrivePos(dPoint);
                AntiActBuff = antiActBuff;
                return (twoDPoint, null);
            }

            if (NowCastSkill?.InWhichPeriod() == Skill.SkillPeriod.End) NowCastSkill = null;

            if (NowCastSkill != null)
            {
                var (move, launchBullet) = ActNowSkillATick();

                ComboByNext();

                if (operate?.ActOrMove == null) return (move, launchBullet);

                var weaponSkillStatus = NowCastSkill.ComboInputRes();

                if (weaponSkillStatus != null)
                {
                    var status = weaponSkillStatus.Value;
                    var b = operate.GetAction() == OpAction.Switch;
                    var toUse = NowWeapon;
                    if (b)
                    {
                        status = 0;
                        toUse = (toUse + 1) % Weapons.Count;
                    }

                    var nowWeapon = Weapons.TryGetValue(toUse, out var weapon)
                        ? weapon
                        : Weapons.First().Value;

                    var operateAction = operate.GetAction();


                    if (!nowWeapon.SkillGroups.TryGetValue(operateAction.Value, out var skills) ||
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

            if (operate.GetAction() == null) return (null, null);
            {
                if (operate.GetAction() == OpAction.Switch) NowWeapon = (NowWeapon + 1) % Weapons.Count;
                else
                {
                    if (!Weapons.TryGetValue(NowWeapon, out var weapon) ||
                        !weapon.SkillGroups.TryGetValue((OpAction) operate?.GetAction(), out var value) ||
                        !value.TryGetValue(0, out var skill)) return (null, null);
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


    public class Heal
    {
        private int HealValue;
    }


    public class DamageBuff
    {
    }
}