using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class MapInitData
    {
        public SightMap SightMap { get; }
        public WalkMap WalkMap { get; }
        public Dictionary<int, StartPts> TeamToStartPt { get; }
        public HashSet<ApplyDevice> StandardMapInteractableList { get; }
        public SightMap BulletBlockMap { get; }

        public Zone GetZone()
        {
            return WalkMap.SizeToEdge.Values
                .Select(walkBlock => walkBlock.QSpace?.Zone ?? Zone.Zero())
                .Aggregate(
                    SightMap.Lines.Zone, (current, qSpaceZone) => current.Join(qSpaceZone));
        }

        public MapInitData(SightMap sightMap, WalkMap walkMap, Dictionary<int, StartPts> teamToStartPt,
            HashSet<ApplyDevice> standardMapInteractableList, SightMap bulletBlockMap)
        {
            SightMap = sightMap;
            WalkMap = walkMap;
            TeamToStartPt = teamToStartPt;
            StandardMapInteractableList = standardMapInteractableList;
            BulletBlockMap = bulletBlockMap;
        }
    }
}