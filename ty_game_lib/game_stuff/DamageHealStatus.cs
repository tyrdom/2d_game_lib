using System;

namespace game_stuff
{
    public class DamageHealStatus
    {
        private int MaxHp { get; }
        private int NowHp { get; set; }

        private int NowArmor { get; set; }
        private int MaxArmor { get; }

        private int ArmorStrength { get; }
        private int NowShield { get; set; }
        private int MaxShield { get; }

        private int NowDelayTick { get; set; }

        private int ShieldDelayTick { get; }

        private int ShieldRecover { get; }


        private DamageHealStatus(int maxHp, int nowHp, int nowArmor, int maxArmor, int nowShield, int maxShield,
            int shieldDelayTick, int armorStrength, int shieldRecover)
        {
            MaxHp = maxHp;
            NowHp = nowHp;
            NowArmor = nowArmor;
            MaxArmor = maxArmor;
            NowShield = nowShield;
            MaxShield = maxShield;
            ShieldDelayTick = shieldDelayTick;
            ArmorStrength = armorStrength;
            ShieldRecover = shieldRecover;
        }

        public static DamageHealStatus StartDamageHealAbout()
        {
            return new DamageHealStatus(TempConfig.StartHp, TempConfig.StartHp, 0, 0, 0, 0, 5, 1, 0);
        }

        
        
        public void TakeDamage(Damage damage)
        {
            var rest = damage.StandardDamageValue;

            NowDelayTick = ShieldDelayTick;

            var nowShield = NowShield - rest;

            if (nowShield >= 0)
            {
                NowShield = nowShield;
                return;
            }

            NowShield = 0;
            rest = -nowShield;

            var nowArmor = NowArmor - rest + ArmorStrength;


            if (nowArmor >= 0)
            {
                NowArmor = nowArmor;
                return;
            }

            {
                NowArmor = 0;
                rest = -nowArmor;
            }

            NowHp -= rest;
        }

        public void GetHeal(int heal)
        {
            NowHp = Math.Min(NowHp + heal, MaxHp);
        }

        public void FixArmor(int fix)
        {
            NowArmor = Math.Min(NowArmor + fix, MaxArmor);
        }

        public void ChargeShield(int charge)
        {
            NowShield = Math.Min(NowShield + charge, MaxShield);
        }


        public void GoATick()
        {
            if (NowDelayTick == 0)
            {
                NowShield = Math.Min(NowShield + ShieldRecover, MaxShield);
            }

            else if (NowDelayTick > 0)
            {
                NowDelayTick--;
            }
        }
    }
}