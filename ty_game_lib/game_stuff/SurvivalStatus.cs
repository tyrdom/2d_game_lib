using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class SurvivalStatus
    {
        private uint MaxHp { get; set; }
        private uint NowHp { get; set; }

        private float HealEffect { get; set; }
        private uint NowArmor { get; set; }
        private uint MaxArmor { get; set; }

        private uint ArmorDefence { get; set; }
        private uint NowShield { get; set; }
        private uint MaxShield { get; set; }

        private uint NowDelayTick { get; set; }

        private uint ShieldDelayTick { get; }
        private uint ShieldInstability { get; set; }
        private uint ShieldRecover { get; set; }


        private SurvivalStatus(uint maxHp, uint nowHp, uint nowArmor, uint maxArmor, uint nowShield, uint maxShield,
            uint shieldDelayTick, uint armorDefence, uint shieldRecover, uint shieldInstability, float healEffect)
        {
            MaxHp = maxHp;
            NowHp = nowHp;
            NowArmor = nowArmor;
            MaxArmor = maxArmor;
            NowShield = nowShield;
            MaxShield = maxShield;
            ShieldDelayTick = shieldDelayTick;
            ArmorDefence = armorDefence;
            ShieldRecover = shieldRecover;
            ShieldInstability = shieldInstability;
            HealEffect = healEffect;
        }

        public static SurvivalStatus StartDamageHealAbout()
        {
            return new SurvivalStatus(TempConfig.BaseHp, TempConfig.BaseHp, 0, 0, 0, 0, 5, 1, 0, 0, 1f);
        }


        private bool IsDead()
        {
            return NowHp <= 0;
        }

        public void TakeDamage(Damage damage)
        {
            NowDelayTick = ShieldDelayTick;
            TakeOneDamage(damage.MainDamage);
            TakeMultiDamage(damage.ShardedDamage, damage.ShardedNum);
        }

        private void TakeMultiDamage(uint damage, uint times)
        {
            static uint GetTime(int restDamage, uint damage)
            {
                return (uint) restDamage / damage;
            }

            var restDamage = (int) (damage * times);
            var restTime = GetTime(restDamage, damage);
            if (NowShield > 0)
            {
                var nowShield = (int) NowShield - (int) (ShieldInstability * restTime) - restDamage;
                if (nowShield >= 0)
                {
                    NowShield = (uint) nowShield;
                    return;
                }

                NowShield = 0;
                restDamage = -nowShield;
                restTime = GetTime(restDamage, damage);
            }

            if (NowArmor > 0)
            {
                var nowArmor = (int) NowArmor + (int) (ArmorDefence * restTime) - restDamage;

                if (nowArmor >= 0)
                {
                    NowArmor = (uint) nowArmor;
                    return;
                }

                NowArmor = 0;
                restDamage = -nowArmor;
            }

            if ((uint) restDamage >= NowHp)
            {
                NowHp = 0;
                return;
            }

            NowHp -= (uint) restDamage;
        }


        public void TakeOneDamage(uint damage)
        {
            var rest = (int) damage;

            if (NowShield > 0)
            {
                var nowShield = (int) NowShield - (int) ShieldInstability - rest;

                if (nowShield >= 0)
                {
                    NowShield = (uint) nowShield;
                    return;
                }

                NowShield = 0;
                rest = -nowShield;
            }

            if (NowArmor > 0)
            {
                var nowArmor = (int) NowArmor + (int) ArmorDefence - rest;

                if (nowArmor >= 0)
                {
                    NowArmor = (uint) nowArmor;
                    return;
                }

                {
                    NowArmor = 0;
                    rest = -nowArmor;
                }
            }

            if ((uint) rest >= NowHp)
            {
                NowHp = 0;
                return;
            }

            NowHp -= (uint) rest;
        }

        public void GetRegen(Regeneration regeneration)
        {
            GetHeal(regeneration.HealMulti);
            FixArmor(regeneration.FixMulti);
            ChargeShield(regeneration.ShieldMulti);
        }

        private void GetHeal(float healMulti)
        {
            NowHp = (uint) MathTools.Min(NowHp + healMulti * MaxHp * HealEffect, MaxHp);
        }

        private void FixArmor(float fixMulti)
        {
            NowArmor = (uint) MathTools.Min(NowArmor + fixMulti * MaxArmor, MaxArmor);
        }

        private void ChargeShield(float chargeMulti)
        {
            var maxShield = (uint) (chargeMulti * MaxShield);
            NowShield += maxShield;
        }


        public bool GoATickAndCheckAlive()
        {
            if (IsDead()) return false;
            if (NowDelayTick == 0 && NowShield < MaxShield)
            {
                NowShield = Math.Min(NowShield + ShieldRecover, MaxShield);
            }

            else if (NowDelayTick > 0)
            {
                NowDelayTick--;
            }

            return true;
        }


        public static SurvivalStatus GenByConfig(base_attribute baseAttribute)
        {
            var baseAttributeMaxHp = baseAttribute.MaxHP;
            var baseAttributeMaxArmor = baseAttribute.MaxArmor;
            var baseAttributeArmorDefence = baseAttribute.ArmorDefence;
            var baseAttributeMaxShield = baseAttribute.MaxShield;
            var baseAttributeShieldRecover = TempConfig.NumSecToTick(baseAttribute.ShieldRecover);
            var baseAttributeShieldInstability = baseAttribute.ShieldInstability;
            var baseAttributeShieldDelayTime = TempConfig.GetTickByTime(baseAttribute.ShieldDelayTime);

            var baseAttributeHealEffect = baseAttribute.HealEffect;
            return new SurvivalStatus(baseAttributeMaxHp, baseAttributeMaxHp, baseAttributeMaxArmor,
                baseAttributeMaxArmor, baseAttributeMaxShield, baseAttributeMaxShield,
                baseAttributeShieldDelayTime, baseAttributeArmorDefence, baseAttributeShieldRecover,
                baseAttributeShieldInstability, baseAttributeHealEffect);
        }


        public void SurvivalPassiveEffectChange(IEnumerable<SurvivalAboutPassiveEffect> passiveTrait,
            SurvivalStatus baseSurvivalStatus)

        {
            var (hpm, hem, arm, dem, shm, srm, sim) = passiveTrait.Aggregate((0f, 0f, 0f, 0f, 0f, 0f, 0f), (s, x) =>
                (s.Item1 + x.HpMultiAdd,
                    s.Item2 + x.HealMultiAdd,
                    s.Item3 + x.ArmorMultiAdd,
                    s.Item4 + x.DefMultiAdd,
                    s.Item5 + x.ShieldMultiAdd,
                    s.Item6 + x.ShieldRegMultiAdd,
                    s.Item7 + x.ShieldInstabilityMultiAdd));
            var lossHp = MaxHp - NowHp;
            MaxHp = (uint) (baseSurvivalStatus.MaxHp * (1 + hpm));
            NowHp = MaxHp - lossHp;
            HealEffect = baseSurvivalStatus.HealEffect * (1 + hem);
            var lossAr = MaxArmor - NowArmor;
            MaxArmor = (uint) (baseSurvivalStatus.MaxArmor * (1 + arm));
            NowArmor = MaxArmor - lossAr;
            ArmorDefence = (uint) (baseSurvivalStatus.ArmorDefence * (1 + dem));
            MaxShield = (uint) (baseSurvivalStatus.MaxShield * (1 + shm));
            ShieldInstability = (uint) (baseSurvivalStatus.ShieldInstability * (1 + sim));
            ShieldRecover = (uint) (baseSurvivalStatus.ShieldRecover * (1 + srm));
        }
    }
}