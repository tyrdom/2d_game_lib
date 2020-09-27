using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class CageCanPick : IMapInteractable
    {
        public CageCanPick(Interaction charAct, Round canInterActiveRound, Zone zone)
        {
            CharAct = charAct;
            CharAct.InCage.InWhichMapInteractive = this;
            CanInterActiveRound = canInterActiveRound;
            Zone = zone;
            NowInterUser = null;
        }

        public CageCanPick(ICanPutInCage canPutInCage, TwoDPoint pos)
        {
            //todo
            
            // switch (canPutInCage)
            // {
            //     case Prop prop:
            //         var roundP = new Round(pos, TempConfig.PropR);
            //         var zonesP = roundP.GetZones();
            //         var interaction = new Interaction(TempConfig.Configs.interactions.TryGetValue(), prop.TotalTick, prop);
            //         break;
            //     case Vehicle vehicle:
            //         var roundV = new Round(pos, TempConfig.GetRBySize(vehicle.VehicleSize));
            //         var zonesV = roundV.GetZones();
            //         var interactionV = new Interaction(v.NowTough, prop.TotalTick, prop);
            //         break;
            //     case Weapon weapon:
            //         var roundW = new Round(pos, TempConfig.WeaponR);
            //         var zonesW = roundW.GetZones();
            //         var interactionW = new Interaction(prop.NowTough, prop.TotalTick, prop);
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(canPutInCage));
            // }
        }

        public CharacterBody? NowInterUser { get; set; }
        public Interaction CharAct { get; }
        public Round CanInterActiveRound { get; }

        public IShape GetShape()
        {
            return CanInterActiveRound;
        }

        public Zone Zone { get; set; }

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            var (f, vertical1) = Zone.GetMid();
            var splitByQuads = Zone.SplitByQuads(f, vertical1);
            return splitByQuads
                .Select(x => (x.Item1, new CageCanPick(this.CharAct, CanInterActiveRound, x.Item2) as IAaBbBox))
                .ToList();
        }
    }
}