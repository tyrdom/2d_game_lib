using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class PlayerInitData
    {
        public int Gid;
        public int TeamId;
        private Dictionary<int, Weapon> WeaponConfigs;
        private BodySize BodySize;
        private float MaxSpeed;
        private float MinSpeed;
        private float AddSpeed;

        public static PlayerInitData GenByConfig(int gid, int teamId, weapon[] weapons, size size, float maxSpeed,
            float minSpeed, float addSpeed)
        {
            var dictionary = new Dictionary<int, Weapon>();
            for (var i = 0; i < MathTools.Min(TempConfig.WeaponNum, weapons.Length); i++)
            {
                dictionary[i] = Weapon.GenByConfig(weapons[i]);
            }

            var bz = size switch
            {
                size.@default => BodySize.Small,
                size.medium => BodySize.Medium,
                size.small => BodySize.Small,
                size.big => BodySize.Big,
                _ => BodySize.Small
            };


            return new PlayerInitData(gid, teamId, dictionary, bz, maxSpeed, minSpeed, addSpeed);
        }

        public PlayerInitData(int gid, int teamId, Dictionary<int, Weapon> weaponConfigs, BodySize bodySize,
            float maxSpeed, float minSpeed, float addSpeed)
        {
            Gid = gid;
            TeamId = teamId;
            WeaponConfigs = weaponConfigs;
            BodySize = bodySize;
            MaxSpeed = maxSpeed;
            MinSpeed = minSpeed;
            AddSpeed = addSpeed;
        }

        public CharacterBody GenCharacterBody(TwoDPoint startPos)
        {
            var characterStatus = new CharacterStatus(MaxSpeed, Gid, 0, WeaponConfigs,
                DamageHealStatus.StartDamageHealAbout(), 0, AddSpeed, MinSpeed);
            var characterBody = new CharacterBody(startPos, BodySize, characterStatus, startPos,
                AngleSight.StandardAngleSight(),
                TeamId);
            return characterBody;
        }
    }
}