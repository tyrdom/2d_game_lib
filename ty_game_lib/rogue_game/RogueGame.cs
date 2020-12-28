using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Permissions;
using game_stuff;

namespace rogue_game
{
    public enum GameReqType
    {
        KickGame,
        LeaveGame,
        Reborn
    }

    public readonly struct GameRequest
    {
        public GameReqType GameReqType { get; }
        public int Seat { get; }
    }

    public struct RogueGamePlayer
    {
        public RogueGamePlayer(CharacterBody player, PveMap inPlayPveMap)
        {
            Player = player;
            InPlayGround = inPlayPveMap;
        }

        public CharacterBody Player { get; }
        public PveMap InPlayGround { get; set; }
    }

    public class RogueGame
    {
        private Queue<Chapter> Chapters { get; }
        private Chapter NowChapter { get; set; }
        public Dictionary<int, RogueGamePlayer> NowGamePlayers { get; }
        public int RebornCountDownTick { get; set; }
        public int PlayerLeaderSeat { get; set; }

        public RogueGame(Queue<Chapter> chapters, Dictionary<int, RogueGamePlayer> nowGamePlayers,
            int rebornCountDownTick, int playerLeader)
        {
            Chapters = chapters;
            NowChapter = chapters.Dequeue();
            NowGamePlayers = nowGamePlayers;
            RebornCountDownTick = rebornCountDownTick;
            PlayerLeaderSeat = playerLeader;
        }

        public bool LeaveGame(int seat)
        {
            return NowGamePlayers.Remove(seat);
        }

        public void GoNextChapter()
        {
            //todo
        }

        private static ImmutableDictionary<int, LevelUpsData> RogueLevelData()
        {
            var levelUpsData = new LevelUpsData(null, LocalConfig.RogueRebornTick, LocalConfig.RogueRebornCost,
                new int[] { });
            var levelUpsDates = new Dictionary<int, LevelUpsData> {{1, levelUpsData}};
            return levelUpsDates.ToImmutableDictionary();
        }


        private bool IsPlayerAllDead()
        {
            var all = NowGamePlayers.Values.All(x => x.Player.CharacterStatus.SurvivalStatus.IsDead());
            return all;
        }


        private bool IsNowChapterPass()
        {
            return NowChapter.IsPass();
        }

        private bool IsFail()
        {
            var any = NowGamePlayers.Any();
            if (any)
            {
                return true;
            }

            if (IsPlayerAllDead())
            {
                RebornCountDownTick -= 1;
                return RebornCountDownTick < 0;
            }

            RebornCountDownTick = LocalConfig.RogueRebornTick;
            return false;
        }

        public LevelUps GetLevelUp()
        {
            return new LevelUps(RogueLevelData());
        }

        public void GameRuleCheckGoATick()
        {
        }

        public void PlayGroundGoATick(Dictionary<int, Operate> requestDic)
        {
            var groupBy = NowGamePlayers.Values.GroupBy(x => x.InPlayGround);
            foreach (var rogueGamePlayers in groupBy)
            {
                var valuePairs = requestDic.Where(x => rogueGamePlayers.Select(r => r.Player.GetId()).Contains(x.Key))
                    .ToDictionary(p => p.Key, p => p.Value);
                var ((playerBeHit, trapBeHit), playerSeeMsg) = rogueGamePlayers.Key.PlayGroundGoATick(valuePairs);
            }

            //todo return msg
        }
    }
}