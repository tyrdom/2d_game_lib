using System;
using System.Collections.Generic;
using System.Linq;

namespace game_stuff
{
    public interface IPlayingBuffConfig
    {
        uint OriginRestTick { get; }
        IPlayingBuff CreateBuff(uint buffId);
        StackMode StackMode { get; }
    }


    public interface IPlayingBuff
    {
        uint BuffId { get; }
        int RestTick { get; set; }
        bool IsFinish();
        uint Stack { get; set; }
        void GoATick();
    }

    public enum StackMode
    {
        More,
        Stack,
        Time
    }

    public class MakeDamageBuffConfig : IPlayingBuffConfig
    {
        public MakeDamageBuffConfig(uint originRestTick, float addDamageMulti, StackMode stackMode)
        {
            OriginRestTick = originRestTick;
            AddDamageMulti = addDamageMulti;
            StackMode = stackMode;
        }

        public uint OriginRestTick { get; }


        private float AddDamageMulti { get; }

        public IPlayingBuff CreateBuff(uint buffId)
        {
            throw new NotImplementedException();
        }

        public StackMode StackMode { get; }
    }

    public class AddDamageBuff : IPlayingBuff
    {
        public AddDamageBuff(uint buffId, int restTick, float addDamageMulti, uint stack)
        {
            BuffId = buffId;
            RestTick = restTick;
            AddDamageMulti = addDamageMulti;
            Stack = stack;
        }

        public uint BuffId { get; }
        public int RestTick { get; set; }
        public uint Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public float AddDamageMulti { get; }
    }

    public static class PlayBuffStandard
    {
        public static void GoATick(IPlayingBuff playingBuff)
        {
            playingBuff.RestTick -= 1;
        }


        public static bool IsFinish(IPlayingBuff playingBuff)
        {
            return playingBuff.RestTick <= 0;
        }


        public static float StackDamage(IEnumerable<AddDamageBuff> damageBuffs)
        {
            var sum = damageBuffs.Sum(x => x.AddDamageMulti);
            return sum;
        }

        public static List<IPlayingBuff> AddBuffs(List<IPlayingBuff> playingBuffs1,
            IEnumerable<IPlayingBuff> playingBuffs2
        )
        {
            playingBuffs1.AddRange(playingBuffs2);

            var groupBy = playingBuffs1.GroupBy(x => x.BuffId);
            var buffs = new List<IPlayingBuff>();
            foreach (var grouping in groupBy)
            {
                if (LocalConfig.BuffConfigs.TryGetValue(grouping.Key, out var playingBuffConfig))
                {
                    var valueStackMode = playingBuffConfig.StackMode;
                    switch (valueStackMode)
                    {
                        case StackMode.More:
                            var enumerable = grouping.Select(x => x);
                            buffs.AddRange(enumerable);
                            break;
                        case StackMode.Stack:
                            var sum = grouping.Sum(x => x.Stack);
                            var stack = grouping.First();
                            stack.Stack = (uint) sum;
                            buffs.Add(stack);
                            break;
                        case StackMode.Time:
                            var sum2 = grouping.Sum(x => x.RestTick);
                            var time = grouping.First();
                            time.RestTick = sum2;
                            buffs.Add(time);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return buffs;
        }
    }
}