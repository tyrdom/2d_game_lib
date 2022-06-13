#if DEBUG
using System;
#endif

namespace game_stuff
{
    public class TransRegenEffectStatus
    {
        public TransRegenEffectStatus()
        {
            ArmorBreakPropMulti = 0;
            ArmorBreakShieldMulti = 0;
            ArmorBreakProtectMulti = 0;
            ShieldBreakPropMulti = 0;
            ShieldBreakArmorMulti = 0;
            ShieldBreakProtectMulti = 0;
            HpToArmor = 0;
            HpToShield = 0;
            ArmorToShield = 0;
            ArmorToHp = 0;
            ShieldToHp = 0;
            ShieldToArmor = 0;
            PropToHpMulti = 0;

        }

        private float HpToArmor { get; set; }
        private float HpToShield { get; set; }
        private float ArmorToShield { get; set; }
        private float ArmorToHp { get; set; }
        private float ShieldToHp { get; set; }
        private float ShieldToArmor { get; set; }

        public float PropToHpMulti { get; set; }
        private float ArmorBreakPropMulti { get; set; }
        private float ArmorBreakShieldMulti { get; set; }
        private float ArmorBreakProtectMulti { get; set; }
        private float ShieldBreakPropMulti { get; set; }
        private float ShieldBreakArmorMulti { get; set; }
        private float ShieldBreakProtectMulti { get; set; }

        public void TransRegenerationEffectChange(float[] transRegeneration)
        {
            HpToArmor = transRegeneration[0];
            HpToShield = transRegeneration[1];
            ArmorToShield = transRegeneration[2];
            ArmorToHp = transRegeneration[3];
            ShieldToHp = transRegeneration[4];
            ShieldToArmor = transRegeneration[5];
            PropToHpMulti = transRegeneration[6];
#if DEBUG
            Console.Out.WriteLine(
                $"now regen attr~ a2s:{ArmorToShield} h2s:{HpToShield} h2a:{HpToArmor} s2a:{HpToArmor} s2h:{ShieldToHp} a2h:{ArmorToHp}");
#endif
        }

        public void BreakTrickRegenerationEffectChange(float[] transRegeneration)
        {
            ArmorBreakShieldMulti = transRegeneration[0];
            ArmorBreakPropMulti = transRegeneration[1];
            ArmorBreakProtectMulti = transRegeneration[2];
            ShieldBreakArmorMulti = transRegeneration[3];
            ShieldBreakPropMulti = transRegeneration[4];
            ShieldBreakProtectMulti = transRegeneration[5];
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
            shieldBreakArmor = ShieldBreakArmorMulti;
            shieldHp = ShieldBreakPropMulti;
            return ShieldBreakProtectMulti;
        }

        public float GetArmorBreakValue(out float o, out float o1)
        {
            o = ArmorBreakShieldMulti;
            o1 = ArmorBreakPropMulti;
            return ArmorBreakProtectMulti;
        }

        public Regeneration GetPropTrans(int propPropPointCost)
        {
            return new Regeneration(propPropPointCost*PropToHpMulti,0,0,0);
        }
    }
}