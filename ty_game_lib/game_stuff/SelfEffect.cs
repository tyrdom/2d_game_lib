using System.Collections.Generic;
using System.IO;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class SelfEffect : IEffectMedia
    {
        public static SelfEffect GenById(string id)
        {
            if (TempConfig.Configs.self_effects.TryGetValue(id, out var selfEffect))
            {
                return new SelfEffect(selfEffect);
            }
            throw new DirectoryNotFoundException($"not such self effect id::{id}");
        }
        public SelfEffect(self_effect selfEffect)
        {
            var b = selfEffect.HealMulti == 0 && selfEffect.FixMulti == 0 && selfEffect.ShieldMulti == 0 &&
                    selfEffect.ReloadMulti == 0;
            var regeneration = new Regeneration(selfEffect.HealMulti, selfEffect.FixMulti, selfEffect.ShieldMulti,
                selfEffect.ReloadMulti);
            if (b)
            {
                RegenerationBase = null;
            }
            else
            {
                RegenerationBase = regeneration;
            }

            PlayingBuffToAdd = new List<IPlayingBuff>(); //todo Playing buff
        }

        public SelfEffect(List<IPlayingBuff> playingBuffToAdd, Regeneration? regeneration)
        {
            Caster = null;
            PlayingBuffToAdd = playingBuffToAdd;
            RegenerationBase = regeneration;
        }

        public IBattleUnitStatus? Caster { get; set; }

        public void Sign(CharacterStatus characterStatus)
        {
            Caster = characterStatus;
        }

        public List<IPlayingBuff> PlayingBuffToAdd { get; }
        public Regeneration? RegenerationBase { get; }

        public bool CanGoNextTick()
        {
            return false;
        }
    }
}