using collision_and_rigid;

namespace game_stuff
{
    public class DamageMultiStatus
    {
        private float NoArmorMulti { get; set; }

        private float NoShieldMulti { get; set; }

        private float LossPercentHpMulti { get; set; }

        public DamageMultiStatus()
        {
            NoArmorMulti = 0f;
            NoShieldMulti = 0f;
            LossPercentHpMulti = 0f;
        }

        public void RefreshSurvivalDmgMulti(float[] vector)
        {
            NoShieldMulti += vector[0];
            NoArmorMulti += vector[1];
            LossPercentHpMulti += vector[2];
        }

        public float GetTotalMulti(SurvivalStatus survivalStatus)
        {
            var noShieldMulti = survivalStatus.NowShield > 0 ? 0 : NoShieldMulti;
            var noArmorMulti = survivalStatus.NowArmor > 0 ? 0 : NoArmorMulti;
            var max = MathTools.Max(0, survivalStatus.MaxHp - survivalStatus.NowHp);
            var survivalStatusMaxHp = (float) max / survivalStatus.MaxHp * LossPercentHpMulti;
            var statusMaxHp = 1 + noShieldMulti + noArmorMulti + survivalStatusMaxHp;
            return statusMaxHp;
        }
    }
}