using System;

namespace game_stuff
{
    public class TransRegenEffectStatus
    {
        public TransRegenEffectStatus()
        {
            ArmorBreakHp = 0;
            ArmorBreakShield = 0;
            ArmorBreakProtect = 0;
            ShieldBreakHp = 0;
            ShieldBreakArmor = 0;
            ShieldBreakProtect = 0;
            HpToArmor = 0;
            HpToShield = 0;
            ArmorToShield = 0;
            ArmorToHp = 0;
            ShieldToHp = 0;
            ShieldToArmor = 0;
        }

        private float HpToArmor { get; set; }
        private float HpToShield { get; set; }
        private float ArmorToShield { get; set; }
        private float ArmorToHp { get; set; }
        private float ShieldToHp { get; set; }
        private float ShieldToArmor { get; set; }

        private float ArmorBreakHp { get; set; }
        private float ArmorBreakShield { get; set; }
        private float ArmorBreakProtect { get; set; }
        private float ShieldBreakHp { get; set; }
        private float ShieldBreakArmor { get; set; }
        private float ShieldBreakProtect { get; set; }

        public void TransRegenerationEffectChange(float[] transRegeneration)
        {
            HpToArmor = transRegeneration[0];
            HpToShield = transRegeneration[1];
            ArmorToShield = transRegeneration[2];
            ArmorToHp = transRegeneration[3];
            ShieldToHp = transRegeneration[4];
            ShieldToArmor = transRegeneration[5];
#if DEBUG
            Console.Out.WriteLine(
                $"now regen attr~ a2s:{ArmorToShield} h2s:{HpToShield} h2a:{HpToArmor} s2a:{HpToArmor} s2h:{ShieldToHp} a2h:{ArmorToHp}");
#endif
        }

        public void BreakTrickRegenerationEffectChange(float[] transRegeneration)
        {
            ArmorBreakShield = transRegeneration[0];
            ArmorBreakHp = transRegeneration[1];
            ArmorBreakProtect = transRegeneration[2];
            ShieldBreakArmor = transRegeneration[3];
            ShieldBreakHp = transRegeneration[4];
            ShieldBreakProtect = transRegeneration[5];
        }

        public void GetTransValue(int sLoss, int aLoss, int hLoss, out int sR, out int armorR,
            out int hpR)
        {
            hpR = (int) (ArmorToHp * aLoss + ShieldToHp * sLoss);
            armorR = (int) (ShieldToArmor * sLoss + HpToArmor * hLoss);
            sR = (int) (ArmorToShield * aLoss + HpToShield * hLoss);

#if DEBUG
            Console.Out.WriteLine($"take loss are s:{sLoss} a:{aLoss} h:{hLoss}");
            Console.Out.WriteLine(
                $"take damage to regen  sR:{sR} by  {ArmorToShield} + {HpToShield}\n aR:{armorR} by {ShieldToArmor} + {HpToArmor} \n hR:{hpR} by {ArmorToHp} + {ShieldToHp} ");
#endif
        }

       

        public float GetShieldBreakValue(out float shieldBreakArmor,out float shieldHp)
        {
            shieldBreakArmor = ShieldBreakArmor;
            shieldHp = ShieldBreakHp;
            return ShieldBreakProtect;
        }

        public float GetArmorBreakValue(out float o, out float o1)
        {
            o = ArmorBreakShield;
            o1 = ArmorBreakHp;
            return ArmorBreakProtect;
        }
    }
}