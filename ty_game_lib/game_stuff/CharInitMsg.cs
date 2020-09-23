using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CharInitMsg
    {
        public int GId;
        public TwoDPoint Pos;
        public TwoDVector Aim;

        private DamageHealStatus _damageHealStatus;
        public Dictionary<int, Weapon> WeaponConfigs { get; set; }

        public CharInitMsg(int gId, TwoDPoint pos, TwoDVector aim, DamageHealStatus damageHealStatus,
            Dictionary<int, Weapon> weaponConfigs)
        {
            GId = gId;
            Pos = pos;
            Aim = aim;
            _damageHealStatus = damageHealStatus;
            WeaponConfigs = weaponConfigs;
        }
    }
}