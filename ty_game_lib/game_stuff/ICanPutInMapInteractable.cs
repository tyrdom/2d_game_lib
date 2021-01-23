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

    public readonly struct TelePortMsg : IActResult
    {
        public TelePortMsg(int mgId, TwoDPoint toPos)
        {
            GMid = mgId;
            ToPos = toPos;
        }

        public int GMid { get; }
        public TwoDPoint ToPos { get; }

        public void Deconstruct(out int gMid, out TwoDPoint toPos)
        {
            gMid = GMid;
            toPos = ToPos;
        }
    }


    public interface IActResult
    {
    }

    public interface ICanDrop
    {
        public IMapInteractable DropAsIMapInteractable(TwoDPoint pos);
    }

    public interface ICanPutInMapInteractable
    {
        IMapInteractable? InWhichMapInteractive { get; set; }


        bool CanInterActOneBy(CharacterStatus characterStatus);

        bool CanInterActTwoBy(CharacterStatus characterStatus);
        IActResult? ActWhichChar(CharacterStatus characterStatus, MapInteract interactive);
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