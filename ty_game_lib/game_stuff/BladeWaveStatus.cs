namespace game_stuff
{
    public class BladeWaveStatus
    {
        public float WaveRange { get; private set; }

        public float DamageMulti { get; private set; }

        public BladeWaveStatus()
        {
            WaveRange = 0;
            DamageMulti = 1;
        }
    }
}