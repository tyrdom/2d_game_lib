using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Force.DeepCloner;
using game_config;
using Newtonsoft.Json.Serialization;

namespace game_stuff
{
    public interface IPlayingBuff
    {
        int BuffId { get; }
        int RestTick { get; set; }
        bool IsFinish();
        int Stack { get; set; }
        void GoATick();
    }


    public class MakeDamageBuff : IPlayingBuff
    {
        public MakeDamageBuff(int buffId, int restTick, float addDamageMulti, int stack)
        {
            BuffId = buffId;
            RestTick = restTick;
            AddDamageMulti = addDamageMulti;
            Stack = stack;
        }

        public int BuffId { get; }
        public int RestTick { get; set; }
        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }


        public IPlayingBuff Copy()
        {
            return PlayBuffStandard.Copy(this);
        }


        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public static float GetDamageMulti(IEnumerable<MakeDamageBuff> damageBuffs)
        {
            var sum = damageBuffs.Sum(x => x.AddDamageMulti);
            if (sum > 0)
            {
                return 1f + sum;
            }

            return 1f / (1f - sum);
        }

        private float AddDamageMulti { get; }
    }

    public static class PlayBuffStandard
    {
        public static IPlayingBuff Copy(IPlayingBuff playingBuff)
        {
            return playingBuff.DeepClone();
        }

        public static IPlayingBuff GenById(int id)
        {
            var playBuff = GetBuffConfig(id);
            var intTickByTime = CommonConfig.GetIntTickByTime(playBuff.LastTime);
            return playBuff.EffectType switch
            {
                play_buff_effect_type.TakeDamageAdd => new TakeDamageBuff(id, intTickByTime, playBuff.EffectValue, 1),
                play_buff_effect_type.Break => new BreakBuff(id, intTickByTime, 1),
                play_buff_effect_type.MakeDamageAdd => new MakeDamageBuff(id, intTickByTime, playBuff.EffectValue, 1),
                play_buff_effect_type.Tough => new ToughBuff(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static play_buff GetBuffConfig(int id)
        {
            return LocalConfig.Configs.play_buffs.TryGetValue(id, out var playBuff)
                ? playBuff
                : throw new DirectoryNotFoundException($"not such a play buff {id}");
        }

        public static void GoATick(IPlayingBuff playingBuff)
        {
            playingBuff.RestTick -= 1;
        }


        public static bool IsFinish(IPlayingBuff playingBuff)
        {
            return playingBuff.RestTick <= 0 || playingBuff.Stack == 0;
        }


        public static void AddBuffs(
            Dictionary<play_buff_effect_type, Dictionary<int, IPlayingBuff>> playingBuffsDictionary,
            IEnumerable<IPlayingBuff> playingBuffsToAdd
        )
        {
            foreach (var aPlayBuff in playingBuffsToAdd)
            {
                AddABuff(playingBuffsDictionary, aPlayBuff);
            }
        }

        public static void AddABuff(
            Dictionary<play_buff_effect_type, Dictionary<int, IPlayingBuff>> playingBuffsDictionary,
            IPlayingBuff aPlayBuff)
        {
            var idBuffId = aPlayBuff.BuffId;
            var playBuff = GetBuffConfig(idBuffId);
            var playBuffEffectType = playBuff.EffectType;
            var playBuffStackMode = playBuff.StackMode;
            if (!playingBuffsDictionary.TryGetValue(playBuffEffectType, out var dictionary) ||
                !dictionary.TryGetValue(idBuffId, out var playingBuff)) return;
            switch (playBuffStackMode)
            {
                case stack_mode.normal:
                    playingBuff.RestTick = aPlayBuff.RestTick;
                    playingBuff.Stack = 1;
                    break;
                case stack_mode.time:
                    playingBuff.RestTick += aPlayBuff.RestTick;
                    playingBuff.Stack = 1;
                    break;
                case stack_mode.stack:
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
    }

    public class TakeDamageBuff : IPlayingBuff
    {
        public TakeDamageBuff(int buffId, int restTick, float takeDamageAdd, int stack)
        {
            BuffId = buffId;
            RestTick = restTick;
            TakeDamageAdd = takeDamageAdd;
            Stack = stack;
        }

        public int BuffId { get; }
        public int RestTick { get; set; }

        public float TakeDamageAdd { get; }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }
    }

    public class BreakBuff : IPlayingBuff
    {
        public BreakBuff(int buffId, int restTick, int stack)
        {
            BuffId = buffId;
            RestTick = restTick;
            Stack = stack;
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

        public IPlayingBuff Copy()
        {
            return PlayBuffStandard.Copy(this);
        }
    }
}