using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using game_config;
using game_stuff;

namespace game_bot
{
    public class FirstSkillCtrl
    {
        public FirstSkillCtrl(
            IEnumerable<(int weight, SkillAction skillAction)> stackWeightToSkillActions,
            int noActWeight,
            int doNotMaxTick,
            int doNotMinTick)
        {
            var (weightOverList, i) = stackWeightToSkillActions.GetWeightOverList();
            StackWeightToSkillActions = weightOverList;
            Total = noActWeight + i;
            ComboTotal = i;
            DoNotMaxTick = doNotMaxTick + 1;
            DoNotMinTick = doNotMinTick;

            DoNothingRestTick = 0;
        }

        private ImmutableArray<(int, SkillAction)> StackWeightToSkillActions { get; }
        private int Total { get; }
        private int ComboTotal { get; }
        private int DoNothingRestTick { get; set; }
        private int DoNotMaxTick { get; }
        private int DoNotMinTick { get; }


        public SkillAction? GetAct(Random random)
        {
            if (DoNothingRestTick > 0)
            {
                DoNothingRestTick--;
                return null;
            }

            var next = random.Next(Total);
            var (isGetOk, things) = StackWeightToSkillActions.GetWeightThings(next);
            if (isGetOk)
            {
                return things;
            }

            var i = random.Next(DoNotMinTick, DoNotMaxTick);
            DoNothingRestTick = i;

            return null;
        }

        internal SkillAction GetComboAction(Random random)
        {
            return StackWeightToSkillActions.GetWeightThings(random.Next(ComboTotal)).things;
        }
    }
}