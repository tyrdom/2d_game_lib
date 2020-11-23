using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class SelfEffect : IEffectMedia
    {
        public SelfEffect(CharacterStatus? caster, List<IPlayingBuff> playingBuffToAdd, Regeneration? regeneration)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            Caster = caster;
            PlayingBuffToAdd = playingBuffToAdd;
            RegenerationBase = regeneration;
        }


        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public CharacterStatus? Caster { get; set; }
        public List<IPlayingBuff> PlayingBuffToAdd { get; }
        public Regeneration? RegenerationBase { get; }

        public bool CanGoNextTick()
        {
            return false;
        }
    }
}