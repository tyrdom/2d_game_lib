using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface ICanDrop
    {
        public IMapInteractable DropAsIMapInteractable(TwoDPoint pos);
    }

    public interface ICanPutInMapInteractable
    {
        IMapInteractable? InWhichMapInteractive { get; set; }


        bool CanInterActOneBy(CharacterStatus characterStatus);

        bool CanInterActTwoBy(CharacterStatus characterStatus);
        IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive);
    }

    public static class CanPutInMapInteractableStandard
    {
        public static IMapInteractable GenIMapInteractable(TwoDPoint pos,
            ICanPutInMapInteractable canPutInMapInteractable)
        {
            switch (canPutInMapInteractable.InWhichMapInteractive)
            {
                case null:
                    return new CageCanPick(canPutInMapInteractable, pos);
                case CageCanPick cageCanPick:
                    cageCanPick.ReLocate(pos);
                    return cageCanPick;
                default:
                    throw new ArgumentOutOfRangeException(nameof(canPutInMapInteractable.InWhichMapInteractive));
            }
        }
    }
}