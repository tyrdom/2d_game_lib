namespace game_bot
{
    public class ComboCtrl
    {
        private int ComboMax { get; }

        private int ComboNow { get; set; }

        private bool ComboOn { get; set; }

        public ComboCtrl(int comboMax)
        {
            ComboMax = comboMax;
            ComboNow = 0;
        }

        public bool CanCombo()
        {
            return ComboNow > 0;
        }

        public void ActACombo()
        {
            ComboNow--;
            if (ComboNow<=0)
            {
                ComboOn = false;
            }
        }

        public void ComboTurnOn()
        {
            if (ComboOn) return;
            ComboNow = ComboMax;
            ComboOn = true;
        }

        public void ComboLoss()
        {
            ComboNow = 0;
            ComboOn = false;
        }
    }
}