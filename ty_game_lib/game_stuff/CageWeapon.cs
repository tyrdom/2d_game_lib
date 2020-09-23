using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class CageWeapon : ICageCanBePickUp, IAaBbBox
    {
        public CageWeapon(Round pickRound, Zone zone, Weapon? weapon)
        {
            PickRound = pickRound;
            Zone = zone;
            Weapon = weapon;
        }

        private Weapon? Weapon { get; set; }
        public Round PickRound { get; }

        public IShape GetShape()
        {
            return PickRound;
        }

        public Zone Zone { get; set; }

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            var valueTuples = Zone.SplitByQuads(horizon, vertical)
                .Select(x => (x.Item1, new CageWeapon(PickRound, x.Item2, Weapon) as IAaBbBox)).ToList();
            return valueTuples;
        }
    }
}