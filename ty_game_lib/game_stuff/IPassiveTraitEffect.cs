using System.Numerics;

namespace game_stuff
{
    public interface IPassiveTraitEffect
    {
        IPassiveTraitEffect GenEffect(uint level);


        Vector<float> GetZero();
        Vector<float> GetVector();
    }

    public static class PassiveEffectStandard
    {
    }

    public readonly struct SurvivalAboutPassiveEffect : IPassiveTraitEffect
    {
        private Vector<float> SurvivalMultiAdd { get; }
        // public float HpMultiAdd { get; }0
        //
        // public float HealMultiAdd { get; }1
        //
        // public float ArmorMultiAdd { get; }2
        //
        // public float DefMultiAdd { get; }3
        //
        // public float ShieldMultiAdd { get; }4
        //
        // public float ShieldRegMultiAdd { get; }5
        //
        // public float ShieldInstabilityMultiAdd { get; }6

        private SurvivalAboutPassiveEffect(Vector<float> survivalMultiAdd)
        {
            SurvivalMultiAdd = survivalMultiAdd;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new SurvivalAboutPassiveEffect(Vector.Multiply(level, SurvivalMultiAdd));
        }

        public Vector<float> GetZero()
        {
            return Zero();
        }

        public Vector<float> GetVector()
        {
            return SurvivalMultiAdd;
        }

        public static Vector<float> Zero()
        {
            return new Vector<float>(new[] {0f, 0f, 0f, 0f, 0f, 0f, 0f});
        }
    }


    public readonly struct OtherAttrPassiveEffect : IPassiveTraitEffect
    {
        private OtherAttrPassiveEffect(Vector<float> otherAttrAdd)
        {
            OtherAttrAdd = otherAttrAdd;
        }

        private Vector<float> OtherAttrAdd { get; }
        // public float MaxAmmoMultiAdd { get; }0

        // public float MaxSpeedUp { get; }1
        // public float AddSpeedUp { get; }2

        // public float MaxPropMultiAdd { get; }3
        // public float RecycleMultiAdd { get; }4


        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, OtherAttrAdd);
            return new OtherAttrPassiveEffect(multiply);
        }

        Vector<float> IPassiveTraitEffect.GetZero()
        {
            return Zero();
        }

        public Vector<float> GetVector()
        {
            return OtherAttrAdd;
        }

        public static Vector<float> Zero()
        {
            return new Vector<float>(new[] {0f, 0f, 0f, 0f, 0f});
        }
    }

    public readonly struct AtkAboutPassiveEffect : IPassiveTraitEffect
    {
        private AtkAboutPassiveEffect(Vector<float> atkAttrAdd)
        {
            // MainAtkMultiAdd = mainAtkMultiAdd;
            // ShardedNumAdd = shardedNumAdd;
            // BackStabAdd = backStabAdd;
            AtkAttrAdd = atkAttrAdd;
        }

        public static Vector<float> Zero()
        {
            return new Vector<float>(new[] {0f, 0f, 0f});
        }

        private Vector<float> AtkAttrAdd { get; }
        // public float MainAtkMultiAdd { get; }
        // public float ShardedNumAdd { get; }
        // public float BackStabAdd { get; }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, AtkAttrAdd);
            return new AtkAboutPassiveEffect(multiply);
        }

        Vector<float> IPassiveTraitEffect.GetZero()
        {
            return Zero();
        }

        public Vector<float> GetVector()
        {
            return AtkAttrAdd;
        }
    }
}