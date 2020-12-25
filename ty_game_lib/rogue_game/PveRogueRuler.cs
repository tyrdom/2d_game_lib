using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using game_stuff;

namespace rogue_game
{
    public class PveRogueRuler
    {
        public PveRogueRuler(PveWinCond pveWinCond, GameItem winBonus, IMapInteractable winSpawn, int playerInTeam,
            Dictionary<int, CharacterBody> gamePlayers)
        {
            PveWinCond = pveWinCond;
            WinBonus = winBonus;
            WinSpawn = winSpawn;
            PlayerInTeam = playerInTeam;
            GamePlayers = gamePlayers;
            DeadPool = new HashSet<CharacterBody>();
        }

        public Dictionary<int, CharacterBody> GamePlayers { get; }
        public int CountDownTick { get; set; }
        public HashSet<CharacterBody> DeadPool { get; }
        public int PlayerInTeam { get; }
        public PveWinCond PveWinCond { get; }

        public GameItem WinBonus { get; }

        public IMapInteractable WinSpawn { get; }

        public PveResult RulerGoTick()
        {
            var pveResultType = CheckPve(PveWinCond, DeadPool);
            if (pveResultType == PveResultType.PveLoss)
            {
                if (CountDownTick <= 0) return new PveResult(pveResultType);
                CountDownTick -= 1;
                pveResultType = PveResultType.NotFinish;
            }
            else
            {
                CountDownTick = TempConfig.RogueRebornTick;
            }

            return new PveResult(pveResultType);
        }

        private static ImmutableDictionary<int, LevelUpsData> RogueLevelData()
        {
            var levelUpsData = new LevelUpsData(null, TempConfig.RogueRebornTick, TempConfig.RogueRebornCost,
                new int[] { });
            var levelUpsDates = new Dictionary<int, LevelUpsData> {{1, levelUpsData}};
            return levelUpsDates.ToImmutableDictionary();
        }

        public LevelUps GetLevelUp()
        {
            return new LevelUps(RogueLevelData());
        }

        public PveResultType CheckPve(PveWinCond pveWinCond,
            HashSet<CharacterBody> deadPool)
        {
            var groupBy = GamePlayers.Values.GroupBy(x => x.BodyMark);
            var creepClear = false;
            var bossClear = false;

            foreach (var characterBodies in groupBy)
            {
                switch (characterBodies.Key)
                {
                    case BodyMark.Player:
                        var enumerable = characterBodies.Where(x => x.CharacterStatus.SurvivalStatus.IsDead());
                        deadPool.UnionWith(enumerable);
                        var all = characterBodies.All(x => x.CharacterStatus.SurvivalStatus.IsDead());
                        if (all)
                        {
                            return PveResultType.PveLoss;
                        }

                        break;
                    case BodyMark.Creep:
                        var all2 = characterBodies.All(x => x.CharacterStatus.SurvivalStatus.IsDead());
                        if (all2)
                        {
                            creepClear = true;
                        }

                        break;
                    case BodyMark.Boss:
                        var all3 = characterBodies.All(x => x.CharacterStatus.SurvivalStatus.IsDead());
                        if (all3)
                        {
                            bossClear = true;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return pveWinCond switch
            {
                PveWinCond.AllClear => bossClear && creepClear
                    ? PveResultType.PveWin
                    : PveResultType.NotFinish,
                PveWinCond.BossClear => bossClear
                    ? PveResultType.PveWin
                    : PveResultType.NotFinish,
                _ => throw new ArgumentOutOfRangeException(nameof(pveWinCond), pveWinCond, null)
            };
        }
    }
}