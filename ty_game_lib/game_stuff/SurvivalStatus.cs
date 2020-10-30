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

        
        public void TakeDamage(uint damage)
        {
            var rest = (int) damage;

            NowDelayTick = ShieldDelayTick;

            var nowShield = (int) (NowShield - ShieldInstability) - rest;

            if (nowShield >= 0)
            {
                NowShield = (uint) nowShield;
                return;
            }

            NowShield = 0;
            rest = -nowShield;

            var nowArmor = (int) NowArmor - rest + (int) ArmorDefence;


            if (nowArmor >= 0)
            {
                NowArmor = (uint) nowArmor;
                return;
            }

            {
                NowArmor = 0;
                rest = -nowArmor;
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