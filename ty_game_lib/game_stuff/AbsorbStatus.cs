using System.Numerics;
using game_config;

namespace game_stuff
{
    public struct AbsorbStatus
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
            return new AbsorbStatus(genBaseAttrById.HPAbsorb, genBaseAttrById.ArmorAbsorb, genBaseAttrById.ShieldAbsorb,
                genBaseAttrById.AmmoAbsorb, genBaseAttrById.ProtectAbsorb
            );
        }

        public void PassiveEffectChange(float[] vector, AbsorbStatus regenBaseAttr)
        {
            HpAbs = regenBaseAttr.HpAbs * (1 + vector[0]);
            ArmorAbs = regenBaseAttr.ArmorAbs * (1 + vector[1]);
            ShieldAbs = regenBaseAttr.ShieldAbs * (1 + vector[2]);
            AmmoAbs = regenBaseAttr.AmmoAbs * (1 + vector[3]);
            ProtectAbs = regenBaseAttr.ProtectAbs * (1 + vector[4]);
        }
    }
}