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

        private Dictionary<passive_id, PassiveTrait>? InitPassiveTraits { get; }
        private int? WeaponMaxNum { get; }

        public static CharacterInitData GenNpcByConfig(int gid, int teamId, string[] weapons, size size,
            int baseAttrId, int battleNpcMaxWeaponSlot)
        {
            var dictionary = new Dictionary<int, Weapon>();
            for (var i = 0; i < weapons.Length; i++)
            {
                dictionary[i] = Weapon.GenById(weapons[i]);
            }


            return new CharacterInitData(gid, teamId, dictionary, size, baseAttrId, battleNpcMaxWeaponSlot,
                new Dictionary<passive_id, PassiveTrait>());
        }

        public static CharacterInitData GenPlayerByConfig(int gid, int teamId, weapon_id[] weapons, size size,
            int baseAttrId, Dictionary<passive_id, uint>? initPassive = null)
        {
            var dictionary = new Dictionary<int, Weapon>();
            for (var i = 0; i < MathTools.Min(CommonConfig.OtherConfig.weapon_num, weapons.Length); i++)
            {
                dictionary[i] = Weapon.GenById(weapons[i]);
            }


            if (initPassive == null)
                return new CharacterInitData(gid, teamId, dictionary, size, baseAttrId, null, null);
            var d2 = new Dictionary<passive_id, PassiveTrait>();
            foreach (var passiveTrait in initPassive
                .Select(keyValuePair => PassiveTrait.GenManyByPId(keyValuePair.Key, keyValuePair.Value))
                .SelectMany(genManyByPId => genManyByPId))
            {
                if (d2.TryGetValue(passiveTrait.PassId, out var nowPass))
                {
                    nowPass.AddLevel(passiveTrait.Level);
                }
                else
                {
                    d2[passiveTrait.PassId] = passiveTrait;
                }
            }


            return new CharacterInitData(gid, teamId, dictionary, size, baseAttrId, null, d2);
        }

        private CharacterInitData(int gid, int teamId, Dictionary<int, Weapon> weapons, size bodySize,
            int baseAttrId, int? weaponMaxNum, Dictionary<passive_id, PassiveTrait>? initPassiveTraits)
        {
            Gid = gid;
            TeamId = teamId;
            Weapons = weapons;
            BodySize = bodySize;
            BaseAttrId = baseAttrId;
            WeaponMaxNum = weaponMaxNum;
            InitPassiveTraits = initPassiveTraits;
        }


        public CharacterBody GenCharacterBody(TwoDPoint startPos,
            Dictionary<passive_id, PassiveTrait>? passiveTraits = null, PlayingItemBag? playingItemBag = null)
        {
            var dd = InitPassiveTraits ?? new Dictionary<passive_id, PassiveTrait>();
            if (passiveTraits != null)
            {
                foreach (var initPassiveTrait in passiveTraits)
                {
                    if (dd.TryGetValue(initPassiveTrait.Key, out var nowPass))
                    {
                        nowPass.AddLevel(initPassiveTrait.Value.Level);
                    }
                    else
                    {
                        dd[initPassiveTrait.Key] = initPassiveTrait.Value;
                    }
                }
            }
#if DEBUG
            Console.Out.WriteLine($"pass d to create : {dd.Count}");
#endif
            var characterStatus = new CharacterStatus(Gid, BaseAttrId, playingItemBag ??
                                                                       PlayingItemBag.InitByConfig(), dd,
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