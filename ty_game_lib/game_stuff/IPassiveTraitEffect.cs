namespace game_stuff
{
    public interface IPassiveTraitEffect
    {
    }

    public readonly struct SurvivalAboutPassiveEffect : IPassiveTraitEffect
    {
        public float HpMultiAdd { get; }

        public float HealMultiAdd { get; }

        public float ArmorMultiAdd { get; }

        public float DefMultiAdd { get; }

        public float ShieldMultiAdd { get; }

        public float ShieldRegMultiAdd { get; }

        public float ShieldInstabilityMultiAdd { get; }

        private SurvivalAboutPassiveEffect(float hpMultiAdd, float healMultiAdd, float armorMultiAdd, float defMultiAdd,
            float shieldMultiAdd, float shieldRegMultiAdd, float shieldInstabilityMultiAdd)
        {
            HpMultiAdd = hpMultiAdd;
            HealMultiAdd = healMultiAdd;
            ArmorMultiAdd = armorMultiAdd;
            DefMultiAdd = defMultiAdd;
            ShieldMultiAdd = shieldMultiAdd;
            ShieldRegMultiAdd = shieldRegMultiAdd;
            ShieldInstabilityMultiAdd = shieldInstabilityMultiAdd;
        }

        public SurvivalAboutPassiveEffect GenEffect(uint level)
        {
            return new SurvivalAboutPassiveEffect(
                HpMultiAdd * level,
                HealMultiAdd * level,
                ArmorMultiAdd * level,
                DefMultiAdd * level,
                ShieldMultiAdd * level,
                ShieldRegMultiAdd * level,
                ShieldInstabilityMultiAdd * level
            );
        }
    }

    public readonly struct AtkAboutPassiveEffect : IPassiveTraitEffect
    {
        private AtkAboutPassiveEffect(float mainAtkMultiAdd, float shardedNumAdd,
            float backStabAdd)
        {
            MainAtkMultiAdd = mainAtkMultiAdd;
            ShardedNumAdd = shardedNumAdd;
            BackStabAdd = backStabAdd;
        }

        public float MainAtkMultiAdd { get; }
        public float ShardedNumAdd { get; }
        public float BackStabAdd { get; }

        public AtkAboutPassiveEffect GenEffect(uint level)
        {
            return new AtkAboutPassiveEffect(MainAtkMultiAdd * level, ShardedNumAdd * level,
                BackStabAdd * level);
        }
    }
}