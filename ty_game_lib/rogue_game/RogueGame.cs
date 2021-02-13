using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_bot;
using game_stuff;

namespace rogue_game
{
    public readonly struct Leave : IGameRequest
    {
    }

    public readonly struct KickPlayer : IGameRequest
    {
        public KickPlayer(int seat)
        {
            Seat = seat;
        }

        public int Seat { get; }
    }

    public readonly struct RebornPlayer : IGameRequest
    {
        public RebornPlayer(int seat)
        {
            Seat = seat;
        }

        public int Seat { get; }
    }

    public interface IGameRequest
    {
    }

    public class RogueGame
    {
        
        private BotTeam BotTeam { get; }
        private int ChapterId { get; set; }
        private Chapter NowChapter { get; set; }
        public Dictionary<int, RogueGamePlayer> NowGamePlayers { get; }
        public int RebornCountDownTick { get; set; }
        public GameItem[] RebornCost { get; }
        public int PlayerLeaderGid { get; set; }

        public PveMap NowPlayMap { get; set; }

        public RogueGame(Queue<Chapter> chapters, Dictionary<int, RogueGamePlayer> nowGamePlayers,
            int rebornCountDownTick, int playerLeader, PveMap nowPlayMap, int chapterId)
        {
            RebornCost = LocalConfig.RogueRebornCost;
            NowChapter = chapters.Dequeue();
            NowGamePlayers = nowGamePlayers;
            RebornCountDownTick = rebornCountDownTick;
            PlayerLeaderGid = playerLeader;
            NowPlayMap = nowPlayMap;
            ChapterId = chapterId;
            BotTeam = new BotTeam();
        }

        private bool Reborn(int seat, int toSeat)
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

        private bool LeaveGame(int seat)
        {
            var leaveGame = NowGamePlayers.Remove(seat);
            if (leaveGame && seat == PlayerLeaderGid && NowGamePlayers.Any())
            {
                PlayerLeaderGid = NowGamePlayers.First().Key;
            }

            return leaveGame;
        }

        private void GoNextChapter()
        {
            ChapterId++;
            NowChapter = Chapter.GenById(ChapterId);
            var nowChapterEntrance = NowChapter.Entrance;
            var characterBodies = NowGamePlayers.Values.Select(x => x.Player).ToArray();
            nowChapterEntrance.AddCharacterBodiesToStart(characterBodies);
        }


        private bool IsPlayerAllDead()
        {
            var all = NowGamePlayers.Values.All(x => x.IsDead);
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

        private bool DoGameRequest(int callSeat, IGameRequest gameRequest)
        {
            return gameRequest switch
            {
                KickPlayer kickPlayer => callSeat == PlayerLeaderGid && LeaveGame(kickPlayer.Seat),
                Leave _ => LeaveGame(callSeat),
                RebornPlayer rebornPlayer => Reborn(callSeat, rebornPlayer.Seat),
                _ => throw new ArgumentOutOfRangeException(nameof(gameRequest))
            };
        }

        // rogue游戏控制台包含核心玩法外的规则，应该与玩法异步运行
        public HashSet<IGameResp> GameConsoleGoATick(Dictionary<int, IGameRequest> gameRequests)
        {
            var gameRespSet = new HashSet<IGameResp>();
            var enumerable = gameRequests.ToImmutableDictionary(x => x.Key, x => DoGameRequest(x.Key, x.Value));
            if (enumerable != null && enumerable.Any())
            {
                gameRespSet.Add(new RequestResult(enumerable));
            }

            if (NowChapter.IsPass())
            {
                GoNextChapter();
                gameRespSet.Add(new ChapterPass());
            }

            if (NowPlayMap.IsClear)
            {
                NowPlayMap.PlayGround.ActiveApplyDevice();
                gameRespSet.Add(new MapClear());
            }

            if (IsFail())
            {
                gameRespSet.Add(new GameFail());
            }

            return gameRespSet;
        }

        public void Start()
        {
        }

        // roguelike接入核心玩法，
        public PlayGroundGoTickResult GamePlayGoATick(Dictionary<int, Operate> opDic)
        {
            var playGroundGoTickResult = NowPlayMap.PlayGroundGoATick(opDic);
            var playerBeHit = playGroundGoTickResult.PlayerBeHit;


            if (playerBeHit.Any())
            {
                NowPlayMap.KillCreep(playerBeHit);

                foreach (var rogueGamePlayer in NowGamePlayers.Values.Where(rogueGamePlayer => !rogueGamePlayer.IsDead)
                )
                {
                    rogueGamePlayer.CheckDead();
                }
            }

            var playerTeleportTo = playGroundGoTickResult.PlayerTeleportTo;
            if (!playerTeleportTo.Any()) return playGroundGoTickResult;

            var (map, toPos) = playerTeleportTo.Values.First();


            NowPlayMap = NowChapter.MGidToMap[map];
            NowPlayMap.TeleportToThisMap(NowGamePlayers.Values.Select(x => (x.Player, toPos)));

            return playGroundGoTickResult;
        }
    }

    public interface IGameResp
    {
    }

    public class GameFail : IGameResp
    {
    }

    public class MapClear : IGameResp
    {
    }

    public class ChapterPass : IGameResp
    {
    }

    public class RequestResult : IGameResp
    {
        public ImmutableDictionary<int, bool> GidToOk { get; }

        public RequestResult(ImmutableDictionary<int, bool> gidToOk)
        {
            GidToOk = gidToOk;
        }
    }
}