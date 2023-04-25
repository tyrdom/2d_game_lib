using System;
using System.Collections.Generic;
using System.Numerics;
using game_config;

namespace game_stuff
{
    public interface IPassiveTraitEffect
    {
        IPassiveTraitEffect GenEffect(uint level);


        float[] GetVector();
    }

    public interface IPassiveTraitEffectForVehicle : IPassiveTraitEffect
    {
    }

    public static class PassiveEffectStandard
    {
        public static IPassiveTraitEffect GenById(passive_id id)
        {
            if (!CommonConfig.Configs.passives.TryGetValue(id, out var passive))
                throw new KeyNotFoundException($"not such passive id {id}");
            var passiveParamValues = passive.param_values;

#if DEBUG
            // var aggregate = passiveParamValues.Aggregate("", (s, c) => s + c + ',');
            // Console.Out.WriteLine($"{aggregate} ~~~ pass{passiveParamValues.Length}");
#endif

            return passive.passive_effect_type switch
            {
                passive_type.Survive => new SurvivalAboutPassiveEffect(passiveParamValues),
                passive_type.ProtectAbout => new ProtectAboutEffect(passiveParamValues),
                passive_type.Other => new OtherAttrPassiveEffect(passiveParamValues),
                passive_type.HitWinBuff => new HitPass(passiveParamValues),
                passive_type.Attack => new AtkAboutPassiveEffect(passiveParamValues),
                passive_type.AddItem => new AddItem(passiveParamValues),
                passive_type.TrapAbout => new TrapEffect(passiveParamValues),
                passive_type.Regen => new RegenPassiveEffect(passiveParamValues),
                passive_type.AbsorbAdd => new AbsorbAboutPassiveEffect(passiveParamValues),
                passive_type.SpecialDamageAdd => new SpecialDamageAddEffect(passiveParamValues),
                passive_type.TransRegeneration => new TransRegenerationEffect(passiveParamValues),
                passive_type.StunFix => new StunFixEffect(passiveParamValues),
                passive_type.BladeWave => new BladeWaveEffect(passiveParamValues),
                passive_type.TrickRegeneration => new BreakTrickRegenerationEffect(passiveParamValues),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public readonly struct BreakTrickRegenerationEffect : IPassiveTraitEffect
    {
        public BreakTrickRegenerationEffect(float[] passiveParamValues)
        {
            V = passiveParamValues;
        }

        public float[] V { get; }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new BreakTrickRegenerationEffect(ArrayTools.Multiply(level, GetVector()));
        }

        public float[] GetVector()
        {
            return V;
        }
    }

    public readonly struct BladeWaveEffect : IPassiveTraitEffect
    {
        public BladeWaveEffect(float[] v)
        {
            V = v;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new BladeWaveEffect(ArrayTools.Multiply(level, GetVector()));
        }

        public float[] GetVector()
        {
            return V;
        }

        private float[] V { get; }
    }

    public readonly struct StunFixEffect : IPassiveTraitEffect
    {
        private float[] V { get; }

        public StunFixEffect(float[] vector)
        {
            V = vector;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new StunFixEffect(ArrayTools.Multiply(level, GetVector()));
        }

        public float[] GetVector()
        {
            return V;
        }
    }

    public readonly struct TransRegenerationEffect : IPassiveTraitEffect
    {
        private float[] V { get; }

        public TransRegenerationEffect(float[] vector)
        {
#if DEBUG
            Console.Out.WriteLine($"gen transRegen effect {vector.Aggregate("", (s, x) => s + "," + x)}");
#endif
            V = vector;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new TransRegenerationEffect(ArrayTools.Multiply(level, GetVector()));
        }

        public float[] GetVector()
        {
            return V;
        }
    }

    public readonly struct SpecialDamageAddEffect : IPassiveTraitEffect
    {
        private float[] Adds { get; }

        public SpecialDamageAddEffect(float[] vector)
        {
            Adds = vector;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new SpecialDamageAddEffect(ArrayTools.Multiply(level, GetVector()));
        }

        public float[] GetVector()
        {
            return Adds;
        }
    }

    public readonly struct HitPass : IPassiveTraitEffect
    {
        public HitPass(float[] hitAddNum)
        {
            HitAddNum = hitAddNum;
        }

        private float[] HitAddNum { get; }

        // public float AtkPass { get; }0
        //
        // public float DefencePass { get; }1
        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = ArrayTools.Multiply(level, GetVector());
            return new HitPass(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public float[] GetVector()
        {
            return HitAddNum;
        }
    }

    public readonly struct AddItem : IPassiveTraitEffect
    {
        private float[] TicketAdd { get; }

        public AddItem(float[] ticketAdd)
        {
            TicketAdd = ticketAdd;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new AddItem(ArrayTools.Multiply(level, GetVector()));
        }


        public float[] GetVector()
        {
            return TicketAdd;
        }
    }

    public readonly struct SurvivalAboutPassiveEffect : IPassiveTraitEffectForVehicle
    {
        private float[] SurvivalMultiAdd { get; }
        // public float HpMultiAdd { get; }0

        // public float ArmorMultiAdd { get; }1
        //
        // public float DefMultiAdd { get; }2
        //
        // public float ShieldMultiAdd { get; }3
        //
        // public float ShieldRegMultiAdd { get; }4
        //
        // public float ShieldInstabilityMultiAdd { get; }5

        public SurvivalAboutPassiveEffect(float[] survivalMultiAdd)
        {
            SurvivalMultiAdd = survivalMultiAdd;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new SurvivalAboutPassiveEffect(ArrayTools.Multiply(level, GetVector()));
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public float[] GetVector()
        {
            return SurvivalMultiAdd;
        }
    }

    public readonly struct TrapEffect : IPassiveTraitEffectForVehicle
    {
        public TrapEffect(float[] trapAdd)
        {
            TrapAdd = trapAdd;
        }

        private float[] TrapAdd { get; }

        // public float TrapAtkMultiAdd { get; }0
        //
        // public float TrapSurvivalMultiAdd { get; }1
        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = ArrayTools.Multiply(level, TrapAdd);
            return new TrapEffect(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public float[] GetVector()
        {
            return TrapAdd;
        }
    }

    public readonly struct OtherAttrPassiveEffect : IPassiveTraitEffectForVehicle
    {
        public OtherAttrPassiveEffect(float[] otherAttrAdd)
        {
            OtherAttrAdd = otherAttrAdd;
        }

        private float[] OtherAttrAdd { get; }
        // public float MaxAmmoMultiAdd { get; }0

        // public float MaxSpeedUp { get; }1
        // public float AddSpeedUp { get; }2

        // public float MaxPropMultiAdd { get; }3
        // public float RecycleMultiAdd { get; }4


        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = ArrayTools.Multiply(level, GetVector());
            return new OtherAttrPassiveEffect(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public float[] GetVector()
        {
            return OtherAttrAdd;
        }
    }

    public readonly struct ProtectAboutEffect : IPassiveTraitEffect
    {
        public ProtectAboutEffect(float[] tickAddMulti)
        {
            TickAddMulti = tickAddMulti;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = ArrayTools.Multiply(level, GetVector());
            return new ProtectAboutEffect(multiply);
        }

        private float[] TickAddMulti { get; }

        public float[] GetVector()
        {
            return TickAddMulti;
        }
    }

    public readonly struct RegenPassiveEffect : IPassiveTraitEffectForVehicle
    {
        public RegenPassiveEffect(float[] regenAdd)
        {
            RegenAdd = regenAdd;
        }
        // public float HealAdd { get; }0

        // public float FixAdd { get; }1
        //
        // public float ShieldChargeAdd { get; }2
        //
        // public float ReloadAdd { get; }3
        private float[] RegenAdd { get; }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = ArrayTools.Multiply(level, GetVector());
            return new RegenPassiveEffect(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public float[] GetVector()
        {
            return RegenAdd;
        }
    }

    public readonly struct AbsorbAboutPassiveEffect : IPassiveTraitEffectForVehicle
    {
        public AbsorbAboutPassiveEffect(float[] absorbAttrAdd)
        {
            AbsorbAttrAdd = absorbAttrAdd;
        }

        private float[] AbsorbAttrAdd { get; }

        // public float HPAbsMultiAdd { get; }
        // public float ArmorAbsMultiAdd { get; }
        // public float ShieldAbsMultiAdd { get; }
        // public float AmmoAbsMultiAdd { get; }
        // public float ProtectAbsMultiAdd { get; }
        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = ArrayTools.Multiply(level, GetVector());
            return new AbsorbAboutPassiveEffect(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public float[] GetVector()
        {
            return AbsorbAttrAdd;
        }
    }

    public readonly struct AtkAboutPassiveEffect : IPassiveTraitEffectForVehicle
    {
        public AtkAboutPassiveEffect(float[] atkAttrAdd)
        {
            // MainAtkMultiAdd = mainAtkMultiAdd;
            // ShardedNumAdd = shardedNumAdd;
            // BackStabAdd = backStabAdd;
            AtkAttrAdd = atkAttrAdd;
        }


        private float[] AtkAttrAdd { get; }
        // public float MainAtkMultiAdd { get; }
        // public float ShardedNumAdd { get; }
        // public float BackStabAdd { get; }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = ArrayTools.Multiply(level, AtkAttrAdd);
            return new AtkAboutPassiveEffect(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public float[] GetVector()
        {
            return AtkAttrAdd;
        }
    }
}