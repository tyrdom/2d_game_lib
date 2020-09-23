using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CageProp : ICageCanBePickUp, IAaBbBox
    {
        public Prop? Prop { get; set; }
        public Round PickRound { get; }

        public IShape GetShape()
        {
            return PickRound;
        }

        public Zone Zone { get; set; }

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            throw new System.NotImplementedException();
        }
    }
}