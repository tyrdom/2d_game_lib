using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game_config;

namespace game_stuff
{
    public interface IPlayingBuff
    {
        int BuffId { get; }
        int RestTick { get; set; }
        bool IsFinish();
        int Stack { get; set; }
        void GoATick();
        bool UseStack { get; }
    }


    public class MakeDamageBuff : IPlayingBuff, IDamageAboutBuff
    {
        public MakeDamageBuff(int buffId, int restTick, float addDamageMulti, int stack, bool useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            AddDamageMulti = addDamageMulti;
            Stack = stack;
            UseStack = useStack;
        }

        public int BuffId { get; }
        public int RestTick { get; set; }
        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public bool UseStack { get; }


        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }


        public float AddDamageMulti { get; }
    }

    public interface IDamageAboutBuff
    {
        float AddDamageMulti { get; }
    }

    public static class PlayBuffStandard
    {
        public static IPlayingBuff GenById(int id)
        {
            var playBuff = GetBuffConfig(id);
            var intTickByTime = (int)(playBuff.LastTime);
            var playBuffUseStack = playBuff.UseStack;
            return playBuff.EffectType switch
            {
                play_buff_effect_type.TakeDamageAdd => new TakeDamageBuff(id, intTickByTime, playBuff.EffectValue, 1,
                    playBuffUseStack),
                play_buff_effect_type.Break => new BreakBuff(id, intTickByTime, 1, playBuffUseStack),
                play_buff_effect_type.MakeDamageAdd => new MakeDamageBuff(id, intTickByTime, playBuff.EffectValue, 1,
                    playBuffUseStack),
                play_buff_effect_type.Tough => new ToughBuff(id, intTickByTime, 1, playBuffUseStack),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static play_buff GetBuffConfig(int id)
        {
            return CommonConfig.Configs.play_buffs.TryGetValue(id, out var playBuff)
                ? playBuff
                : throw new KeyNotFoundException($"not such a play buff {id}");
        }

        public static void GoATick(this IPlayingBuff playingBuff)
        {
            playingBuff.RestTick -= 1;
        }


        public static bool IsFinish(this IPlayingBuff playingBuff)
        {
            return playingBuff.RestTick <= 0 || playingBuff.Stack == 0;
        }


        public static void AddBuffs(
            Dictionary<int, IPlayingBuff> playingBuffsDictionary,
            IEnumerable<IPlayingBuff> playingBuffsToAdd
        )
        {
            foreach (var aPlayBuff in playingBuffsToAdd)
            {
                AddABuff(playingBuffsDictionary, aPlayBuff);
            }
        }

        public static void UseBuff(this IPlayingBuff playingBuff)
        {
            if (!playingBuff.UseStack) return;
            playingBuff.Stack -= 1;
        }

        public static float GetDamageMulti(this IEnumerable<IDamageAboutBuff> damageBuffs)
        {
            var sum = damageBuffs.Sum(x => x.AddDamageMulti);
            if (sum > 0)
            {
                return 1f + sum;
            }

            return 1f / (1f - sum);
        }

        public static void AddABuff(
            Dictionary<int, IPlayingBuff> playingBuffsDictionary,
            IPlayingBuff aPlayBuff)
        {
            var idBuffId = aPlayBuff.BuffId;
            var playBuff = GetBuffConfig(idBuffId);
            var playBuffStackMode = playBuff.StackMode;
            if (!playingBuffsDictionary.TryGetValue(idBuffId, out var playingBuff)) return;
            switch (playBuffStackMode)
            {
                case stack_mode.OverWrite:
                    playingBuff.RestTick = aPlayBuff.RestTick;
                    playingBuff.Stack = 1;
                    break;
                case stack_mode.Time:
                    playingBuff.RestTick += aPlayBuff.RestTick;
                    playingBuff.Stack = 1;
                    break;
                case stack_mode.Stack:
                    playingBuff.RestTick = aPlayBuff.RestTick;
                    playingBuff.Stack += aPlayBuff.Stack;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class ToughBuff : IPlayingBuff
    {
        public ToughBuff(int buffId, int restTick, int stack, bool useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            Stack = stack;
            UseStack = useStack;
        }

        public int BuffId { get; }
        public int RestTick { get; set; }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public bool UseStack { get; }
    }

    public class TakeDamageBuff : IPlayingBuff, IDamageAboutBuff
    {
        public TakeDamageBuff(int buffId, int restTick, float takeDamageAdd, int stack, bool useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            AddDamageMulti = takeDamageAdd;
            Stack = stack;
            UseStack = useStack;
        }

        public int BuffId { get; }
        public int RestTick { get; set; }
        public float AddDamageMulti { get; }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public bool UseStack { get; }
    }

    public class BreakBuff : IPlayingBuff
    {
        public BreakBuff(int buffId, int restTick, int stack, bool useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            Stack = stack;
            UseStack = useStack;
        }

        public int BuffId { get; }
        public int RestTick { get; set; }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public bool UseStack { get; }
    }
}