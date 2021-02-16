using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        private Queue<int> ChapterIds { get; }
        private Chapter NowChapter { get; set; }
        public Dictionary<int, RogueGamePlayer> NowGamePlayers { get; }
        public int RebornCountDownTick { get; set; }
        public GameItem[] RebornCost { get; }
        public int PlayerLeaderGid { get; set; }

        public PveMap NowPlayMap { get; set; }

        public Random Random { get; }

        public RogueGame(HashSet<CharacterBody> characterBodies,
            int rebornCountDownTick, int playerLeader, PveMap nowPlayMap, IEnumerable<int> chapterIds)
        {
            ChapterIds =
                chapterIds
                    .Aggregate(new Queue<int>(), (ints, i) =>
                    {
                        ints.Enqueue(i);
                        return ints;
                    });
            Random = new Random();
            RebornCost = LocalConfig.RogueRebornCost;
            NowChapter = Chapter.GenMapsById(ChapterIds.Dequeue(), Random);
            NowGamePlayers = characterBodies.ToDictionary(x => x.GetId(), x => new RogueGamePlayer(x));
            RebornCountDownTick = rebornCountDownTick;
            PlayerLeaderGid = playerLeader;
            NowPlayMap = nowPlayMap;
            BotTeam = new BotTeam();
            NowChapter.Entrance.AddCharacterBodiesToStart(characterBodies);
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

        private bool LeaveGame(int gidSeat)
        {
            var leaveGame = NowGamePlayers.Remove(gidSeat);
            if (leaveGame && gidSeat == PlayerLeaderGid && NowGamePlayers.Any())
            {
                PlayerLeaderGid = NowGamePlayers.First().Key;
            }

            return leaveGame;
        }

        private void GoNextChapter()
        {
            var dequeue = ChapterIds.Dequeue();
            NowChapter = Chapter.GenMapsById(dequeue, Random);
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
            var enumerable = gameRequests
                .ToImmutableDictionary(x => x.Key,
                    x => DoGameRequest(x.Key, x.Value));
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

        // roguelike接入核心玩法，
        public PlayGroundGoTickResult GamePlayGoATick(Dictionary<int, Operate> opDic)
        {
            foreach (var botTeamTempOpThink in BotTeam.TempOpThinks.Where(botTeamTempOpThink =>
                botTeamTempOpThink.Value.Operate != null))
            {
                opDic[botTeamTempOpThink.Key] = botTeamTempOpThink.Value.Operate!;
            }

            var playGroundGoTickResult = NowPlayMap.PlayGroundGoATick(opDic);
            BotTeam.AllBotsGoATick(playGroundGoTickResult.PlayerSee);
            var characterBeHit = playGroundGoTickResult.CharacterBeHit;

            if (characterBeHit.Any())
            {
                var (all, kill, mapInteractableList) =
                    NowPlayMap.KillCreep(characterBeHit);

                foreach (var rogueGamePlayer in NowGamePlayers)
                {
                    rogueGamePlayer.Value.AddItem(all);
                    if (kill.TryGetValue(rogueGamePlayer.Key, out var enumerable))
                    {
                        rogueGamePlayer.Value.AddItem(enumerable);
                    }

                    if (!rogueGamePlayer.Value.IsDead)
                    {
                        rogueGamePlayer.Value.CheckDead();
                    }
                }

                foreach (var mapInteractable in mapInteractableList)
                {
                    NowPlayMap.PlayGround.AddMapInteractable(mapInteractable);
                }
            }

            var playerTeleportTo = playGroundGoTickResult.PlayerTeleportTo;
            if (!playerTeleportTo.Any()) return playGroundGoTickResult;
            var (map, toPos) = playerTeleportTo.Values.First();
            NowPlayMap = NowChapter.MGidToMap[map];
            NowPlayMap.TeleportToThisMap(NowGamePlayers.Values.Select(x => (x.Player, toPos)));
            var valueTuples = NowPlayMap.SpawnNpc(Random);
            BotTeam.SetNaviMaps(NowPlayMap.PlayGround.ResMId);
            BotTeam.SetBots(valueTuples, Random);
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