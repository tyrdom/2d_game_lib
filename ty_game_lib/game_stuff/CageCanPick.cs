using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CageCanPick : IMapInteractable
    {
        private CageCanPick(Interaction charAct, Round canInterActiveRound, Zone zone)
        {
            CharAct = charAct;
            CharAct.InCage.InWhichMapInteractive = this;
            CanInterActiveRound = canInterActiveRound;
            Zone = zone;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }

        public CageCanPick(ICanPutInCage canPutInCage, TwoDPoint pos)
        {
            Round roundP;
            Interaction interaction;
            var configsInteraction = TempConfig.Configs.interactions;
            switch (canPutInCage)
            {
                case Prop prop:
                    roundP = new Round(pos, TempConfig.PropR);
                    var interaction1 = configsInteraction[interactionAct.pick_prop];
                    interaction = new Interaction(interaction1.BaseTough,
                        interaction1.TotalTick,
                        prop);
                    break;
                case Vehicle vehicle:
                    roundP = new Round(pos, TempConfig.GetRBySize(vehicle.VehicleSize));
                    var interaction2 = configsInteraction[interactionAct.get_in_vehicle];
                    interaction = new Interaction(interaction2.BaseTough, interaction2.TotalTick,
                        vehicle);
                    break;
                case Weapon weapon:
                    roundP = new Round(pos, TempConfig.WeaponR);
                    var interaction3 = configsInteraction[interactionAct.pick_weapon];
                    interaction = new Interaction(interaction3.BaseTough, interaction3.TotalTick, weapon);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(canPutInCage));
            }

            CharAct = interaction;
            var zonesP = roundP.GetZones();
            Zone = zonesP;
            CanInterActiveRound = roundP;
            CharAct.InCage.InWhichMapInteractive = this;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }

        public void ReLocate(TwoDPoint pos)
        {
            LocateRecord.Clear();
            CanInterActiveRound.O = pos;
            NowInterCharacterBody = null;
            Zone = CanInterActiveRound.GetZones();
        }

        public void StartPickBySomeBody(CharacterBody characterBody)
        {
            CharAct.Interactive = MapInteractive.PickOrInVehicle;
            if (characterBody.CharacterStatus.LoadInteraction(CharAct)) NowInterCharacterBody = characterBody;
        }

        public void StartRecycleBySomeBody(CharacterBody characterBody)
        {
            CharAct.Interactive = MapInteractive.RecycleCall;
            if (characterBody.CharacterStatus.LoadInteraction(CharAct)) NowInterCharacterBody = characterBody;
        }

        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharAct { get; }

        public Queue<Quad> LocateRecord { get; set; }

        public Interaction? GetAct(CharacterStatus characterStatus)
        {
            return CharAct.InCage.CanPick(characterStatus) ? CharAct : null;
        }

        public Round CanInterActiveRound { get; }

        public void WriteQuadRecord(Quad quad)
        {
            LocateRecord.Enqueue(quad);
        }

        public Quad? GetNextQuad()
        {
            var nextQuad = LocateRecord.Count > 0 ? LocateRecord.Dequeue() : (Quad?) null;
            return nextQuad;
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
            var (f, vertical1) = Zone.GetMid();
            var splitByQuads = Zone.SplitByQuads(f, vertical1);
            return splitByQuads
                .Select(x => (x.Item1, new CageCanPick(CharAct, CanInterActiveRound, x.Item2) as IAaBbBox))
                .ToList();
        }
    }
}