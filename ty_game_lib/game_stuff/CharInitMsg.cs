using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CharInitMsg : ICharMsg
    {
        public int GId { get; }
        public HashSet<ICharEvent> CharEvents { get; }


        public CharInitMsg(int gId, TwoDPoint pos, TwoDVector aim, SurvivalStatus survivalStatus,
            Weapon weaponConfigs, Dictionary<passive_id, PassiveTrait> characterStatusPassiveTraits,
            PlayingItemBag characterStatusPlayingItemBag, Prop? characterStatusProp)
        {
            GId = gId;
            var posChange = new PosChange(pos);
            var aimChange = new AimChange(aim);
            var newSurvivalStatus = survivalStatus.GenNewMsg();
            var getPassives = characterStatusPassiveTraits.Values.Select(x => new GetPassive(x.PassId, x.Level));
            var gameItems = characterStatusPlayingItemBag.GameItems.Select(x => new GameItem(x.Key, x.Value)).ToArray();
            var itemChange = new ItemDetailChange(gameItems);
            var pickWeapons = new SwitchWeapon(weaponConfigs.WId);

            var charEvents = new HashSet<ICharEvent> {posChange, aimChange, newSurvivalStatus, pickWeapons, itemChange};
            charEvents.UnionWith(getPassives);
            if (characterStatusProp != null)
            {
                var propId = characterStatusProp.PId;
                var pickAProp = new PickAProp(propId);
                charEvents.Add(pickAProp);
            }

            CharEvents = charEvents;
        }
    }
}