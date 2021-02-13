using System;
using System.Linq;
using collision_and_rigid;
using game_config;
using game_stuff;

namespace rogue_game
{
    public class BattleNpc
    {
        public BattleNpc(CharacterBody characterBody, WantedBonus wantedBonus)
        {
            CharacterBody = characterBody;
            WantedBonus = wantedBonus;
        }

        public CharacterBody CharacterBody { get; }
        public WantedBonus WantedBonus { get; }

        public static BattleNpc GenByConfig(battle_npc battleNpc, int gid, int team)
        {
            var characterInitData =
                CharacterInitData.GenByIds(gid, team, battleNpc.Weapons, battleNpc.BodyId, battleNpc.AttrId);
            var genCharacterBody = characterInitData.GenCharacterBody(TwoDPoint.Zero());

            static ICanPutInMapInteractable Func(interactableType t, int id)
            {
                return t switch
                {
                    interactableType.weapon => Weapon.GenById(id),
                    interactableType.prop => Prop.GenById(id),
                    interactableType.vehicle => Vehicle.GenById(id),
                    _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
                };
            }

            ICanPutInMapInteractable canPutInMapInteractable =
                Func(battleNpc.DropMapInteractableType, battleNpc.DropMapInteractableId);
            var mapInteractable = canPutInMapInteractable.PutInteractable(TwoDPoint.Zero(), true);
            var gameItems = battleNpc.KillDrops.Select(GameItem.GenByConfigGain).ToArray();
            var array = battleNpc.AllDrops.Select(GameItem.GenByConfigGain).ToArray();
            var wantedBonus = new WantedBonus(array, gameItems, mapInteractable);
            return new BattleNpc(genCharacterBody, wantedBonus);
        }
    }
}