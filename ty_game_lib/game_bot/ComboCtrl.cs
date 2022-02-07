using System;
using game_stuff;

namespace game_bot
{
    public class ComboCtrl
    {
        private int ComboMax { get; }
        private int ComboNow { get; set; }

        public void SetComboStart()
        {
            if (ComboNow == 0)
            {
                ComboNow = ComboMax;
            }
        }


        public ComboCtrl(int comboMax)
        {
            ComboMax = comboMax;
            ComboNow = 0;
        }

        public bool TryGetNextSkillAction(FirstSkillCtrl firstOrDefault, Random random, out SkillAction? skillAction)
        {
            skillAction = null;
            if (ComboNow <= 0) return false;
            var comboAction = firstOrDefault.GetComboAction(random);
            skillAction = comboAction;
            ComboNow--;
            return true;
        }
    }
}