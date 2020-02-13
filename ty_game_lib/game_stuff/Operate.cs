using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Operate
    {
        private Action Action;
        private TwoDVector Move;
    }

    internal enum Action
    {
        A1,
        A2,
        A3
    }

    public class TickOprations
    {
        public Dictionary<int, Operate> GidToOperates;
    }
}