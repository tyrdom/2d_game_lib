using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using game_config;
using game_stuff;

namespace game_bot
{
    public class FirstSkillCtrl
    {
        public FirstSkillCtrl(IEnumerable<(int weight, SkillAction skillAction)> stackWeightToSkillActions,
            int noActWeight,
            int doNotMaxTick,
            int doNotMinTick, int showDelayMax)
        {
            var (weightOverList, i) = stackWeightToSkillActions.GetWeightOverList();
            StackWeightToSkillActions = weightOverList;
            Total = noActWeight + i;
            ComboTotal = i;
            DoNotMaxTick = doNotMaxTick + 1;
            DoNotMinTick = doNotMinTick;
            NowThinkAction = null;
            ShowDelayNow = 0;
            ShowDelayMax = showDelayMax;
            DoNotRestTick = 0;
        }

        private ImmutableArray<(int, SkillAction)> StackWeightToSkillActions { get; }

        private int Total { get; }

        private int ComboTotal { get; }
        public int DoNotRestTick { get; set; }
        private int DoNotMaxTick { get; }
        private int DoNotMinTick { get; }
        public SkillAction? NowThinkAction { get; set; }
        private int ShowDelayNow { get; set; }
        private int ShowDelayMax { get; }

        internal bool NeedThink()
        {
            return DoNotRestTick <= 0 && NowThinkAction == null;
        }

        internal SkillAction GetComboAction(Random random)
        {
            return StackWeightToSkillActions.GetWeightThings(random.Next(ComboTotal)).things;
        }


        internal SkillAction? GetAction(Random random)
        {
            return DoNotRestTick > 0 ? null : NowThinkAction;
        }

        private void ThinkAAct(Random random)
        {
            var next = random.Next(Total);
            var (isGetOk, things) = StackWeightToSkillActions.GetWeightThings(next);
            if (isGetOk)
            {
                ShowDelayNow = 0;
                NowThinkAction = things;
            }
            else
            {
                var i = random.Next(DoNotMinTick, DoNotMaxTick);
                DoNotRestTick = i;
                ShowDelayNow = 0;
                NowThinkAction = null;
            }
        }

        public SkillAction? ShowThink()
        {
            return NowThinkAction;
        }

        private bool CanShowThinkAction()
        {
            return ShowDelayNow == ShowDelayMax;
        }


        internal SkillAction? GoATick(Random random)
        {
            if (NeedThink())
            {
                ThinkAAct(random);
            }

            if (ShowDelayNow <= ShowDelayMax) ShowDelayNow++;
            if (DoNotRestTick > 0) DoNotRestTick--;
            return CanShowThinkAction() ? ShowThink() : null;
        }
    }
}