using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class CageVehicle : ICageCanBePickUp, IAaBbBox
    {
        public CageVehicle(Vehicle vehicle, Round pickRound, Zone zone)
        {
            Vehicle = vehicle;
            PickRound = pickRound;
            Zone = zone;
        }

        private Vehicle Vehicle { get; }
        public Round PickRound { get; }

        public IShape GetShape()
        {
            return PickRound;
        }

        public Zone Zone { get; set; }

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            return Zone.SplitByQuads(horizon, vertical)
                .Select(x => (x.Item1, new CageVehicle(Vehicle, PickRound, x.Item2) as IAaBbBox))
                .ToList();
        }
    }
}