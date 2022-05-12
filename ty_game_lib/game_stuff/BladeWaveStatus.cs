using collision_and_rigid;

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
        public string GetDetails()
        {
            return
                $"剑风范围加成:{MathTools.Round(WaveRange,2)} 剑风伤害加成:{MathTools.Round(DamageMulti,2)}";
        }
    }
}