using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class SelfEffect : IEffectMedia
    {
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