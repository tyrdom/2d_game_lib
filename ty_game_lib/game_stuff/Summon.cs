using System;
using collision_and_rigid;

namespace game_stuff
{
    public class Summon : IEffectMedia
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        public bool CanGoNextTick()
        {
            return false;
        }

        public IBattleUnitStatus? Caster { get; set; }

        public Trap Trap { get; }
    }
}