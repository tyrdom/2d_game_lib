namespace game_stuff
{
    public interface IPassiveTrait
    {
        public uint PassId { get; }

        public void AddLevel(uint num);

        public void RemoveLevel(uint num);
        public void ResetLevel();
        public uint Level { get; set; }
    }

    public static class PassiveStandard
    {
        public static void AddLevel(IPassiveTrait passiveTrait, uint level)
        {
            passiveTrait.Level += level;
        }

        public static void RemoveLevel(IPassiveTrait passiveTrait, uint level)
        {
            passiveTrait.Level = passiveTrait.Level <= level ? 0 : passiveTrait.Level - level;
        }

        public static void ResetLevel(IPassiveTrait passiveTrait)
        {
            passiveTrait.Level = 0;
        }
    }

    public class AtkAboutPassive : IPassiveTrait
    {
        public AtkAboutPassive(uint passId, uint level, float mainAtkAdd, float shardedNumAdd, float backStabAddPerLevel)
        {
            PassId = passId;
            Level = level;
            MainAtkAddPerLevel = mainAtkAdd;
            ShardedNumAddPerLevel = shardedNumAdd;
            BackStabAddPerLevel = backStabAddPerLevel;
        }

        public uint PassId { get; }

        public void AddLevel(uint num)
        {
            PassiveStandard.AddLevel(this, num);
        }

        public void RemoveLevel(uint num)
        {
            PassiveStandard.RemoveLevel(this, num);
        }

        public void ResetLevel()
        {
            PassiveStandard.ResetLevel(this);
        }

        public uint Level { get; set; }

        public float MainAtkAddPerLevel { get; }
        public float ShardedNumAddPerLevel { get; }

        public float BackStabAddPerLevel { get; }
    }
}