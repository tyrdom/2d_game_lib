using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_bot;
using game_config;
using game_stuff;
using rogue_chapter_maker;

namespace rogue_game
{
    [Serializable]
    public class RogueGame
    {
        public BotTeam BotTeam { get; }
        public Queue<int> ChapterIds { get; set; }
        public Chapter NowChapter { get; set; }
        public HashSet<CharacterInitData> CharacterInitDataS { get; }
        public Dictionary<int, RogueGamePlayer> NowGamePlayers { get; set; }
        public int RebornCountDownTick { get; set; }

        public bool Pause { get; set; }
        public int ChapterCountDownTick { get; set; }
        public GameItem[] RebornCost { get; }
        public int PlayerLeaderGid { get; private set; }

        public PveMap NowPlayMap { get; private set; }

        private bool NeedCheckClear { get; set; }

        private bool OnReset { get; set; } = false;
        private Random Random { get; }


   
        public static (RogueGame genByConfig, ImmutableHashSet<IGameResp> gameResps) GenByConfig(
            HashSet<CharacterInitData> characterBodies,
            int leaderId)
        {
            var gameRespS = new HashSet<IGameResp>();
            var genByConfig = new RogueGame(characterBodies, leaderId,
                out var ySlotArray);
            var pushChapterGoNext = new PushChapterGoNext(ySlotArray);
            gameRespS.Add(pushChapterGoNext);
            return (genByConfig, gameRespS.ToImmutableHashSet());
        }

        public ImmutableHashSet<IGameResp> GenLoadRespS()
        {
            var chapterIdsCount = ChapterIds.Count;
            return NowChapter.GenLoadChapterMsg(chapterIdsCount);
        }

        public RogueGameSave Save()
        {
            var rogueGameSave = RogueGameSave.Save(this);
            return rogueGameSave;
        }

        //for load game
        public RogueGame(HashSet<CharacterInitData> characterBodies, int playerLeader, ChapterSave chapterSave,
            Dictionary<int, PlayerStatusSave> playerSaves, int restChapterNum, int nowMap)
        {
            CharacterInitDataS = characterBodies;
            ChapterIds = LoadChapterIds(restChapterNum);
            Random = new Random();
            RebornCost = RogueLocalConfig.RogueRebornCost;
            var genByConfig = chapterSave.Load(Random);
            NowChapter = genByConfig;
            NowGamePlayers = playerSaves.ToDictionary(x => x.Key,
                x => new RogueGamePlayer(x.Value.LoadBody(x.Key)));
            RebornCountDownTick = RogueLocalConfig.RogueRebornTick;
            ChapterCountDownTick = -1;
            PlayerLeaderGid = playerLeader;
            NowPlayMap = NowChapter.MGidToMap[nowMap];
            BotTeam = new BotTeam();
            NowPlayMap.AddCharacterBodiesToStart(NowGamePlayers.Values.Select(x => x.Player));
            NowPlayMap.PlayGround.ActiveApplyDevice();
            NeedCheckClear = true;
            Pause = false;
        }

        private RogueGame(HashSet<CharacterInitData> characterBodies, int playerLeader,
            out (int x, MapType MapType, int MGid)[][] ySlotArray)
        {
            CharacterInitDataS = characterBodies;
            var enumerable = CharacterInitDataS.Select(x => x.GenCharacterBody(TwoDPoint.Zero())).ToArray();
            ChapterIds = LoadChapterIds();
            Random = new Random();
            RebornCost = RogueLocalConfig.RogueRebornCost;
            if (ChapterIds == null) throw new Exception("no ChapterIds");
            var (genByConfig, valueTuples) = Chapter.GenChapterById(ChapterIds.Dequeue(), Random);
            NowChapter = genByConfig;
            ySlotArray = valueTuples;
            NowGamePlayers = enumerable.ToDictionary(x => x.GetId(), x => new RogueGamePlayer(x));
            RebornCountDownTick = RogueLocalConfig.RogueRebornTick;
            ChapterCountDownTick = -1;
            PlayerLeaderGid = playerLeader;
            NowPlayMap = NowChapter.Entrance;
            BotTeam = new BotTeam();
            NowChapter.Entrance.AddCharacterBodiesToStart(enumerable);
            NeedCheckClear = true;
            Pause = false;
        }

        private IGameResp ResetRogueGame()
        {
            Pause = true;
            BotTeam.ClearBot();
            NowPlayMap.TelePortOut();
            ChapterIds = LoadChapterIds();
            var (genByConfig, valueTuples) = Chapter.GenChapterById(ChapterIds.Dequeue(), Random);
            NowChapter = genByConfig;
            var enumerable = CharacterInitDataS.Select(x => x.GenCharacterBody(TwoDPoint.Zero())).ToArray();
            NowGamePlayers = enumerable.ToDictionary(x => x.GetId(), x => new RogueGamePlayer(x));
            RebornCountDownTick = RogueLocalConfig.RogueRebornTick;
            ChapterCountDownTick = -1;
            NowPlayMap = NowChapter.Entrance;
            NowChapter.Entrance.AddCharacterBodiesToStart(enumerable);
            NeedCheckClear = true;
            var pushChapterGoNext = new PushChapterGoNext(valueTuples);
            Pause = false;
            OnReset = true;
            return pushChapterGoNext;
        }


        private Queue<int> LoadChapterIds(int rest = -1)
        {
            var otherConfig = CommonConfig.OtherConfig;
            var configRogueChapters = otherConfig.RogueChapters;
            var enumerable = configRogueChapters.Skip(configRogueChapters.Length - rest).ToArray();
            var otherConfigRogueChapters = rest < 0 ? configRogueChapters : enumerable;

            return
                otherConfigRogueChapters
                    .Aggregate(new Queue<int>(), (ints, i) =>
                    {
                        ints.Enqueue(i);
                        return ints;
                    });
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
            BotTeam.ClearBot();
            ChapterCountDownTick = -1;
            if (!ChapterIds.Any())
            {
                return new GameMsgPush(GamePushMsg.GameFinish);
            }

            NowPlayMap.TelePortOut();
            var dequeue = ChapterIds.Dequeue();
            var (genByConfig, ySlotArray) = Chapter.GenChapterById(dequeue, Random);
            NowChapter = genByConfig;
            var nowChapterEntrance = NowChapter.Entrance;
            NowPlayMap = nowChapterEntrance;
            var characterBodies = NowGamePlayers.Values.Select(x => x.Player).ToArray();
            NowPlayMap.AddCharacterBodiesToStart(characterBodies);
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


        private IGameResp DoGameRequest(int callSeat, IGameRequest gameRequest)
        {
            return gameRequest switch
            {
                ResetGame _ => callSeat == PlayerLeaderGid ? ResetRogueGame() : new QuestFail(callSeat),
                Pause _ => PauseOrUnPauseGame(callSeat, gameRequest),
#if DEBUG
                SkipChapter _ => GoNextChapter(),
                ForcePassChapter _ => NowChapter.SetPass(callSeat, gameRequest),
#endif
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

        private IGameResp PauseOrUnPauseGame(int callSeat, IGameRequest gameRequest)
        {
            Pause = !Pause;

            var questOkResult = new QuestOkResult(callSeat, gameRequest);
            return questOkResult;
        }


        // rogue游戏控制台包含核心玩法外的规则，应该与玩法异步运行
        public ImmutableHashSet<IGameResp> GameConsoleGoATick(Dictionary<int, IGameRequest> gameRequests)
        {
            var gameRespSet = new HashSet<IGameResp>();
            var enumerable = gameRequests
                .Select(
                    x => DoGameRequest(x.Key, x.Value)).ToArray();
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
                if (!ChapterIds.Any())
                {
                    var gameConsoleGoATick = new GameMsgPush(GamePushMsg.GameFinish);
                    gameRespSet.Add(gameConsoleGoATick);
                    return gameRespSet.ToImmutableHashSet();
                }

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
            if (Pause)
            {
                return RogueGameGoTickResult.Empty;
            }

            if (OnReset)
            {
                OnReset = false;
                return RogueGameGoTickResult.Empty2;
            }

            foreach (var botTeamTempOpThink in BotTeam.TempOperate.Where(botTeamTempOpThink =>
                botTeamTempOpThink.Value!= null))
            {
                opDic[botTeamTempOpThink.Key] = botTeamTempOpThink.Value!;
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

            var playerTeleportTo = playGroundGoTickResult.ActOutPut;
            if (!playerTeleportTo.Any()) return new RogueGameGoTickResult(playGroundGoTickResult, false);
            var toOutPutResults = playerTeleportTo.Values.First();
            var telePortMsgS = toOutPutResults.OfType<TelePortMsg>().ToArray();
            if (!telePortMsgS.Any())
            {
                return new RogueGameGoTickResult(playGroundGoTickResult, false);
            }

            var (map, toPos) = telePortMsgS.First();
            if (NowPlayMap.PlayGround.MgId == map) return new RogueGameGoTickResult(playGroundGoTickResult, false);
#if DEBUG
            Console.Out.WriteLine($"map change ~~~~{map} ~~ {toPos}");
#endif


            NowPlayMap.TelePortOut();
            NowPlayMap = NowChapter.MGidToMap[map];
            NowPlayMap.TeleportToThisMap(NowGamePlayers.Values.Select(x => (x.Player, toPos)));
            NowPlayMap.IsReached = true;
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
        public static RogueGameGoTickResult Empty = new RogueGameGoTickResult(PlayGroundGoTickResult.Empty, false);
        public static RogueGameGoTickResult Empty2 = new RogueGameGoTickResult(PlayGroundGoTickResult.Empty, true);

        public RogueGameGoTickResult(PlayGroundGoTickResult playGroundGoTickResult, bool mapChange)
        {
            PlayGroundGoTickResult = playGroundGoTickResult;
            MapChange = mapChange;
        }

        public PlayGroundGoTickResult PlayGroundGoTickResult { get; }

        public bool MapChange { get; }
    }

    public enum GameRequestType
    {
        KickPlayer,
        Leave,
#if DEBUG
        ForcePassChapter,
#endif
        RebornPlayer
    }
}