namespace game_stuff
{
    public interface IPassiveTraitEffect
    {
    }


    public struct AtkAboutPassiveEffect : IPassiveTraitEffect
    {
        public AtkAboutPassiveEffect(float mainAtkMultiAdd, float shardedNumAdd,
            float backStabAddPerLevel)
        {
            MainAtkMultiAddPerLevel = mainAtkMultiAdd;
            ShardedNumAddPerLevel = shardedNumAdd;
            BackStabAddPerLevel = backStabAddPerLevel;
        }

        public float MainAtkMultiAddPerLevel { get; }
        public float ShardedNumAddPerLevel { get; }
        public float BackStabAddPerLevel { get; }

        public AtkAboutPassiveEffect GenEffect(uint level)
        {
            return new AtkAboutPassiveEffect(MainAtkMultiAddPerLevel * level, ShardedNumAddPerLevel * level,
                BackStabAddPerLevel * level);
        }
    }
}