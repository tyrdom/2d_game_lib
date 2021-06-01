using System;
using System.Collections.Generic;
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


        public static (BattleNpc, int boId) GenById(int id, int gid, int team, Random random,
            int nowChapterExtraPassiveNum)
        {
            return CommonConfig.Configs.battle_npcs.TryGetValue(id, out var battleNpc)
                ? GenByConfig(battleNpc, gid, team, random, nowChapterExtraPassiveNum)
                : throw new KeyNotFoundException();
        }

        private static (BattleNpc, int boId) GenByConfig(battle_npc battleNpc, int gid, int team, Random random,
            int nowChapterExtraPassiveNum)
        {
            var characterInitData =
                CharacterInitData.GenNpcByConfig(gid, team, battleNpc.Weapons, battleNpc.BodyId, battleNpc.AttrId,
                    battleNpc.MaxWeaponSlot);

            var chooseRandCanSame =
                battleNpc.PassiveRange.ChooseRandCanSame(battleNpc.PassiveNum + nowChapterExtraPassiveNum, random);
            var passiveTraits = chooseRandCanSame.GroupBy(x => x).ToDictionary(
                p => p.Key.TryStringToEnum(out passive_id passiveId)
                    ? passiveId
                    : throw new Exception($"not such id {p.Key}"),
                p => PassiveTrait.GenById(p.Key, (uint) p.Count()));

            var genCharacterBody = characterInitData.GenCharacterBody(TwoDPoint.Zero(), passiveTraits);
            var characterStatus = genCharacterBody.CharacterStatus;
            characterStatus.FullAmmo();
            var battleNpcWithVehicleId = battleNpc.WithVehicleId;
            if (battleNpcWithVehicleId != "")
            {
                var genById = Vehicle.GenById(battleNpcWithVehicleId);

                characterStatus.GetInAVehicle(genById);
            }

            var battleNpcPropIds = battleNpc.PropIds;
            if (battleNpcPropIds.Any())
            {
                var chooseRandOne = battleNpcPropIds.ChooseRandOne(random);
                var byId = Prop.GenById(chooseRandOne);
                characterStatus.PickAProp(byId);
            }

            static ICanPutInMapInteractable? Func(interactableType t, string id)
            {
                return t switch
                {
                    interactableType.weapon => Weapon.GenById(id),
                    interactableType.prop => Prop.GenById(id),
                    interactableType.vehicle => Vehicle.GenById(id),
                    _ => null
                };
            }

            var gameItems = battleNpc.KillDrops.Select(GameItem.GenByConfigGain).ToArray();
            var array = battleNpc.AllDrops.Select(GameItem.GenByConfigGain).ToArray();
            var canPutInMapInteractable =
                Func(battleNpc.DropMapInteractableType, battleNpc.DropMapInteractableId);
            var mapInteractable = canPutInMapInteractable?.PutInteractable(TwoDPoint.Zero(), true);

            var wantedBonus = new WantedBonus(array, gameItems, mapInteractable);
            var genByConfig = new BattleNpc(genCharacterBody, wantedBonus);
            var battleNpcBotId = battleNpc.botId;

            var b = battleNpc.botId == 0;
            if (b)
            {
                throw new Exception($"no good bot id in {battleNpc.id}");
            }

            return (genByConfig, battleNpcBotId);
        }
    }
}