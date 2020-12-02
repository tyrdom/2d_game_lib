using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class SaleBox : IMapInteractable
    {
        public SaleBox(Zone zone, Queue<Quad> locateRecord, Round canInterActiveRound,
            CharacterBody? nowInterCharacterBody, Interaction charActOne, Interaction charActTwo)
        {
            Zone = zone;
            LocateRecord = locateRecord;
            CanInterActiveRound = canInterActiveRound;
            NowInterCharacterBody = nowInterCharacterBody;
            CharActOne = charActOne;
            CharActTwo = charActTwo;
        }

        public SaleBox(ISaleUnit saleUnit, TwoDPoint pos)
        {
            var configsInteraction = TempConfig.Configs.interactions;
            var roundP = new Round(pos, TempConfig.SaleBoxR);
            var interaction1 = configsInteraction[interactionAct.apply];
            var interaction11 = CageCanPick.GenInteractionByConfig(saleUnit, interaction1, MapInteract.InVehicleCall);
            var interaction2 = configsInteraction[interactionAct.buy];
            var interaction22 = CageCanPick.GenInteractionByConfig(saleUnit, interaction2, MapInteract.KickVehicleCall);
            CharActOne = interaction11;
            CharActTwo = interaction22;
            CanInterActiveRound = roundP;
            var zones = roundP.GetZones();
            Zone = zones;
            LocateRecord = new Queue<Quad>();
            CharActOne.InMapInteractable.InWhichMapInteractive = this;
            CharActTwo.InMapInteractable.InWhichMapInteractive = this;
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

        public void StartActOneBySomeBody(CharacterBody characterBody)
        {
            MapInteractableDefault.StartActOneBySomeBody(characterBody, this);
        }

        public void StartActTwoBySomeBody(CharacterBody characterBody)
        {
            MapInteractableDefault.StartActTwoBySomeBody(characterBody, this);
        }

        public TwoDPoint GetAnchor()
        {
            return CanInterActiveRound.O;
        }

        public ISeeTickMsg GenTickMsg()
        {
            throw new NotImplementedException();
        }
    }
}