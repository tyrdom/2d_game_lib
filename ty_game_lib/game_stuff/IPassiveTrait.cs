namespace game_stuff
{
    public interface IPassiveTrait
    {
        public bool AddTrait(IPassiveTraitTalent talent);

        public bool RemoveTrait(IPassiveTraitTalent talent);

        public uint Level { get; set; }
    }

    public interface IAttackTrait : IPassiveTrait
    {
        
    }
}