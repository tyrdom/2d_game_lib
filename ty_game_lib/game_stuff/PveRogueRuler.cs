using System.Collections.Generic;
using System.Collections.Immutable;

namespace game_stuff
{
    public class PveRogueRuler : IPlayRuler
    {
        public PveRogueRuler(PveWinCond pveWinCond, GameItem winBonus, IMapInteractable winSpawn, int playerInTeam)
        {
            PveWinCond = pveWinCond;
            WinBonus = winBonus;
            WinSpawn = winSpawn;
            PlayerInTeam = playerInTeam;
            GidToRebornPool = new Dictionary<int, RebornUnit>();
        }

        private Dictionary<int, RebornUnit> GidToRebornPool { get; }
        public int PlayerInTeam { get; }
        public PveWinCond PveWinCond { get; }

        public GameItem WinBonus { get; }

        public IMapInteractable WinSpawn { get; }

        public IRuleTickResult RulerGoTick(PlayGround playGround)
        {
            var pveResultType = playGround.CheckPve(PveWinCond);

            return new PveResult(pveResultType.pveResultType);
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
    }
}