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

        public static CharacterInitData GenByIds(int gid, int teamId, int[] weapons, size size,
            int baseAttrId)
        {
            var dictionary = new Dictionary<int, Weapon>();
            for (var i = 0; i < MathTools.Min(LocalConfig.StandardWeaponNum, weapons.Length); i++)
            {
                dictionary[i] = Weapon.GenById(weapons[i]);
            }

            var bz = size;

            return new CharacterInitData(gid, teamId, dictionary, bz, baseAttrId);
        }

        public static CharacterInitData GenByConfig(int gid, int teamId, weapon[] weapons, size size,
            int baseAttrId)
        {
            var dictionary = new Dictionary<int, Weapon>();
            for (var i = 0; i < MathTools.Min(LocalConfig.StandardWeaponNum, weapons.Length); i++)
            {
                dictionary[i] = Weapon.GenByConfig(weapons[i]);
            }

            var bz = size;

            return new CharacterInitData(gid, teamId, dictionary, bz, baseAttrId);
        }

        private CharacterInitData(int gid, int teamId, Dictionary<int, Weapon> weapons, size bodySize,
            int baseAttrId)
        {
            Gid = gid;
            TeamId = teamId;
            Weapons = weapons;
            BodySize = bodySize;

            BaseAttrId = baseAttrId;
        }


        public CharacterBody GenCharacterBody(TwoDPoint startPos, 
            Dictionary<int, PassiveTrait>? passiveTraits = null,PlayingItemBag? playingItemBag = null)
        {
            var characterStatus = new CharacterStatus(Gid, BaseAttrId, playingItemBag ??
                                                                       PlayingItemBag.InitByConfig(), passiveTraits);

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