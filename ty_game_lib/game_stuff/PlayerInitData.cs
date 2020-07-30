using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class PlayerInitData
    {
        public int Gid;
        public int TeamId;
        private Dictionary<int, Weapon> Weapons;
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

        public PlayerInitData(int gid, int teamId, Dictionary<int, Weapon> weapons, BodySize bodySize,
            float maxSpeed, float minSpeed, float addSpeed)
        {
            Gid = gid;
            TeamId = teamId;
            Weapons = weapons;
            BodySize = bodySize;
            MaxSpeed = maxSpeed;
            MinSpeed = minSpeed;
            AddSpeed = addSpeed;
        }

        public CharacterBody GenCharacterBody(TwoDPoint startPos)
        {
            var characterStatus = new CharacterStatus(MaxSpeed, Gid, 0, new Dictionary<int, Weapon>(),
                DamageHealStatus.StartDamageHealAbout(), 0, AddSpeed, MinSpeed);

            foreach (var weapon in Weapons.Select(keyValuePair => keyValuePair.Value))
            {
#if DEBUG
                Console.Out.WriteLine($"{weapon.LogUserString()}");
#endif
                weapon.PickedBySomebody(characterStatus);
#if DEBUG
                Console.Out.WriteLine($"{weapon.LogUserString()}");
#endif
            }

            var characterBody = new CharacterBody(startPos, BodySize, characterStatus, startPos,
                AngleSight.StandardAngleSight(),
                TeamId);
            return characterBody;
        }
    }
}