using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CharacterInitData
    {
        public int Gid;
        public int TeamId;
        private Dictionary<int, Weapon> Weapons;
        private BodySize BodySize;
        private float MaxSpeed;
        private float MinSpeed;
        private float AddSpeed;

        public static CharacterInitData GenByConfig(int gid, int teamId, weapon[] weapons, size size, float maxSpeed,
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


            return new CharacterInitData(gid, teamId, dictionary, bz, maxSpeed, minSpeed, addSpeed);
        }

        private CharacterInitData(int gid, int teamId, Dictionary<int, Weapon> weapons, BodySize bodySize,
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
            var characterStatus = new CharacterStatus(MaxSpeed, Gid,
                DamageHealStatus.StartDamageHealAbout(), AddSpeed, MinSpeed);

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

        public void ReloadCharacterBody(CharacterBody characterBody)
        {
            characterBody.CharacterStatus.ReloadInitData(DamageHealStatus.StartDamageHealAbout(), MaxSpeed, AddSpeed,
                MinSpeed);
            foreach (var weapon in Weapons.Select(keyValuePair => keyValuePair.Value))
            {
#if DEBUG
                Console.Out.WriteLine($"{weapon.LogUserString()}");
#endif
                weapon.PickedBySomebody(characterBody.CharacterStatus);
#if DEBUG
                Console.Out.WriteLine($"{weapon.LogUserString()}");
#endif
            }
        }
    }
}