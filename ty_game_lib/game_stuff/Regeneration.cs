namespace game_stuff
{
    public readonly struct Regeneration
    {
        public Regeneration(float healMulti, float fixMulti, float shieldMulti, float reloadMulti)
        {
            HealMulti = healMulti;
            FixMulti = fixMulti;
            ShieldMulti = shieldMulti;
            ReloadMulti = reloadMulti;
        }

        public float HealMulti { get; }
        public float FixMulti { get; }
        public float ShieldMulti { get; }

        public float ReloadMulti { get; }
    }
}