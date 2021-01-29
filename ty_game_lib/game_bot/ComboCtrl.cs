namespace game_bot
{
    public class ComboCtrl
    {
        private int ComboMax { get; }

        private int ComboNow { get; set; }

        public bool ComboOn { get; set; }

        public ComboCtrl(int comboMax)
        {
            ComboMax = comboMax;
            ComboNow = 0;
            ComboOn = false;
        }
    }
}