using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CharacterInitData
    {
        public int Gid { get; }

        public int TeamId { get; }
        private Dictionary<int, Weapon> Weapons { get; }
        private BodySize BodySize { get; }

        private BodyMark BodyMark { get; }

        private base_attr_id BaseAttrId { get; }

        public static CharacterInitData GenByConfig(int gid, int teamId, weapon[] weapons, size size, BodyMark bodyMark,
            base_attr_id baseAttrId)
        {
            var dictionary = new Dictionary<int, Weapon>();
            for (var i = 0; i < MathTools.Min(TempConfig.StandardWeaponNum, weapons.Length); i++)
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


            return new CharacterInitData(gid, teamId, dictionary, bz, bodyMark, baseAttrId);
        }

        private CharacterInitData(int gid, int teamId, Dictionary<int, Weapon> weapons, BodySize bodySize,
            BodyMark bodyMark, base_attr_id baseAttrId)
        {
            Gid = gid;
            TeamId = teamId;
            Weapons = weapons;
            BodySize = bodySize;
            BodyMark = bodyMark;
            BaseAttrId = baseAttrId;
        }


        public CharacterBody GenCharacterBody(TwoDPoint startPos)
        {
            var characterStatus = new CharacterStatus(Gid, TempConfig.TrickProtect, BaseAttrId,
                PlayingItemBag.InitByConfig());

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
                TeamId, BodyMark);
            return characterBody;
        }

        public void ReloadCharacterBody(CharacterBody characterBody)
        {
            characterBody.CharacterStatus.ReloadInitData();
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