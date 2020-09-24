using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class PickCage : IMapInteractive, IAaBbBox
    {
        public PickCage(InterActiveAct charAct, Round canInterActiveRound, Zone zone)
        {
            CharAct = charAct;
            CanInterActiveRound = canInterActiveRound;
            Zone = zone;
            InterUser = null;
        }

        
        public CharacterBody? InterUser { get; set; }
        public InterActiveAct CharAct { get; }

        public Round CanInterActiveRound { get; }

        public IShape GetShape()
        {
            return CanInterActiveRound;
        }

        public Zone Zone { get; set; }

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            throw new System.NotImplementedException();
        }
    }
}