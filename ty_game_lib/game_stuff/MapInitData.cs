using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class MapInitData
    {
        public SightMap SightMap;
        public WalkMap WalkMap;
        public Dictionary<int, StartPts> TeamToStartPt;
        public HashSet<ApplyDevice> StandardMapInteractableList;

        public Zone GetZone()
        {
            return WalkMap.SizeToEdge.Values
                .Select(walkBlock => walkBlock.QSpace?.Zone ?? Zone.Zero())
                .Aggregate(
                    SightMap.Lines.Zone, (current, qSpaceZone) => current.Join(qSpaceZone));
        }

        public MapInitData(SightMap sightMap, WalkMap walkMap, Dictionary<int, StartPts> teamToStartPt,
            HashSet<ApplyDevice > standardMapInteractableList)
        {
            SightMap = sightMap;
            WalkMap = walkMap;
            TeamToStartPt = teamToStartPt;
            StandardMapInteractableList = standardMapInteractableList;
        }
    }
}