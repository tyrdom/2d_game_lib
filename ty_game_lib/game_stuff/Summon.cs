using System;
using collision_and_rigid;

namespace game_stuff
{
    public class Summon : IPosMedia
    {
        public Summon(TwoDPoint pos, TwoDVector aim, TrapConfig trapConfig)
        {
            Pos = pos;
            Aim = aim;
            Caster = null;
            TrapConfig = trapConfig;
        }

        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public IPosMedia Active(TwoDPoint casterPos, TwoDVector casterAim)
        {
            return
                PosMediaStandard.Active(casterPos, casterAim, this);
        }

        public bool CanGoNextTick()
        {
            return false;
        }

        public IBattleUnitStatus? Caster { get; set; }

        private TrapConfig TrapConfig { get; }


        public void Sign(CharacterStatus characterStatus)
        {
            Caster = characterStatus;
            TrapConfig.LaunchMedia?.Sign(characterStatus);
            TrapConfig.TrapMedia.Sign(characterStatus);
        }
    }
}