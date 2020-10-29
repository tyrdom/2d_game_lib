using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CharInitMsg
    {
        public int GId;
        public TwoDPoint Pos;
        public TwoDVector Aim;

        private SurvivalStatus _survivalStatus;
        public Dictionary<int, Weapon> WeaponConfigs { get; set; }

        public CharInitMsg(int gId, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            Dictionary<int, Weapon> weaponConfigs)
        {
            GId = gId;
            Pos = pos;
            Aim = aim;
            _survivalStatus = survivalStatus;
            WeaponConfigs = weaponConfigs;
        }
    }
}