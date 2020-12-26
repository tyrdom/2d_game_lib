using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class VehicleCanInTickMsg : ISeeTickMsg
    {
        public VehicleCanInTickMsg(int vId, bool isBroken, float sStatus)
        {
            VId = vId;
            IsBroken = isBroken;
            SStatus = sStatus;
        }

        public int VId { get; }

        public bool IsBroken { get; }

        public float SStatus { get; }

        public override string ToString()
        {
            return $"vId:{VId} IsBroken :{IsBroken} Status:{SStatus}";
        }
    }

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
            var configsInteraction = LocalConfig.Configs.interactions;

            var roundP = new Round(pos, LocalConfig.GetRBySize(vehicle.Size));
            var interaction1 = configsInteraction[interactionAct.get_in_vehicle];
            var interaction11
                =
                GenInteractionByConfig(vehicle, interaction1, MapInteract.InVehicleCall, pos);
            var interaction2 = configsInteraction[interactionAct.kick_vehicle];
            var interaction22 = GenInteractionByConfig(vehicle, interaction2, MapInteract.KickVehicleCall, pos);
            CharActOne = interaction11;
            CharActTwo = interaction22;

            var zonesP = roundP.GetZones();
            Zone = zonesP;
            CanInterActiveRound = roundP;
            CharActOne.InMapInteractable.InWhichMapInteractive = this;
            CharActTwo.InMapInteractable.InWhichMapInteractive = this;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }

        private static Interaction GenInteractionByConfig(ICanPutInMapInteractable vehicle, interaction interaction1,
            MapInteract interact, TwoDPoint twoDPoint)
        {
            return new Interaction(interaction1.BaseTough,
                CommonConfig.GetTickByTime(interaction1.TotalTime),
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

        public IEnumerable<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            return MapInteractableDefault.SplitByQuads(horizon, vertical, this);
        }

        public void RecordQuad(int qI)
        {
            MapInteractableDefault.RecardQuad(qI, this);
        }

        public Queue<Quad> LocateRecord { get; }

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

        public ISeeTickMsg GenTickMsg(int? gid = null)
        {
            if (!(CharActOne.InMapInteractable is Vehicle vehicle))
                throw new TypeAccessException("vehicleCanIn Must contain vehicle");
            var vehicleIsDsOn = vehicle.IsDsOn;
            var vehicleVId = vehicle.VId;
            var genShortStatus = vehicle.SurvivalStatus.GenShortStatus();
            return new VehicleCanInTickMsg(vehicleVId, vehicleIsDsOn, genShortStatus);
        }

        public TwoDPoint GetAnchor()
        {
            return CanInterActiveRound.O;
        }

        public Bullet? GoATick()
        {
            if (!(CharActOne.InMapInteractable is Vehicle vehicle)) return null;
            var (_, destroyBullet) = vehicle.GoATickCheckSurvival();
            return destroyBullet;
        }
    }
}