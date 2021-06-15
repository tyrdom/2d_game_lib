using System;

namespace game_stuff
{
    public struct TransRegenEffectStatus
    {
        public TransRegenEffectStatus(int a)
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

        public void GetTransValue(int sLoss, int aLoss, int hLoss, out int sR, out int armorR,
            out int hpR)
        {
            hpR = ArmorToHp * aLoss + ShieldToHp * sLoss;
            armorR = ShieldToArmor * sLoss + HpToArmor * hLoss;
            sR = ArmorToShield * sLoss + HpToShield * hLoss;

#if DEBUG
            Console.Out.WriteLine(
                $"take damage to regen  sR:{sR} aR:{armorR} hR:{hpR}");
#endif
        }
    }
}