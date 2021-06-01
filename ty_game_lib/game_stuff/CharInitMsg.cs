using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class CharInitMsg
    {
        public int GId { get; }
        public HashSet<ICharEvent> CharEvents { get; }


        public CharInitMsg(int gId, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            Dictionary<int, Weapon> weaponConfigs)
        {
            GId = gId;
            var posChange = new PosChange(pos);
            var aimChange = new AimChange(aim);
            var newSurvivalStatus = survivalStatus.GenNewMsg();

            var pickWeapons = weaponConfigs.Values.Select(x => new PickWeapon(x.WId));
            var charEvents = new HashSet<ICharEvent> {posChange, aimChange, newSurvivalStatus};
            charEvents.UnionWith(pickWeapons);
            CharEvents = charEvents;
        }
    }
}