using System;
using collision_and_rigid;

namespace game_stuff
{
    public class Summons : IEffectMedia
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public bool CanGoNextTick()
        {
            return false;
        }

        public CharacterStatus? Caster { get; set; }
    }
}