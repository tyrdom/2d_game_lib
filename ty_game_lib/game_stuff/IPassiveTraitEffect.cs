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

    public static class PassiveEffectStandard
    {
        public static IPassiveTraitEffect GenById(int id)
        {
            if (!TempConfig.Configs.passives.TryGetValue(id, out var passive))
                throw new DirectoryNotFoundException($"not such passive id {id}");
            var passiveParamValues = passive.param_values;
            var vector = new Vector<float>(passiveParamValues);

            switch (passive.passive_effect_type)
            {
                case passive_type.Survive:
                    return new SurvivalAboutPassiveEffect(vector);
                case passive_type.TickAdd:
                    return new TickAddEffect(vector);

                case passive_type.Other:
                    return new OtherAttrPassiveEffect(vector);
                case passive_type.Speical:
                    return new HitPass(vector);
                case passive_type.Attack:
                    return new OtherAttrPassiveEffect(vector);
                case passive_type.AddItem:
                    return new AddItem(vector);
                case passive_type.TrapAbout:
                    return new TrapEffect(vector);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public readonly struct HitPass : IPassiveTraitEffect
    {
        public HitPass(Vector<float> hitAddNum)
        {
            HitAddNum = hitAddNum;
        }

        private Vector<float> HitAddNum { get; }

        // public float HpMultiAdd { get; }0
        //
        // public float HealMultiAdd { get; }1
        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, HitAddNum);
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

        public SurvivalAboutPassiveEffect(Vector<float> survivalMultiAdd)
        {
            SurvivalMultiAdd = survivalMultiAdd;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            return new SurvivalAboutPassiveEffect(Vector.Multiply(level, SurvivalMultiAdd));
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

    public readonly struct TrapEffect : IPassiveTraitEffect
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

    public readonly struct OtherAttrPassiveEffect : IPassiveTraitEffect
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
            var multiply = Vector.Multiply(level, OtherAttrAdd);
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

        // private static Vector<float> Zero()
        // {
        //     return new Vector<float>(new[] {0f, 0f, 0f, 0f, 0f});
        // }
    }

    public readonly struct TickAddEffect : IPassiveTraitEffect
    {
        public TickAddEffect(Vector<float> tickAddMulti)
        {
            TickAddMulti = tickAddMulti;
        }

        public IPassiveTraitEffect GenEffect(uint level)
        {
            var multiply = Vector.Multiply(level, TickAddMulti);
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

    public readonly struct AtkAboutPassiveEffect : IPassiveTraitEffect
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