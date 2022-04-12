using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IBuffMaker
    {
    }

    public class PlayingBuffsMaker : IBuffMaker
    {
        public PlayingBuffsMaker(play_buff_id[] playBuffIds)
        {
            PlayBuffIds = playBuffIds;
        }

        public PlayingBuffsMaker(IEnumerable<string> playBuffIds)
        {
            var buffIds = playBuffIds.Select(x => (play_buff_id)Enum.Parse(typeof(play_buff_id), x, true));
            PlayBuffIds = buffIds.ToArray();
        }

        private play_buff_id[] PlayBuffIds { get; }

        public IEnumerable<IPlayingBuff> GenBuffs(CharacterStatus? charMark = null)
        {
            var playingBuffs = PlayBuffIds.Select(x => PlayBuffStandard.GenById(x, charMark));
            return playingBuffs;
        }
    }

    public interface IPlayingBuff
    {
        play_buff_id BuffId { get; }
        int RestTick { get; set; }
        bool IsFinish();
        int Stack { get; set; }
        void GoATick();
        int UseStack { get; }
        void ActiveWhenUse(CharacterStatus characterStatus);
    }


    public class MakeDamageBuff : IPlayingBuff, IDamageAboutBuff
    {
        public MakeDamageBuff(play_buff_id buffId, int restTick, float addDamageMulti, int stack, int useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            AddDamageMulti = addDamageMulti;
            Stack = stack;
            UseStack = useStack;
        }

        public play_buff_id BuffId { get; }
        public int RestTick { get; set; }
        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
        }


        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }


        public float AddDamageMulti { get; }

        public float GetAddDamage()
        {
            return AddDamageMulti * Stack;
        }
    }

    public interface IDamageAboutBuff
    {
        float AddDamageMulti { get; }
        float GetAddDamage();
    }

    public static class PlayBuffStandard
    {
        public static IPlayingBuff GenById(string id)
        {
            var o = (play_buff_id)Enum.Parse(typeof(play_buff_id), id, true);
            var genById = GenById(o);
            return genById;
        }

        public static CharacterStatus? GetCharMark(this IPlayingBuff playingBuff)
        {
            return playingBuff is PullMark pullMark ? pullMark.TargetMark : null;
        }

        public static IPlayingBuff Copy(this IPlayingBuff playingBuff)
        {
            var playBuffId = playingBuff.BuffId;
            var playingBuffStack = playingBuff.Stack;
            var characterStatus = playingBuff.GetCharMark();
            var genById = GenById(playBuffId, characterStatus);
            genById.Stack = playingBuffStack;
            return genById;
        }

        public static IPlayingBuff GenById(play_buff_id id, CharacterStatus? charMark = null)
        {
            var playBuff = GetBuffConfig(id);

            return PlayingBuff(charMark, playBuff);
        }

        private static IPlayingBuff PlayingBuff(CharacterStatus? charMark, play_buff playBuff)
        {
            var id = playBuff.id;
            var intTickByTime = (int)playBuff.LastTime;
            var playBuffUseStack = playBuff.UseStack;
            return playBuff.EffectType switch
            {
                play_buff_effect_type.TakeDamageAdd => new TakeDamageBuff(id, intTickByTime, playBuff.EffectValue, 1,
                    playBuffUseStack),
                play_buff_effect_type.Break => new BreakBuff(id, intTickByTime, 1, playBuffUseStack),
                play_buff_effect_type.MakeDamageAdd => new MakeDamageBuff(id, intTickByTime, playBuff.EffectValue, 1,
                    playBuffUseStack),
                play_buff_effect_type.Tough => new ToughBuff(id, intTickByTime, 1, playBuffUseStack),
                play_buff_effect_type.ToughUp => new ToughUpBuff(id, intTickByTime, playBuff.EffectValue, 1,
                    playBuffUseStack),
                play_buff_effect_type.ChangeComboStatus => new ChangeComboStatus(id, intTickByTime,
                    playBuff.EffectValue, 1, playBuffUseStack),
                play_buff_effect_type.PullMark => new PullMark(id, intTickByTime, playBuff.EffectString, 1,
                    playBuffUseStack, charMark),
                play_buff_effect_type.MaxSpeedChange => new MaxSpeedChangeBuff(id, intTickByTime, playBuff.EffectValue,
                    1,
                    playBuffUseStack),
                play_buff_effect_type.PowerUp => new PowerUpBuff(id, intTickByTime, playBuff.EffectValue, 1,
                    playBuffUseStack),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static play_buff GetBuffConfig(play_buff_id id)
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
            return playingBuff.RestTick <= 0 || playingBuff.Stack <= 0;
        }


        public static void AddBuffs(
            Dictionary<play_buff_id, IPlayingBuff> playingBuffsDictionary,
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
            playingBuff.Stack -= playingBuff.UseStack;
        }

        public static float GetDamageMultiAdd(this IEnumerable<IDamageAboutBuff> damageBuffs)
        {
            var sum = damageBuffs.Sum(x => MathTools.Max(0, x.GetAddDamage()));
            return sum;
        }

        public static float GetDamageMultiDecrease(this IEnumerable<IDamageAboutBuff> damageBuffs)
        {
            var sum = damageBuffs.Sum(x => MathTools.Min(0, x.GetAddDamage()));
            return -sum;
        }

        public static void AddABuff(
            Dictionary<play_buff_id, IPlayingBuff> playingBuffsDictionary,
            IPlayingBuff aPlayBuff)
        {
            var idBuffId = aPlayBuff.BuffId;
#if DEBUG
            Console.Out.WriteLine($"to add buff {idBuffId.ToString()} restTick {aPlayBuff.RestTick}");
#endif
            var playBuff = GetBuffConfig(idBuffId);
            var playBuffStackMode = playBuff.StackMode;
            if (playingBuffsDictionary.TryGetValue(idBuffId, out var playingBuff))
            {
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
            else
            {
                playingBuffsDictionary[idBuffId] = aPlayBuff;
            }
#if DEBUG
            var aggregate = playingBuffsDictionary.Keys.Aggregate("", (s, x) => s + " " + x);
            Console.Out.WriteLine($"buffs is {aggregate}");
#endif
        }
    }

    public class ChangeComboStatus : IPlayingBuff
    {
        public ChangeComboStatus(play_buff_id id, int intTickByTime, float playBuffEffectValue, int i,
            int playBuffUseStack)
        {
            BuffId = id;
            RestTick = intTickByTime;
            ComboStatusFix = (int)playBuffEffectValue;
            Stack = i;
            UseStack = playBuffUseStack;
        }

        public play_buff_id BuffId { get; }
        public int RestTick { get; set; }

        public int ComboStatusFix { get; }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
        }
    }

    public class PullMark : IPlayingBuff
    {
        public PullMark(play_buff_id id, int intTickByTime, string playBuffEffectString, int i, int playBuffUseStack,
            CharacterStatus? charMark)
        {
            TargetMark = charMark ?? throw new Exception("mark cant be null");
            BuffId = id;
            RestTick = intTickByTime;
            var genStunBuffMakerByC = StunBuffStandard.GenStunBuffMakerByC(buff_type.pull_buff, playBuffEffectString);
            PullStunBuffMaker = genStunBuffMakerByC;
            Stack = i;
            UseStack = playBuffUseStack;
        }

        public CharacterStatus TargetMark { get; }
        public IStunBuffMaker PullStunBuffMaker { get; }
        public play_buff_id BuffId { get; }
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

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
#if DEBUG
            Console.Out.WriteLine($"active pull buff to target:{TargetMark.GetId()}");
#endif
            var stunBuff = PullStunBuffMaker.GenBuff(characterStatus.GetPos(),
                TargetMark.GetPos(),
                characterStatus.GetAim(), null, 0,
                TargetMark, characterStatus, false);
            TargetMark.SetStunBuff(stunBuff);
            characterStatus.CharEvents.Add(new DirectHit(TargetMark.GetPos()));
        }
    }

    public class ToughUpBuff : IPlayingBuff
    {
        public ToughUpBuff(play_buff_id id, int intTickByTime, float playBuffEffectValue, int i, int playBuffUseStack)
        {
            BuffId = id;
            RestTick = intTickByTime;
            PlayBuffEffectValue = (int)playBuffEffectValue;
            Stack = i;
            UseStack = playBuffUseStack;
        }

        public play_buff_id BuffId { get; }
        public int RestTick { get; set; }
        public int PlayBuffEffectValue { get; }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
        }
    }

    public class ToughBuff : IPlayingBuff
    {
        public ToughBuff(play_buff_id buffId, int restTick, int stack, int useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            Stack = stack;
            UseStack = useStack;
        }

        public play_buff_id BuffId { get; }
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

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
        }
    }

    public class TakeDamageBuff : IPlayingBuff, IDamageAboutBuff
    {
        public TakeDamageBuff(play_buff_id buffId, int restTick, float takeDamageAdd, int stack, int useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            AddDamageMulti = takeDamageAdd;
            Stack = stack;
            UseStack = useStack;
        }

        public play_buff_id BuffId { get; }
        public int RestTick { get; set; }
        public float AddDamageMulti { get; }

        public float GetAddDamage()
        {
            return AddDamageMulti * Stack;
        }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
        }
    }

    public class BreakBuff : IPlayingBuff
    {
        public BreakBuff(play_buff_id buffId, int restTick, int stack, int useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            Stack = stack;
            UseStack = useStack;
        }

        public play_buff_id BuffId { get; }
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

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
        }
    }

    public class PowerUpBuff : IPlayingBuff
    {
        public PowerUpBuff(play_buff_id buffId, int restTick, float stunForceAddMulti, int stack, int useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            StunForceAddMulti = stunForceAddMulti;
            Stack = stack;
            UseStack = useStack;
        }

        public play_buff_id BuffId { get; }
        public int RestTick { get; set; }

        public float StunForceAddMulti { get; }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
        }
    }

    public class MaxSpeedChangeBuff : IPlayingBuff
    {
        public play_buff_id BuffId { get; }
        public int RestTick { get; set; }

        public float SpeedMaxAddMulti { get; }

        public bool IsFinish()
        {
            return PlayBuffStandard.IsFinish(this);
        }

        public int Stack { get; set; }

        public void GoATick()
        {
            PlayBuffStandard.GoATick(this);
        }

        public int UseStack { get; }

        public void ActiveWhenUse(CharacterStatus characterStatus)
        {
        }

        public MaxSpeedChangeBuff(play_buff_id buffId, int restTick, float speedMaxAddMulti, int stack, int useStack)
        {
            BuffId = buffId;
            RestTick = restTick;
            SpeedMaxAddMulti = speedMaxAddMulti;
            Stack = stack;
            UseStack = useStack;
        }
    }
}