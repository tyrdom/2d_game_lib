using collision_and_rigid;

namespace game_stuff
{
    public class DamageMultiStatus
    {
        private float NoArmorMulti { get; set; }

        private float NoShieldMulti { get; set; }

        private float LossPercentHpMulti { get; set; }

        private float LossPercentArmorMulti { get; set; }

        public DamageMultiStatus()
        {
            NoArmorMulti = 0f;
            NoShieldMulti = 0f;
            LossPercentHpMulti = 0f;
            LossPercentArmorMulti = 0f;
        }

        public void RefreshSurvivalDmgMulti(float[] vector)
        {
            NoShieldMulti += vector[0];
            NoArmorMulti += vector[1];
            LossPercentHpMulti += vector[2];
            LossPercentArmorMulti += vector[3];
        }

        public float GetTotalMulti(SurvivalStatus survivalStatus)
        {
            var noShieldMulti = survivalStatus.NowShield > 0 ? 0 : NoShieldMulti;
            var noArmorMulti = survivalStatus.NowArmor > 0 ? 0 : NoArmorMulti;
            var lossP1 = MathTools.Max(0, 1 - survivalStatus.HpPercent());
            var lossP2 = MathTools.Max(0, 1 - survivalStatus.ArmorPercent());
            var survivalStatusMaxHp = lossP1 * LossPercentHpMulti;
            var survivalStatusMaxArmor = lossP2 * LossPercentArmorMulti;
            var statusMaxHp = 1 + noShieldMulti + noArmorMulti + survivalStatusMaxHp + survivalStatusMaxArmor;
            return statusMaxHp;
        }
    }
}