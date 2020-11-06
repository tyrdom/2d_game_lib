using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class ShopUnit 
    {
    }

    public interface ICanBeInShop
    {
    }

    public interface ICanPutInMapInteractable
    {
        IMapInteractable? InWhichMapInteractive { get; set; }

        public IMapInteractable GenIMapInteractable(TwoDPoint pos);
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