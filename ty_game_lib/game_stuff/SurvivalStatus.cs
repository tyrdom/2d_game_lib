using System;

namespace game_stuff
{
    public class SurvivalStatus
    {
        private uint MaxHp { get; }
        private uint NowHp { get; set; }

        private uint NowArmor { get; set; }
        private uint MaxArmor { get; }

        private uint ArmorDefence { get; }
        private uint NowShield { get; set; }
        private uint MaxShield { get; }

        private uint NowDelayTick { get; set; }

        private uint ShieldDelayTick { get; }
        private uint ShieldInstability { get; }
        private uint ShieldRecover { get; }


        private SurvivalStatus(uint maxHp, uint nowHp, uint nowArmor, uint maxArmor, uint nowShield, uint maxShield,
            uint shieldDelayTick, uint armorDefence, uint shieldRecover, uint shieldInstability)
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
        }

        public static SurvivalStatus StartDamageHealAbout()
        {
            return new SurvivalStatus(TempConfig.BaseHp, TempConfig.BaseHp, 0, 0, 0, 0, 5, 1, 0, 0);
        }


        private bool IsDead()
        {
            return NowHp == 0;
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

            NowHp -= (uint) rest;
        }

        public void GetHeal(uint heal)
        {
            NowHp = Math.Min(NowHp + heal, MaxHp);
        }

        public void FixArmor(uint fix)
        {
            NowArmor = Math.Min(NowArmor + fix, MaxArmor);
        }

        public void ChargeShield(uint charge)
        {
            NowShield += charge;
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
    }
}