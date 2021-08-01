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
        private BattleNpc(CharacterBody characterBody, WantedBonus wantedBonus, int npcId)
        {
            CharacterBody = characterBody;
            WantedBonus = wantedBonus;
            NpcId = npcId;
        }

        public int NpcId { get; }
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
            var battleNpcWeapons = battleNpc.Weapons;
            var select = battleNpcWeapons.Select(x => x.ChooseRandOne()).ToList();
            var characterInitData =
                CharacterInitData.GenNpcByConfig(gid, team, select, battleNpc.BodyId, battleNpc.AttrId,
                    battleNpc.MaxWeaponSlot);

            var chooseRandCanSame =
                battleNpc.PassiveRange.ChooseRandCanSame(battleNpc.PassiveNum + nowChapterExtraPassiveNum, random);

            var selectMany = chooseRandCanSame.SelectMany(x => PassiveTrait.GenManyById(x, 1));
            var enumerable = battleNpc.MustPassive.SelectMany(x => PassiveTrait.GenManyById(x.pass, x.level));
            var traits = selectMany.Union(enumerable);

            var passiveTraits = traits.GroupBy(x => x.PassId)
                .ToDictionary(x => x.Key, x =>
                {
                    var passiveTrait = x.First();
                    var sum = x.Sum(pp => pp.Level);
                    passiveTrait.SetLevel((uint) sum);
                    return passiveTrait;
                });

            var genCharacterBody = characterInitData.GenCharacterBody(TwoDPoint.Zero(), passiveTraits);
            var characterStatus = genCharacterBody.CharacterStatus;
            characterStatus.FullAmmo();
            characterStatus.SetPropPoint(battleNpc.PropPoint);

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
            var randOne = battleNpc.DropMapInteractableId.ChooseRandOne();
            var canPutInMapInteractable =
                Func(battleNpc.DropMapInteractableType, randOne);
            var mapInteractable = canPutInMapInteractable?.PutInteractable(TwoDPoint.Zero(), true);

            var wantedBonus = new WantedBonus(array, gameItems, mapInteractable);
            var genByConfig = new BattleNpc(genCharacterBody, wantedBonus, battleNpc.id);
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