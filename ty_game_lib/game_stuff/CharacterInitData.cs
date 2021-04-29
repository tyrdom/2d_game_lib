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
        private size BodySize { get; }
        private int BaseAttrId { get; }

        private int? WeaponMaxNum { get; }

        public static CharacterInitData GenNpcByConfig(int gid, int teamId, string[] weapons, size size,
            int baseAttrId, int battleNpcMaxWeaponSlot)
        {
            var dictionary = new Dictionary<int, Weapon>();
            for (var i = 0; i < weapons.Length; i++)
            {
                dictionary[i] = Weapon.GenById(weapons[i]);
            }

            var bz = size;

            return new CharacterInitData(gid, teamId, dictionary, bz, baseAttrId, battleNpcMaxWeaponSlot);
        }

        public static CharacterInitData GenPlayerByConfig(int gid, int teamId, weapon_id[] weapons, size size,
            int baseAttrId)
        {
            var dictionary = new Dictionary<int, Weapon>();
            for (var i = 0; i < MathTools.Min(CommonConfig.OtherConfig.weapon_num, weapons.Length); i++)
            {
                dictionary[i] = Weapon.GenById(weapons[i]);
            }

            var bz = size;

            return new CharacterInitData(gid, teamId, dictionary, bz, baseAttrId, null);
        }

        private CharacterInitData(int gid, int teamId, Dictionary<int, Weapon> weapons, size bodySize,
            int baseAttrId, int? weaponMaxNum)
        {
            Gid = gid;
            TeamId = teamId;
            Weapons = weapons;
            BodySize = bodySize;
            BaseAttrId = baseAttrId;
            WeaponMaxNum = weaponMaxNum;
        }


        public CharacterBody GenCharacterBody(TwoDPoint startPos,
            Dictionary<passive_id, PassiveTrait>? passiveTraits = null, PlayingItemBag? playingItemBag = null)
        {
            var characterStatus = new CharacterStatus(Gid, BaseAttrId, playingItemBag ??
                                                                       PlayingItemBag.InitByConfig(), passiveTraits,
                WeaponMaxNum);

            var characterBody = new CharacterBody(startPos, BodySize, characterStatus, startPos,
                AngleSight.StandardAngleSight(),
                TeamId);
            foreach (var weapon in Weapons.Select(keyValuePair => keyValuePair.Value))
            {
// #if DEBUG
//                 Console.Out.WriteLine($"{weapon.LogUserString()}");
// #endif
                weapon.PickedBySomebody(characterStatus);
#if DEBUG
                Console.Out.WriteLine($"got weapon broadcast :{weapon.LogUserString()}");
#endif
            }

            
            return characterBody;
        }
    }
}