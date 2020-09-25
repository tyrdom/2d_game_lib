using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class PickCage : IMapInteractive
    {
        public PickCage(CageActiveAct charAct, Round canInterActiveRound, Zone zone)
        {
            CharAct = charAct;
            CanInterActiveRound = canInterActiveRound;
            Zone = zone;
            NowInterUser = null;
        }


        public CharacterBody? NowInterUser { get; set; }
        public CageActiveAct CharAct { get; }

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
                .Select(x => (x.Item1, new PickCage(this.CharAct, CanInterActiveRound, x.Item2) as IAaBbBox)).ToList();
        }
    }
}