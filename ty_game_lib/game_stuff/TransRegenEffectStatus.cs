namespace game_stuff
{
    public class TransRegenEffectStatus
    {
        public TransRegenEffectStatus()
        {
            HpToArmor = 0;
            HpToShield = 0;
            ArmorToShield = 0;
            ArmorToHp = 0;
            ShieldToHp = 0;
            ShieldToArmor = 0;
        }

        private int HpToArmor { get; set; }
        private int HpToShield { get; set; }
        private int ArmorToShield { get; set; }
        private int ArmorToHp { get; set; }
        private int ShieldToHp { get; set; }
        private int ShieldToArmor { get; set; }

        public void TransRegenerationEffectChange(float[] transRegeneration)
        {
            HpToArmor = (int) transRegeneration[0];
            HpToShield = (int) transRegeneration[1];
            ArmorToShield = (int) transRegeneration[2];
            ArmorToHp = (int) transRegeneration[3];
            ShieldToHp = (int) transRegeneration[4];
            ShieldToArmor = (int) transRegeneration[5];
        }

        public (int sR, int aR, int hR) GetTransValue((uint sLoss, uint aLoss, uint hLoss) valueTuple)
        {
            var (sLoss, aLoss, hLoss) = valueTuple;

            var hpR = ArmorToHp * aLoss + ShieldToHp * sLoss;
            var armorR = ShieldToArmor * sLoss + HpToArmor * hLoss;
            var sR = ArmorToShield * sLoss + HpToShield * hLoss;

            return ((int) sR, (int) armorR, (int) hpR);
        }
    }
}