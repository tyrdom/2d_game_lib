using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using game_stuff;

namespace rogue_game
{
    public readonly struct Leave : IGameRequest
    {
    }

    public readonly struct KickPlayer : IGameRequest
    {
        public int Seat { get; }
    }

    public readonly struct RebornPlayer : IGameRequest
    {
        public int Seat { get; }
    }

    public interface IGameRequest
    {
    }

    public struct RogueGamePlayer
    {
        public RogueGamePlayer(CharacterBody player, PveMap inPlayPveMap)
        {
            Player = player;
            InPlayGround = inPlayPveMap;
            FinalBill = new Deal();
        }

        public CharacterBody Player { get; }
        public PveMap InPlayGround { get; set; }

        public void SetPveMap(PveMap pveMap)
        {
            InPlayGround = pveMap;
        }

        public Deal FinalBill { get; }
    }

    public class Deal
    {
        public Dictionary<int, int> Cost { get; }
        public Dictionary<int, int> Gain { get; }

        public Deal()
        {
            Cost = new Dictionary<int, int>();
            Gain = new Dictionary<int, int>();
        }

        private static void Add(IEnumerable<GameItem> gameItems, IDictionary<int, int> add)
        {
            {
                var sumSame = GameItem.SumSame(gameItems);
                var enumerable = sumSame.Where(x => LocalConfig.CanEndRest(x.ItemId));
                foreach (var gameItem in enumerable)
                {
                    var gameItemItemId = gameItem.ItemId;
                    var itemNum = gameItem.Num;
                    if (add.TryGetValue(gameItemItemId, out var num))
                    {
                        var gameItemNum = num + itemNum;
                        add[gameItemItemId] = gameItemNum;
                    }
                    else
                    {
                        add[gameItemItemId] = itemNum;
                    }
                }
            }
        }

        public void AddCost(IEnumerable<GameItem> gameItems)
        {
            Add(gameItems, Cost);
        }

        public void AddGain(IEnumerable<GameItem> gameItems)
        {
            Add(gameItems, Gain);
        }
    }

    public class RogueGame
    {
        private Queue<Chapter> Chapters { get; }
        private Chapter NowChapter { get; set; }
        public Dictionary<int, RogueGamePlayer> NowGamePlayers { get; }
        public int RebornCountDownTick { get; set; }

        public GameItem[] RebornCost { get; }
        public int PlayerLeaderSeat { get; set; }

        public RogueGame(Queue<Chapter> chapters, Dictionary<int, RogueGamePlayer> nowGamePlayers,
            int rebornCountDownTick, int playerLeader)
        {
            RebornCost = LocalConfig.RogueRebornCost;
            Chapters = chapters;
            NowChapter = chapters.Dequeue();
            NowGamePlayers = nowGamePlayers;
            RebornCountDownTick = rebornCountDownTick;
            PlayerLeaderSeat = playerLeader;
        }

        public bool Reborn(int seat, int toSeat)
        {
            if (!NowGamePlayers.TryGetValue(seat, out var player)) return false;
            var costOk = player.Player.CharacterStatus.PlayingItemBag.Cost(RebornCost);
            if (!costOk) return false;
            player.FinalBill.AddCost(RebornCost);

            if (NowGamePlayers.TryGetValue(toSeat, out var player2))
            {
                player2.Player.ReBorn(player.Player.GetAnchor());
            }

            return true;
        }

        public bool LeaveGame(int seat)
        {
            var leaveGame = NowGamePlayers.Remove(seat);
            if (leaveGame && seat == PlayerLeaderSeat && NowGamePlayers.Any())
            {
                PlayerLeaderSeat = NowGamePlayers.First().Key;
            }

            return leaveGame;
        }

        public bool GoNextChapter()
        {
            if (!Chapters.Any()) return false;
            var characterBodies = NowGamePlayers.Values.Select(x => x.Player).ToArray();
            NowChapter = Chapters.Dequeue();
            var nowChapterEntrance = NowChapter.Entrance;
            nowChapterEntrance.AddPlayers(characterBodies);
            foreach (var gamePlayer in NowGamePlayers.Select(rogueGamePlayer => rogueGamePlayer.Value))
            {
                gamePlayer.SetPveMap(nowChapterEntrance);
            }

            return true;
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

        public bool DoGameRequest(int callSeat, IGameRequest gameRequest)
        {
            return gameRequest switch
            {
                KickPlayer kickPlayer => callSeat == PlayerLeaderSeat && LeaveGame(kickPlayer.Seat),
                Leave leave => LeaveGame(callSeat),
                RebornPlayer rebornPlayer => Reborn(callSeat, rebornPlayer.Seat),
                _ => throw new ArgumentOutOfRangeException(nameof(gameRequest))
            };
        }

        public void GameRuleCheckGoATick(Dictionary<int, IGameRequest> gameRequests)
        {
            var enumerable = gameRequests.Select(x => DoGameRequest(x.Key, x.Value));


            if (NowChapter.IsPass())
            {
                var goNextChapterOk = GoNextChapter();
            }

            IsFail();
        }

        public PlayGroundGoTickResult GamePlayGoATick(Dictionary<int, Operate> opDic)
        {
            var groupBy = NowGamePlayers.Values.GroupBy(x => x.InPlayGround);

            var playGroundGoTickResults = groupBy.Select(rp =>
            {
                var valuePairs = opDic.Where(x => rp.Select(r => r.Player.GetId()).Contains(x.Key))
                    .ToDictionary(p => p.Key, p => p.Value);
                return rp.Key.PlayGroundGoATick(valuePairs);
            });
            var playGroundGoTickResult = PlayGroundGoTickResult.Sum(playGroundGoTickResults);
            var (_, _, _, teleportTo)
                = playGroundGoTickResult;
            foreach (var keyValuePair in teleportTo)
            {
                if (NowGamePlayers.TryGetValue(keyValuePair.Key, out var gamePlayer) &&
                    NowChapter.MGidToMap.TryGetValue(keyValuePair.Value, out var pveMap))
                {
                    gamePlayer.InPlayGround = pveMap;
                }

                throw new DirectoryNotFoundException();
            }

            return playGroundGoTickResult;
        }
    }
}