using System;
using System.IO;
using System.Numerics;
using game_config;

namespace game_stuff
{
    public interface IPassiveTraitEffect
    {
        IPassiveTraitEffect GenEffect(uint level);


        Vector<float> GetZero();
        Vector<float> GetVector();
    }

    public interface IPassiveTraitEffectForVehicle : IPassiveTraitEffect
    {
    }

    public static class PassiveEffectStandard
    {
        public static IPassiveTraitEffect GenById(int id)
        {
            if (!LocalConfig.Configs.passives.TryGetValue(id, out var passive))
                throw new DirectoryNotFoundException($"not such passive id {id}");
            var passiveParamValues = passive.param_values;
            var vector = new Vector<float>(passiveParamValues);

            return passive.passive_effect_type switch
            {
                passive_type.Survive => new SurvivalAboutPassiveEffect(vector),
                passive_type.TickAdd => new TickAddEffect(vector),
                passive_type.Other => new OtherAttrPassiveEffect(vector),
                passive_type.Special => new HitPass(vector),
                passive_type.Attack => new OtherAttrPassiveEffect(vector),
                passive_type.AddItem => new AddItem(vector),
                passive_type.TrapAbout => new TrapEffect(vector),
                passive_type.Regen => new RegenPassiveEffect(vector),
                passive_type.AbsorbAdd => new AbsorbAboutPassiveEffect(vector),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public readonly struct HitPass : IPassiveTraitEffect
    {
        public HitPass(Vector<float> hitAddNum)
        {
            HitAddNum = hitAddNum;
        }

        private Vector<float> HitAddNum { get; }

        // public float AtkPass { get; }0
        //
        // public float DefencePass { get; }1
        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, GetVector());
            return new HitPass(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public Vector<float> GetVector()
        {
            return HitAddNum;
        }
    }

    public readonly struct AddItem : IPassiveTraitEffect
    {
        private Vector<float> TicketAdd { get; }

        public AddItem(Vector<float> ticketAdd)
        {
            TicketAdd = ticketAdd;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new AddItem(Vector.Multiply(level, GetVector()));
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }


        public Vector<float> GetVector()
        {
            return TicketAdd;
        }
    }

    public readonly struct SurvivalAboutPassiveEffect : IPassiveTraitEffectForVehicle
    {
        private Vector<float> SurvivalMultiAdd { get; }
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

        public SurvivalAboutPassiveEffect(Vector<float> survivalMultiAdd)
        {
            SurvivalMultiAdd = survivalMultiAdd;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new SurvivalAboutPassiveEffect(Vector.Multiply(level, GetVector()));
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public Vector<float> GetVector()
        {
            return SurvivalMultiAdd;
        }
    }

    public readonly struct TrapEffect : IPassiveTraitEffectForVehicle
    {
        public TrapEffect(Vector<float> trapAdd)
        {
            TrapAdd = trapAdd;
        }

        private Vector<float> TrapAdd { get; }

        // public float TrapAtkMultiAdd { get; }0
        //
        // public float TrapSurvivalMultiAdd { get; }1
        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, TrapAdd);
            return new TrapEffect(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public Vector<float> GetVector()
        {
            return TrapAdd;
        }
    }

    public readonly struct OtherAttrPassiveEffect : IPassiveTraitEffectForVehicle
    {
        public OtherAttrPassiveEffect(Vector<float> otherAttrAdd)
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
            var multiply = Vector.Multiply(level, GetVector());
            return new OtherAttrPassiveEffect(multiply);
        }

        Vector<float> IPassiveTraitEffect.GetZero()
        {
            return Vector<float>.Zero;
        }

        public Vector<float> GetVector()
        {
            return OtherAttrAdd;
        }
    }

    public readonly struct TickAddEffect : IPassiveTraitEffect
    {
        public TickAddEffect(Vector<float> tickAddMulti)
        {
            TickAddMulti = tickAddMulti;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, GetVector());
            return new TickAddEffect(multiply);
        }

        private Vector<float> TickAddMulti { get; }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public Vector<float> GetVector()
        {
            return TickAddMulti;
        }
    }

    public readonly struct RegenPassiveEffect : IPassiveTraitEffectForVehicle
    {
        public RegenPassiveEffect(Vector<float> regenAdd)
        {
            RegenAdd = regenAdd;
        }
        // public float HealAdd { get; }0

        // public float FixAdd { get; }1
        //
        // public float ShieldChargeAdd { get; }2
        //
        // public float ReloadAdd { get; }3
        private Vector<float> RegenAdd { get; }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, GetVector());
            return new RegenPassiveEffect(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public Vector<float> GetVector()
        {
            return RegenAdd;
        }
    }

    public readonly struct AbsorbAboutPassiveEffect : IPassiveTraitEffectForVehicle
    {
        public AbsorbAboutPassiveEffect(Vector<float> absorbAttrAdd)
        {
            AbsorbAttrAdd = absorbAttrAdd;
        }

        private Vector<float> AbsorbAttrAdd { get; }

        // public float HPAbsMultiAdd { get; }
        // public float ArmorAbsMultiAdd { get; }
        // public float ShieldAbsMultiAdd { get; }
        // public float AmmoAbsMultiAdd { get; }
        // public float ProtectAbsMultiAdd { get; }
        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, GetVector());
            return new AbsorbAboutPassiveEffect(multiply);
        }

        public Vector<float> GetZero()
        {
            return Vector<float>.Zero;
        }

        public Vector<float> GetVector()
        {
            return AbsorbAttrAdd;
        }
    }

    public readonly struct AtkAboutPassiveEffect : IPassiveTraitEffectForVehicle
    {
        private AtkAboutPassiveEffect(Vector<float> atkAttrAdd)
        {
            // MainAtkMultiAdd = mainAtkMultiAdd;
            // ShardedNumAdd = shardedNumAdd;
            // BackStabAdd = backStabAdd;
            AtkAttrAdd = atkAttrAdd;
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
            return Vector<float>.Zero;
        }

        public Vector<float> GetVector()
        {
            return AtkAttrAdd;
        }
    }
}