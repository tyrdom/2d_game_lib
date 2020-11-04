namespace game_stuff
{
    public readonly struct Regeneration
    {
        public Regeneration(float healMulti, float fixMulti, float shieldMulti)
        {
            HealMulti = healMulti;
            FixMulti = fixMulti;
            ShieldMulti = shieldMulti;
        }

        public float HealMulti { get; }
        public float FixMulti { get; }
        public float ShieldMulti { get; }
    }
}