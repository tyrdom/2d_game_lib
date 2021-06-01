using System.Numerics;
using game_config;

namespace game_stuff
{
    public struct RegenEffectStatus
    {
        private RegenEffectStatus(float healEffect, float fixEffect, float chargeEffect, float reloadEffect)
        {
            HealEffect = healEffect;
            FixEffect = fixEffect;
            ChargeEffect = chargeEffect;
            ReloadEffect = reloadEffect;
        }

        public float HealEffect { get; private set; }
        public float FixEffect { get; private set; }
        public float ChargeEffect { get; private set; }
        public float ReloadEffect { get; private set; }

        public static RegenEffectStatus GenBaseByAttr(base_attribute baseAttribute)
        {
            var baseAttributeArmorFixEffect = baseAttribute.ArmorFixEffect;
            var baseAttributeHealEffect = baseAttribute.HealEffect;
            var baseAttributeShieldChargeEffect = baseAttribute.ShieldChargeEffect;
            var baseAttributeReloadMulti = baseAttribute.ReloadMulti;
            return new RegenEffectStatus(baseAttributeHealEffect, baseAttributeArmorFixEffect,
                baseAttributeShieldChargeEffect, baseAttributeReloadMulti);
        }

        public void PassiveEffectChange(float[] regenAttrPassiveEffects, RegenEffectStatus regenBaseAttr)
        {
            HealEffect = regenBaseAttr.HealEffect * (1 + regenAttrPassiveEffects[0]);
            FixEffect = regenBaseAttr.FixEffect * (1 + regenAttrPassiveEffects[1]);
            ChargeEffect = regenBaseAttr.ChargeEffect * (1 + regenAttrPassiveEffects[2]);
            ReloadEffect = regenBaseAttr.ReloadEffect * (1 + regenAttrPassiveEffects[3]);
        }
    }
}