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
            int doNotMinTick,
            int showDelayMax)
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
            DoNothingRestTick = 0;
        }

        private ImmutableArray<(int, SkillAction)> StackWeightToSkillActions { get; }
        private int Total { get; }
        private int ComboTotal { get; }
        private int DoNothingRestTick { get; set; }
        private int DoNotMaxTick { get; }
        private int DoNotMinTick { get; }
        private SkillAction? NowThinkAction { get; set; }
        private int ShowDelayNow { get; set; }
        private int ShowDelayMax { get; }


        public SkillAction? NoThinkAct(Random random)
        {
            if (DoNothingRestTick > 0)
            {
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

        private bool NeedThink()
        {
            var needThink = DoNothingRestTick <= 0 && NowThinkAction == null;
#if DEBUG
            Console.Out.WriteLine($"think is {needThink} {NowThinkAction} ");
#endif
            return needThink;
        }

        internal SkillAction GetComboAction(Random random)
        {
            return StackWeightToSkillActions.GetWeightThings(random.Next(ComboTotal)).things;
        }


        internal SkillAction? GetAction()
        {
            var nowThinkAction = NowThinkAction;
            NowThinkAction = null;
            return DoNothingRestTick > 0 ? null : nowThinkAction;
        }

        private void ThinkAAct(Random random)
        {
            var next = random.Next(Total);
            var (isGetOk, things) = StackWeightToSkillActions.GetWeightThings(next);


            if (isGetOk)
            {
                NowThinkAction = things;
            }
            else
            {
                var i = random.Next(DoNotMinTick, DoNotMaxTick);
                DoNothingRestTick = i;

                NowThinkAction = null;
            }

            ShowDelayNow = 0;
        }

        private SkillAction? ShowThink()
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
#if DEBUG
                Console.Out.WriteLine($"think is ");
#endif
                ThinkAAct(random);
            }

            if (ShowDelayNow <= ShowDelayMax) ShowDelayNow++;
            if (DoNothingRestTick > 0) DoNothingRestTick--;
            return CanShowThinkAction() ? ShowThink() : null;
        }
    }
}