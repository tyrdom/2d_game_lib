namespace game_stuff
{
    public class BladeWaveStatus
    {
        public float WaveRange { get; private set; }

        public float DamageMulti { get; private set; }

        public BladeWaveStatus()
        {
            WaveRange = 0f;
            DamageMulti = 0f;
        }

        public void PassiveEffectChange(float[] vector)
        {
            WaveRange = vector[0];
            DamageMulti = vector[1];
        }
    }
}