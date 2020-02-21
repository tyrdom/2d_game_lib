using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class PlayGround
    {
        private Dictionary<int, QSpace> TeamToBodies;
        private readonly SightMap SightMap;
        private readonly WalkMap WalkMap;
        private Dictionary<int, int> GidToTeam;

        public PlayGround(Dictionary<int, QSpace> teamToBodies, SightMap sightMap, WalkMap walkMap,
            Dictionary<int, int> gidToTeam)
        {
            TeamToBodies = teamToBodies;
            SightMap = sightMap;
            WalkMap = walkMap;
            GidToTeam = gidToTeam;
        }


        public Dictionary<int, Dictionary<int, Operate>> SepOperatesToTeam(Dictionary<int, Operate> gidToOperates)
        {
            var dictionary = new Dictionary<int, Dictionary<int, Operate>>();

            foreach (var (gid, operate) in gidToOperates)
            {
                if (GidToTeam.TryGetValue(gid, out var team))
                {
                    dictionary[team][gid] = operate;
                }
            }

            return dictionary;
        }

        public void DoOperates(Dictionary<int, Operate> gidToOperates)
        {
            var sepOperatesToTeam = SepOperatesToTeam(gidToOperates);
            foreach (var (team, qSpace
                ) in TeamToBodies)
            {
                if (sepOperatesToTeam.TryGetValue(team, out var gidToOp))
                {
                    var teamToBody = TeamToBodies[team];
                    teamToBody.ForeachDoWithOutMove(GameTools.DoOp, gidToOp);
                }
            }
        }
    }
}