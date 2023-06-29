namespace game_stuff
{
    public class DamageFullMultiStatus
    {
        private float FullHpAddMulti { get; set; }

        private float FullArmorMulti { get; set; }

        private float FullShieldMulti { get; set; }

        private float ExtraShieldMulti { get; set; }

        public float FullAmmoMulti { get; private set; }

        public DamageFullMultiStatus()
        {
            FullHpAddMulti = 0;
            FullArmorMulti = 0;
            FullShieldMulti = 0;
            ExtraShieldMulti = 0;
            FullAmmoMulti = 0;
        }

        public void RefreshSurvivalDmgMulti(float[] vector)
        {
            FullHpAddMulti = vector[0];
            FullArmorMulti = vector[1];
            FullShieldMulti = vector[2];
            ExtraShieldMulti = vector[3];
            FullAmmoMulti = vector[4];
        }

        public float GetTotalMulti(bool hpFull, bool armorFull, bool shieldFull, float extraShield, bool b)
        {
            var v1 = hpFull ? FullHpAddMulti : 0;
            var v2 = armorFull ? FullArmorMulti : 0;
            var v3 = b ? FullAmmoMulti : 0;
            var v4 = shieldFull ? FullShieldMulti : 0;
            var v5 = extraShield * ExtraShieldMulti;
            return v1 + v2 + v3 + v4 + v5;
        }
    }
}