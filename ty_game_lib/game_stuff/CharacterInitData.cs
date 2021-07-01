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
        private weapon_id[] Weapons { get; }
        private size BodySize { get; }
        private int BaseAttrId { get; }

        private Dictionary<passive_id, uint>? InitPassiveTraits { get; }
        private int? WeaponMaxNum { get; }

        public static CharacterInitData GenNpcByConfig(int gid, int teamId, string[] weapons, size size,
            int baseAttrId, int battleNpcMaxWeaponSlot)
        {
            var weaponIds = weapons.Select(Weapon.GenId).ToArray();
            // var dictionary = new Dictionary<int, weapon_id>();
            // for (var i = 0; i < weapons.Length; i++)
            // {
            //     dictionary[i] = Weapon.GenId(weapons[i]);
            // }


            return new CharacterInitData(gid, teamId, weaponIds, size, baseAttrId, battleNpcMaxWeaponSlot,
                new Dictionary<passive_id, uint>());
        }

        public static CharacterInitData GenPlayerByConfig(int gid, int teamId, weapon_id[] weapons, size size,
            int baseAttrId, Dictionary<passive_id, uint>? initPassive = null)
        {
            var min = MathTools.Min(CommonConfig.OtherConfig.weapon_num, weapons.Length);
            var weaponIds = weapons.Take(min).ToArray();


            return initPassive == null
                ? new CharacterInitData(gid, teamId, weaponIds, size, baseAttrId, null, null)
                : new CharacterInitData(gid, teamId, weaponIds, size, baseAttrId, null, initPassive);
            // var d2 = new Dictionary<passive_id, PassiveTrait>();
        }

        private CharacterInitData(int gid, int teamId, weapon_id[] weapons, size bodySize,
            int baseAttrId, int? weaponMaxNum, Dictionary<passive_id, uint>? initPassiveTraits)
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
            var dd = passiveTraits ?? new Dictionary<passive_id, PassiveTrait>();
            if (InitPassiveTraits != null)
            {
                foreach (var passiveTrait in InitPassiveTraits
                    .Select(keyValuePair => PassiveTrait.GenManyByPId(keyValuePair.Key, keyValuePair.Value))
                    .SelectMany(genManyByPId => genManyByPId))
                {
                    if (dd.TryGetValue(passiveTrait.PassId, out var nowPass))
                    {
                        nowPass.AddLevel(passiveTrait.Level);
                    }
                    else
                    {
                        dd[passiveTrait.PassId] = passiveTrait;
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
            foreach (var weapon in Weapons.Select(Weapon.GenById))
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