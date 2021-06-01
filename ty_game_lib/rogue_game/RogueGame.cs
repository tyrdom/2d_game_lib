using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using game_bot;
using game_config;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    public readonly struct GoNextChapter : IGameRequest
    {
    }

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
        public BotTeam BotTeam { get; }
        private Queue<int> ChapterIds { get; }
        private Chapter NowChapter { get; set; }
        public Dictionary<int, RogueGamePlayer> NowGamePlayers { get; }
        public int RebornCountDownTick { get; set; }

        public int ChapterCountDownTick { get; set; }
        public GameItem[] RebornCost { get; }
        public int PlayerLeaderGid { get; private set; }

        public PveMap NowPlayMap { get; set; }

        public bool NeedCheckClear { get; set; }
        public Random Random { get; }


        public static (RogueGame genByConfig, ImmutableHashSet<IGameResp> gameResps) GenByConfig(
            HashSet<CharacterBody> characterBodies,
            CharacterBody leader)
        {
            var gameResps = new HashSet<IGameResp>();
            var otherConfig = CommonConfig.OtherConfig;
            var otherConfigRogueChapters = otherConfig.RogueChapters;
            var genByConfig = new RogueGame(characterBodies, leader.GetId(), otherConfigRogueChapters,
                out var ySlotArray);
            var pushChapterGoNext = new PushChapterGoNext(ySlotArray);
            gameResps.Add(pushChapterGoNext);
            return (genByConfig, gameResps.ToImmutableHashSet());
        }

        private RogueGame(HashSet<CharacterBody> characterBodies, int playerLeader, IEnumerable<int> chapterIds,
            out (int x, MapType MapType, int GId)[][] ySlotArray)
        {
            ChapterIds =
                chapterIds
                    .Aggregate(new Queue<int>(), (ints, i) =>
                    {
                        ints.Enqueue(i);
                        return ints;
                    });
            Random = new Random();
            RebornCost = RogueLocalConfig.RogueRebornCost;
            var genChapterById = Chapter.GenChapterById(ChapterIds.Dequeue(), Random);
            NowChapter = genChapterById.genByConfig;
            ySlotArray = genChapterById.ySlotArray;
            NowGamePlayers = characterBodies.ToDictionary(x => x.GetId(), x => new RogueGamePlayer(x));
            RebornCountDownTick = RogueLocalConfig.RogueRebornTick;
            ChapterCountDownTick = -1;
            PlayerLeaderGid = playerLeader;
            NowPlayMap = NowChapter.Entrance;
            BotTeam = new BotTeam();
            NowChapter.Entrance.AddCharacterBodiesToStart(characterBodies);
            NeedCheckClear = true;
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

        private IGameResp GoNextChapter()
        {
            NowPlayMap.TelePortOut();
            BotTeam.ClearBot();
            var dequeue = ChapterIds.Dequeue();
            var genChapterById = Chapter.GenChapterById(dequeue, Random);
            NowChapter = genChapterById.genByConfig;
            var ySlotArray = genChapterById.ySlotArray;
            var nowChapterEntrance = NowChapter.Entrance;
            var characterBodies = NowGamePlayers.Values.Select(x => x.Player).ToArray();
            nowChapterEntrance.AddCharacterBodiesToStart(characterBodies);
            NowPlayMap = nowChapterEntrance;
            ChapterCountDownTick = -1;
            var pushChapterGoNext = new PushChapterGoNext(ySlotArray);
            return pushChapterGoNext;
        }

        private bool IsPlayerAllDead()
        {
            var all = NowGamePlayers.Values.All(x => x.IsDead);
            return all;
        }


        private bool IsFail()
        {
            var any = NowGamePlayers.Any();
            if (!any)
            {
                return true;
            }

            if (IsPlayerAllDead())
            {
                RebornCountDownTick -= 1;
                return RebornCountDownTick < 0;
            }

            RebornCountDownTick = RogueLocalConfig.RogueRebornTick;
            return false;
        }


        private IGameResp DoGameRequest(int callSeat, IGameRequest gameRequest, ISet<IGameResp> collector)
        {
            return gameRequest switch
            {
                GoNextChapter _ => ChapterCountDownTick > 0 ? GoNextChapter() : new QuestFail(callSeat),
                KickPlayer kickPlayer => callSeat == PlayerLeaderGid && LeaveGame(kickPlayer.Seat)
                    ? (IGameResp) new QuestOkResult(callSeat, kickPlayer)
                    : new QuestFail(callSeat),
                Leave leave => LeaveGame(callSeat)
                    ? (IGameResp) new QuestOkResult(callSeat, leave)
                    : new QuestFail(callSeat),
                RebornPlayer rebornPlayer => Reborn(callSeat, rebornPlayer.Seat)
                    ? (IGameResp) new QuestOkResult(callSeat, rebornPlayer)
                    : new QuestFail(callSeat),

                _ => throw new ArgumentOutOfRangeException(nameof(gameRequest))
            };
        }

        // rogue游戏控制台包含核心玩法外的规则，应该与玩法异步运行
        public ImmutableHashSet<IGameResp> GameConsoleGoATick(Dictionary<int, IGameRequest> gameRequests)
        {
            var gameRespSet = new HashSet<IGameResp>();
            var enumerable = gameRequests
                .Select(
                    x => DoGameRequest(x.Key, x.Value, gameRespSet)).ToArray();
            if (enumerable.Any())
            {
                gameRespSet.UnionWith(enumerable);
            }

            if (ChapterCountDownTick > 0)
            {
                ChapterCountDownTick--;
                return gameRespSet.ToImmutableHashSet();
            }

            if (ChapterCountDownTick == 0)
            {
                var goNextChapter = GoNextChapter();
                gameRespSet.Add(goNextChapter);
                return gameRespSet.ToImmutableHashSet();
            }

            if (NowChapter.IsPass())
            {
                ChapterCountDownTick = RogueLocalConfig.ChapterPassTick;
                gameRespSet.Add(new GameMsgPush(GamePushMsg.ChapterPass));
                return gameRespSet.ToImmutableHashSet();
            }
#if DEBUG
            // Console.Out.WriteLine($" clear!!~~~~~~~~~{NowPlayMap.IsClear}");
#endif
            if (NeedCheckClear && NowPlayMap.IsClear)
            {
                NowPlayMap.PlayGround.ActiveApplyDevice();
#if DEBUG
                // Console.Out.WriteLine(
                //     $" app!!~~~~~~~~~{NowPlayMap.PlayGround.GetMapApplyDevices().All(x => x.IsActive)}");
#endif
                NeedCheckClear = false;
                gameRespSet.Add(new GameMsgPush(GamePushMsg.MapClear));
            }

            if (IsFail())
            {
                gameRespSet.Add(new GameMsgPush(GamePushMsg.GameFail));
            }

            return gameRespSet.ToImmutableHashSet();
        }

        // roguelike接入核心玩法，
        public RogueGameGoTickResult GamePlayGoATick(Dictionary<int, Operate> opDic)
        {
            foreach (var botTeamTempOpThink in BotTeam.TempOpThinks.Where(botTeamTempOpThink =>
                botTeamTempOpThink.Value.Operate != null))
            {
                opDic[botTeamTempOpThink.Key] = botTeamTempOpThink.Value.Operate!;
            }

            var playGroundGoTickResult = NowPlayMap.PlayGroundGoATick(opDic);
            BotTeam.AllBotsGoATick(playGroundGoTickResult);
            var characterBeHit = playGroundGoTickResult.CharacterGidBeHit;

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
            if (!playerTeleportTo.Any()) return new RogueGameGoTickResult(playGroundGoTickResult, false);
            var (map, toPos) = playerTeleportTo.Values.First();
            if (NowPlayMap.PlayGround.MgId == map) return new RogueGameGoTickResult(playGroundGoTickResult, false);
#if DEBUG
            Console.Out.WriteLine($"map change ~~~~{map} ~~ {toPos}");
#endif
            NowPlayMap.TelePortOut();
            NowPlayMap = NowChapter.MGidToMap[map];
            NowPlayMap.TeleportToThisMap(NowGamePlayers.Values.Select(x => (x.Player, toPos)));
            NeedCheckClear = true;
            if (NowPlayMap.IsClear) return new RogueGameGoTickResult(playGroundGoTickResult, true);
            BotTeam.SetNaviMaps(NowPlayMap.PlayGround.ResMId);

            NowPlayMap.SpawnNpcWithBot(Random, BotTeam, NowChapter.ExtraPassiveNum);

            return new RogueGameGoTickResult(playGroundGoTickResult, true);
        }

        public void ForceSpawnNpc()
        {
            BotTeam.SetNaviMaps(NowPlayMap.PlayGround.ResMId);

            NowPlayMap.SpawnNpcWithBot(Random, BotTeam, NowChapter.ExtraPassiveNum);
        }
    }

    public readonly struct RogueGameGoTickResult
    {
        public RogueGameGoTickResult(PlayGroundGoTickResult playGroundGoTickResult, bool mapChange)
        {
            PlayGroundGoTickResult = playGroundGoTickResult;
            MapChange = mapChange;
        }

        public PlayGroundGoTickResult PlayGroundGoTickResult { get; }

        public bool MapChange { get; }
    }

    public interface IGameResp
    {
    }

    public enum GameRequestType
    {
        KickPlayer,
        Leave,
        RebornPlayer
    }

    public class QuestFail : IGameResp
    {
        public int PlayerGid;

        public QuestFail(int playerGid)
        {
            PlayerGid = playerGid;
        }
    }

    public class QuestOkResult : IGameResp
    {
        public QuestOkResult(int playerGid, IGameRequest gameRequest)
        {
            PlayerGid = playerGid;
            var gameRequestType = gameRequest switch
            {
                KickPlayer kickPlayer => rogue_game.GameRequestType.KickPlayer,
                Leave leave => rogue_game.GameRequestType.Leave,
                RebornPlayer rebornPlayer => rogue_game.GameRequestType.RebornPlayer,
                _ => throw new ArgumentOutOfRangeException(nameof(gameRequest))
            };

            GameRequestType = gameRequestType;
        }

        public GameRequestType GameRequestType { get; }

        public int PlayerGid { get; }
    }

    public class PushChapterGoNext : IGameResp
    {
        public PushChapterGoNext((int x, MapType MapType, int GMid)[][] ySlotArray)
        {
            YSlotArray = ySlotArray;
        }

        public (int x, MapType MapType, int GMid)[][] YSlotArray { get; }
    }

    public class GameMsgPush : IGameResp
    {
        public GameMsgPush(GamePushMsg gameMsgToPush)
        {
            GameMsgToPush = gameMsgToPush;
        }

        public GamePushMsg GameMsgToPush { get; }
    }

    public enum GamePushMsg
    {
        GameFail,
        MapClear,
        ChapterPass,
    }
}