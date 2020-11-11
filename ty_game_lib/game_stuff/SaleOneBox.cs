using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class OneSaleUnit : ICanPutInMapInteractable
    {
        public OneSaleUnit(IMapInteractable? inWhichMapInteractive, GameItem cost, ICanPutInMapInteractable good,
            List<CharacterStatus> doneList)
        {
            if (good is OneSaleUnit) throw new TypeAccessException($"cant put this {good.GetType()} in OneSale");
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
            return characterStatus.PlayingItemBag.CanCost(Cost);
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
                                if (characterStatus.NowVehicle == null)
                                {
                                    return vehicle.ActWhichChar(characterStatus, MapInteract.InVehicleCall);
                                }

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

    public class SaleOneBox : IMapInteractable
    {
        public SaleOneBox(Zone zone, Queue<Quad> locateRecord, Round canInterActiveRound,
            CharacterBody? nowInterCharacterBody, Interaction charActOne, Interaction charActTwo, GameItem cost)
        {
            Zone = zone;
            LocateRecord = locateRecord;
            CanInterActiveRound = canInterActiveRound;
            NowInterCharacterBody = nowInterCharacterBody;
            CharActOne = charActOne;
            CharActTwo = charActTwo;
        }

        public void WriteQuadRecord(Quad quad)
        {
            MapInteractableDefault.WriteQuadRecord(quad, this);
        }

        public Quad? GetNextQuad()
        {
            return MapInteractableDefault.GetNextQuad(this);
        }

        public IShape GetShape()
        {
            return CanInterActiveRound;
        }

        public TwoDPoint GetPos()
        {
            return CanInterActiveRound.O;
        }

        public bool CanInteractive(TwoDPoint pos)
        {
            return Zone.IncludePt(pos) && CanInterActiveRound.Include(pos) && NowInterCharacterBody == null;
        }

        public Zone Zone { get; set; }

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            return MapInteractableDefault.SplitByQuads(horizon, vertical, this);
        }

        public IAaBbBox FactAaBbBox(int qI)
        {
            return MapInteractableDefault.FactAaBbBox(qI, this);
        }

        public bool IsCage()
        {
            return false;
        }

        public bool IsVehicle()
        {
            return false;
        }

        public bool IsSale()
        {
            return true;
        }

        public Queue<Quad> LocateRecord { get; set; }

        public Interaction? GetActOne(CharacterStatus characterStatus)
        {
            throw new NotImplementedException();
        }

        public Interaction? GetActTwo(CharacterStatus characterStatus)
        {
            throw new NotImplementedException();
        }

        public Round CanInterActiveRound { get; }
        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharActOne { get; }
        public Interaction CharActTwo { get; }


        public void ReLocate(TwoDPoint pos)
        {
            MapInteractableDefault.ReLocate(pos, this);
        }

        public void StartActOneBySomeBody(CharacterBody characterBody)
        {
            MapInteractableDefault.StartActOneBySomeBody(characterBody, this);
        }

        public void StartActTwoBySomeBody(CharacterBody characterBody)
        {
            MapInteractableDefault.StartActTwoBySomeBody(characterBody, this);
        }
    }
}