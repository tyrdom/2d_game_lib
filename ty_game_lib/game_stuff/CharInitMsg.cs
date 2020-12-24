using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CharInitMsg
    {
        public int GId;
        public TwoDPoint Pos;
        private TwoDVector Aim;

        private SurvivalStatus SurvivalStatus;
        public Dictionary<int, Weapon> WeaponConfigs { get; private set; }

        public CharInitMsg(int gId, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            Dictionary<int, Weapon> weaponConfigs)
        {
            GId = gId;
            Pos = pos;
            Aim = aim;
            SurvivalStatus = survivalStatus;
            WeaponConfigs = weaponConfigs;
        }
    }
}