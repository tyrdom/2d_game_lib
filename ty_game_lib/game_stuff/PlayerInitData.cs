using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class PlayerInitData
    {
        public int Gid;
        public int TeamId;
        private Dictionary<int, WeaponConfig> WeaponConfigs;
        private BodySize BodySize;
        private float Speed;

        public PlayerInitData(int gid, int teamId, Dictionary<int, WeaponConfig> weaponConfigs, BodySize bodySize,
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
                1, WeaponConfigs, null,
                Combo.ZeroCombo, 0, null,
                new List<DamageBuff>(), DamageHealStatus.StartDamageHealAbout(), 0);
            var characterBody = new CharacterBody(startPos, BodySize, characterStatus, startPos,
                AngleSight.StandardAngleSight(),
                TeamId);
            return characterBody;
        }
    }
}