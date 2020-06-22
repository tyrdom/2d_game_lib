using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class PlayerInitData
    {
        public int Gid;
        public int TeamId;
        private Dictionary<int, Weapon> WeaponConfigs;
        private BodySize BodySize;
        private float Speed;

        public PlayerInitData(int gid, int teamId, Dictionary<int, Weapon> weaponConfigs, BodySize bodySize,
            float speed)
        {
            Gid = gid;
            TeamId = teamId;
            WeaponConfigs = weaponConfigs;
            BodySize = bodySize;
            Speed = speed;
        }

        public CharacterBody GenCharacterBody(TwoDPoint startPos)
        {
            var characterStatus = new CharacterStatus(Speed, Gid, 0, null, null,
                1, WeaponConfigs, 0, DamageHealStatus.StartDamageHealAbout(), 0, 0.5f, 0.5f);
            var characterBody = new CharacterBody(startPos, BodySize, characterStatus, startPos,
                AngleSight.StandardAngleSight(),
                TeamId);
            return characterBody;
        }
    }
}