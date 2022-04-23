using collision_and_rigid;

namespace game_stuff
{
    public class DamageMultiStatus
    {
        private float NoArmorMulti { get; set; }

        private float NoShieldMulti { get; set; }

        private float LossPercentHpMulti { get; set; }

        private float LossPercentArmorMulti { get; set; }

        public float OnBreakMulti { get; private set; }

        private float ProtectDamageMulti { get; set; }

        public DamageMultiStatus()
        {
            NoArmorMulti = 0f;
            NoShieldMulti = 0f;
            LossPercentHpMulti = 0f;
            LossPercentArmorMulti = 0f;
            OnBreakMulti = 0f;
            ProtectDamageMulti = 0f;
        }

        public void RefreshSurvivalDmgMulti(float[] vector)
        {
            NoShieldMulti = vector[0];
            NoArmorMulti = vector[1];
            LossPercentHpMulti = vector[2];
            LossPercentArmorMulti = vector[3];
            OnBreakMulti = vector[4];
            ProtectDamageMulti = vector[5];
        }

        public float GetTotalMulti(SurvivalStatus survivalStatus, float pf)
        {
            var noShieldMulti = survivalStatus.NowShield > 0 ? 0 : NoShieldMulti;
            var noArmorMulti = survivalStatus.NowArmor > 0 ? 0 : NoArmorMulti;
            var lossP1 = survivalStatus.NowHp == 1 ? 1f : MathTools.Max(0, 1f - survivalStatus.HpPercent());
            var lossP2 = MathTools.Max(0, 1 - survivalStatus.ArmorPercent());
            var survivalStatusMaxHp = lossP1 * LossPercentHpMulti;
            var survivalStatusMaxArmor = lossP2 * LossPercentArmorMulti;
            var protectDamageMulti = ProtectDamageMulti * pf;
            var statusMaxHp = noShieldMulti + noArmorMulti + survivalStatusMaxHp + survivalStatusMaxArmor +
                              protectDamageMulti;
            return statusMaxHp;
        }

        public string GetDetails()
        {
            return
                $"无盾时攻击加成:{MathTools.Round(NoShieldMulti, 2)} 无甲时攻击加成:{MathTools.Round(NoArmorMulti, 2)} 生命损失增伤:{MathTools.Round(LossPercentHpMulti, 2)} 装甲损失增伤:{MathTools.Round(LossPercentArmorMulti, 2)} 破坏增伤:{OnBreakMulti} 保护值增伤:{ProtectDamageMulti}";
        }
    }
}