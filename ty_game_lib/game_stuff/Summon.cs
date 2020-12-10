using System;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Summon : IPosMedia
    {
        public Summon(TwoDPoint pos, TwoDVector aim, TrapSetter trapSetter)
        {
            Pos = pos;
            Aim = aim;
            Caster = null;
            TrapSetter = trapSetter;
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

        private TrapSetter TrapSetter { get; }


        public IdPointBox? SetATrap()
        {
            switch (Caster)
            {
                case null:
                    return null;
                case CharacterStatus characterStatus:
                    var genATrap = TrapSetter.GenATrap(characterStatus, Pos);
                    characterStatus.AddTrap(genATrap);
                    return genATrap.CovToIdBox();
                case Trap _:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Caster));
            }
        }

        public void Sign(CharacterStatus characterStatus)
        {
            Caster = characterStatus;
            TrapSetter.LaunchMedia?.Sign(characterStatus);
            TrapSetter.TrapMedia.Sign(characterStatus);
        }

        public static Summon GenById(string id)
        {
            throw new NotImplementedException();
        }

        public Summon(summon summon)
        {
            Pos = TwoDPoint.Zero();
            Aim = TwoDVector.Zero();
            Caster = null;
            //todo
        }
    }
}