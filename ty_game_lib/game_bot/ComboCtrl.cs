using System;
using game_stuff;

namespace game_bot
{
    public class ComboCtrl
    {
        private int ComboMax { get; }

        private int ComboNow { get; set; }

        private bool ComboOn { get; set; }

        public SkillAction? NextSkillAction { get; set; }


        public bool GenNextSkillAction(FirstSkillCtrl firstSkillCtrl, Random random)
        {
            var b = ComboNow < ComboMax;
            if (b)
            {
                var comboAction = firstSkillCtrl.GetComboAction(random);
                NextSkillAction = comboAction;
                ComboNow++;
            }
            else
            {
                ComboNow = 0;
            }

            return b;
        }

        public bool TryGetNextSkillAction(out SkillAction? skillAction)
        {
            skillAction = null;
            if (NextSkillAction == null)
            {
                return false;
            }

            skillAction = NextSkillAction;
            return true;
        }

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
            if (ComboNow <= 0)
            {
                ComboOn = false;
            }
        }

        public void ComboTurnOn(Random random)
        {
            if (ComboOn) return;
            ComboNow = random.Next(ComboMax);
            if (ComboNow > 0)
            {
                ComboOn = true;
            }
        }

        public void ComboLoss()
        {
            ComboNow = 0;
            ComboOn = false;
        }
    }
}