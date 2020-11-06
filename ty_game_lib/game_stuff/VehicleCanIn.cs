using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class VehicleCanIn : IMapInteractable
    {
        private VehicleCanIn(Interaction charActOne, Interaction charActTwo, Round canInterActiveRound, Zone zone)
        {
            CharActOne = charActOne;
            CharActOne.InMapInteractable.InWhichMapInteractive = this;
            CharActTwo = charActTwo;
            CharActTwo.InMapInteractable.InWhichMapInteractive = this;
            CanInterActiveRound = canInterActiveRound;
            Zone = zone;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }

        public VehicleCanIn(Vehicle vehicle, TwoDPoint pos)
        {
            var configsInteraction = TempConfig.Configs.interactions;

            var roundP = new Round(pos, TempConfig.GetRBySize(vehicle.Size));
            var interaction1 = configsInteraction[interactionAct.get_in_vehicle];
            Interaction interaction =
                GenInteractionByConfig(vehicle, interaction1, MapInteract.InVehicleCall, pos);
            var interaction2 = configsInteraction[interactionAct.kick_vehicle];
            Interaction interaction22 = GenInteractionByConfig(vehicle, interaction2, MapInteract.KickVehicleCall, pos);
            CharActTwo = interaction22;
            CharActOne = interaction;
            var zonesP = roundP.GetZones();
            Zone = zonesP;
            CanInterActiveRound = roundP;
            CharActOne.InMapInteractable.InWhichMapInteractive = this;
            CharActTwo.InMapInteractable.InWhichMapInteractive = this;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }

        private static Interaction GenInteractionByConfig(Vehicle vehicle, interaction interaction1,
            MapInteract interact, TwoDPoint twoDPoint)
        {
            return new Interaction(interaction1.BaseTough,
                TempConfig.GetTickByTime(interaction1.TotalTime),
                vehicle, twoDPoint, interact);
        }

        public void WriteQuadRecord(Quad quad)
        {
            MapInteractableDefault.WriteQuadRecord(quad, this);
        }

        public Quad? GetNextQuad()
        {
            return MapInteractableDefault.GetNextQuad(this);
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
            return true;
        }

        public bool IsSale()
        {
            return false;
        }

        public Queue<Quad> LocateRecord { get; set; }

        public Interaction? GetActOne(CharacterStatus characterStatus)
        {
            return CharActOne.InMapInteractable.CanInterActOneBy(characterStatus) ? CharActOne : null;
        }

        public Interaction? GetActTwo(CharacterStatus characterStatus)
        {
            return CharActTwo.InMapInteractable.CanInterActTwoBy(characterStatus) ? CharActTwo : null;
        }

        public Round CanInterActiveRound { get; }
        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharActOne { get; }
        public Interaction CharActTwo { get; }

        public void ReLocate(TwoDPoint pos)
        {
            MapInteractableDefault.ReLocate(pos, this);
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