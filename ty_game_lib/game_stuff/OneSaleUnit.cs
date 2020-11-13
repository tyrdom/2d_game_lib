using System;
using System.Collections.Generic;

namespace game_stuff
{
    public interface ISaleUnit : ICanPutInMapInteractable
    {
        GameItem Cost { get; }
    }

    public class OneSaleUnit : ISaleUnit
    {
        public OneSaleUnit(IMapInteractable? inWhichMapInteractive, GameItem cost, ICanPutInMapInteractable good,
            List<CharacterStatus> doneList)
        {
            if (good is ISaleUnit) throw new TypeAccessException($"cant put this {good.GetType()} in OneSale");
            InWhichMapInteractive = inWhichMapInteractive;
            Cost = cost;
            Good = good;
            DoneList = doneList;
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }

        public GameItem Cost { get; }

        public ICanPutInMapInteractable Good { get; }

        public List<CharacterStatus> DoneList { get; }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            var contains = DoneList.Contains(characterStatus);
            return !contains;
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return !DoneList.Contains(characterStatus) && characterStatus.PlayingItemBag.CanCost(Cost);
        }

        public IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.ApplyCall:
                    return new List<IMapInteractable>();
                case MapInteract.BuyCall:

                    var cost = characterStatus.PlayingItemBag.Cost(Cost);
                    if (cost)
                    {
                        switch (Good)
                        {
                            case PassiveTrait passiveTrait:
                                return passiveTrait.ActWhichChar(characterStatus, MapInteract.PickCall);
                            case Prop prop:
                                return prop.ActWhichChar(characterStatus, MapInteract.PickCall);
                            case Vehicle vehicle:
                                vehicle.WhoDriveOrCanDrive = characterStatus;
                                var twoDPoint = InWhichMapInteractive?.GetPos() ?? characterStatus.GetPos();

                                var dropAsIMapInteractable = vehicle.DropAsIMapInteractable(twoDPoint);
                                return new List<IMapInteractable> {dropAsIMapInteractable};

                            case Weapon weapon:
                                return weapon.ActWhichChar(characterStatus, MapInteract.PickCall);
                            default:
                                throw new ArgumentOutOfRangeException(nameof(Good));
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }

            return new List<IMapInteractable>();
        }
    }
}