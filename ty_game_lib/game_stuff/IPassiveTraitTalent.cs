namespace game_stuff
{
    public interface IPassiveTraitTalent
    {
        public uint Level { get; set; }
    }

    public interface IAttackGainTalent : IPassiveTraitTalent
    {
    }
    
}