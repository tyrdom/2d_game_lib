using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public readonly struct DropThings : IActResult
    {
        public IEnumerable<IMapInteractable> DropSet { get; }

        public DropThings(IEnumerable<IMapInteractable> dropThings)
        {
            DropSet = dropThings;
        }
    }


    public interface IActResult
    {
    }

    public interface IPutInCage : ICanPutInMapInteractable
    {
    }

    public interface ICanPickDrop
    {
        public IMapInteractable DropAsIMapInteractable(TwoDPoint pos);
    }

    public interface ICanPutInMapInteractable
    {
        IMapInteractable PutInteractable(TwoDPoint pos, bool isActive);
        IMapInteractable? InWhichMapInteractive { get; set; }
        bool CanInterActOneBy(CharacterStatus characterStatus);
        bool CanInterActTwoBy(CharacterStatus characterStatus);
        IActResult? ActWhichChar(CharacterStatus characterStatus, MapInteract interactive);
    }

    public static class CanPutInMapInteractableStandard
    {
        public static IMapInteractable PutInMapInteractable(TwoDPoint pos,
            IPutInCage putInCage)
        {
            switch (putInCage.InWhichMapInteractive)
            {
                case null:
                    return new CageCanPick(putInCage, pos);
                case CageCanPick cageCanPick:
                    cageCanPick.ReLocate(pos);
                    return cageCanPick;
                default:
                    throw new ArgumentOutOfRangeException(nameof(putInCage.InWhichMapInteractive));
            }
        }
    }
}