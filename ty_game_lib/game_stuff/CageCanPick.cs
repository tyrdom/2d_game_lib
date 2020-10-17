using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class MapInteractableDefault
    {
        public static List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical,
            IMapInteractable mapInteractable)
        {
            var splitByQuads = mapInteractable.Zone.SplitByQuads(horizon, vertical);
            return splitByQuads
                .Select(x => (x.Item1,
                    mapInteractable.FactAaBbBox(x.Item2)))
                .ToList();
        }

        public static Quad? GetNextQuad(IMapInteractable mapInteractable)
        {
            var nextQuad = mapInteractable.LocateRecord.Count > 0
                ? mapInteractable.LocateRecord.Dequeue()
                : (Quad?) null;
            return nextQuad;
        }

        public static void ReLocate(TwoDPoint pos, IMapInteractable mapInteractable)
        {
            mapInteractable.LocateRecord.Clear();
            mapInteractable.CanInterActiveRound.O = pos;
            mapInteractable.NowInterCharacterBody = null;
            mapInteractable.Zone = mapInteractable.CanInterActiveRound.GetZones();
        }

        public static void StartActOneBySomeBody(CharacterBody characterBody, IMapInteractable mapInteractable)
        {
            var actOne = mapInteractable.GetActOne(characterBody.CharacterStatus);
            if (actOne == null)
            {
                return;
            }

            if (characterBody.CharacterStatus.LoadInteraction(mapInteractable.CharActOne))
                mapInteractable.NowInterCharacterBody = characterBody;
        }

        public static void StartActTwoBySomeBody(CharacterBody characterBody, IMapInteractable mapInteractable)
        {
            var actOne = mapInteractable.GetActTwo(characterBody.CharacterStatus);
            if (actOne == null)
            {
                return;
            }

            if (characterBody.CharacterStatus.LoadInteraction(mapInteractable.CharActTwo))
                mapInteractable.NowInterCharacterBody = characterBody;
        }

        public static void WriteQuadRecord(Quad quad, IMapInteractable mapInteractable)
        {
            mapInteractable.LocateRecord.Enqueue(quad);
        }
    }

    public class CageCanPick : IMapInteractable
    {
        private CageCanPick(Interaction charActOne, Interaction charActTwo, Round canInterActiveRound, Zone zone)
        {
            CharActOne = charActOne;
            CharActOne.InCage.InWhichMapInteractive = this;
            CharActTwo = charActTwo;
            CharActTwo.InCage.InWhichMapInteractive = this;
            CanInterActiveRound = canInterActiveRound;
            Zone = zone;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }


        private static Interaction GenInteractionByConfig(ICanPutInCage canPutInCage, interaction interaction,
            MapInteract mapInteract)
        {
            return new Interaction(interaction.BaseTough,
                TempConfig.GetTickByTime(interaction.TotalTime),
                canPutInCage, null, mapInteract);
        }

        public CageCanPick(ICanPutInCage canPutInCage, TwoDPoint pos)
        {
            var configsInteraction = TempConfig.Configs.interactions;

            var roundP = canPutInCage switch
            {
                Prop _ => new Round(pos, TempConfig.PropR),
                Weapon _ => new Round(pos, TempConfig.WeaponR),
                _ => throw new ArgumentOutOfRangeException(nameof(canPutInCage))
            };
            var interaction1 = configsInteraction[interactionAct.pick_weapon_or_prop];
            Interaction interaction =
                GenInteractionByConfig(canPutInCage, interaction1, MapInteract.PickPropOrWeaponCall);
            var interaction2 = configsInteraction[interactionAct.recycle_prop_or_weapon];
            Interaction interaction22 = GenInteractionByConfig(canPutInCage, interaction2, MapInteract.RecycleCall);

            var zonesP = roundP.GetZones();
            Zone = zonesP;
            CanInterActiveRound = roundP;
            CharActTwo = interaction22;
            CharActOne = interaction;
            CharActOne.InCage.InWhichMapInteractive = this;
            CharActTwo.InCage.InWhichMapInteractive = this;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }


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

        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharActOne { get; }
        public Interaction CharActTwo { get; }

        public IAaBbBox FactAaBbBox(
            Zone zone)
        {
            return new CageCanPick(CharActOne, CharActTwo, CanInterActiveRound, zone);
        }

        public bool IsCage()
        {
            return true;
        }

        public bool IsVehicle()
        {
            return false;
        }

        public Queue<Quad> LocateRecord { get; set; }


        public Interaction? GetActOne(CharacterStatus characterStatus)
        {
            return CharActOne.InCage.CanInterActOneBy(characterStatus) ? CharActOne : null;
        }

        public Interaction? GetActTwo(CharacterStatus characterStatus)
        {
            return CharActTwo.InCage.CanInterActTwoBy(characterStatus) ? CharActTwo : null;
        }

        public Round CanInterActiveRound { get; }

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
    }
}