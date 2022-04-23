using System;
using System.Linq;
using System.Numerics;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class AbsorbStatus
    {
        private AbsorbStatus(float hpAbs, float armorAbs, float shieldAbs, float ammoAbs, float protectAbs)
        {
            HpAbs = hpAbs;
            ArmorAbs = armorAbs;
            ShieldAbs = shieldAbs;
            ProtectAbs = protectAbs;
            AmmoAbs = ammoAbs;
        }

        public float HpAbs { get; private set; }
        public float ArmorAbs { get; private set; }
        public float ShieldAbs { get; private set; }
        public float AmmoAbs { get; private set; }
        public float ProtectAbs { get; private set; }


        public static AbsorbStatus GenBaseByAttr(base_attribute genBaseAttrById)
        {
            return new AbsorbStatus(genBaseAttrById.HPAbsorb, genBaseAttrById.ArmorAbsorb, 
                genBaseAttrById.ShieldAbsorb,
                genBaseAttrById.AmmoAbsorb, genBaseAttrById.ProtectAbsorb
            );
        }

        public void PassiveEffectChange(float[] vector, AbsorbStatus regenBaseAttr)
        {
            // Console.Out.WriteLine($"Absorb Change : {vector.Aggregate("", (s, f) => s + f + ",")}");
            HpAbs = regenBaseAttr.HpAbs * (1f + vector[0]) / (1f + vector[5]);
            ArmorAbs = regenBaseAttr.ArmorAbs * (1f + vector[1]) / (1f + vector[6]);
            ShieldAbs = regenBaseAttr.ShieldAbs * (1f + vector[2]) / (1f + vector[7]);
            AmmoAbs = regenBaseAttr.AmmoAbs * (1f + vector[3]) / (1f + vector[8]);
            ProtectAbs = regenBaseAttr.ProtectAbs * (1f + vector[4]) / (1f + vector[9]);

            // Console.Out.WriteLine($"Absorb Change : {GetDetails()}");
        }

        public string GetDetails()
        {
            return
                $"生命吸收:{MathTools.Round(HpAbs,2)} 装甲吸收:{MathTools.Round(ArmorAbs,2)} 护盾吸收:{MathTools.Round(ShieldAbs,2)} 弹药吸收:{MathTools.Round(AmmoAbs,2)} 保护吸收:{MathTools.Round(ProtectAbs,2)}";
        }
    }
}