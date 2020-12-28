using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class ApplyDevice : IMapInteractable
    {
        public void SetActive()
        {
            IsActive = true;
        }

        public bool IsActive { get; set; }

        public ApplyDevice(Zone zone, Queue<Quad> locateRecord, Round canInterActiveRound,
            CharacterBody? nowInterCharacterBody, Interaction charActOne, Interaction charActTwo, bool isActive)
        {
            Zone = zone;
            LocateRecord = locateRecord;
            CanInterActiveRound = canInterActiveRound;
            NowInterCharacterBody = nowInterCharacterBody;
            CharActOne = charActOne;
            CharActTwo = charActTwo;
            IsActive = isActive;
        }

        public ApplyDevice(IApplyUnit saleUnit, TwoDPoint pos, bool isActive)
        {
            IsActive = isActive;
            var configsInteraction = LocalConfig.Configs.interactions;
            var roundP = new Round(pos, LocalConfig.SaleBoxR);
            var interaction1 = configsInteraction[interactionAct.get_info];
            var interaction11 = CageCanPick.GenInteractionByConfig(saleUnit, interaction1, MapInteract.GetInfoCall);
            var interaction2 = configsInteraction[interactionAct.apply];
            var interaction22 = CageCanPick.GenInteractionByConfig(saleUnit, interaction2, MapInteract.BuyOrApplyCall);
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


        public void SetInPlayGround(PlayGround playGround)
        {
            if (CharActTwo.InMapInteractable is TeleportUnit teleportUnit)
                teleportUnit.PutInPlayGround(playGround);
        }


        public bool CanInteractive(TwoDPoint pos)
        {
            return Zone.IncludePt(pos) && CanInterActiveRound.Include(pos) && IsActive;
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

        public ISeeTickMsg GenTickMsg(int? gid = null)
        {
            switch (CharActOne.InMapInteractable)
            {
                case SaleRandom saleRandom:
                    var saleRandomTitle = saleRandom.Title;
                    return new SaleRandomTickMsg(saleRandom.Cost, saleRandomTitle, saleRandom.GetRestStack(gid));
                case SaleUnit saleUnit:
                    return new SaleBoxTickMsg(saleUnit.Cost,
                        saleUnit.Good.Select(x => (SaleRandom.GetTitle(x), x.GetId())).ToArray(),
                        saleUnit.GetRestStack(gid));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}