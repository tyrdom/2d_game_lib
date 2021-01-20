using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using collision_and_rigid;
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

        public bool DoGameRequest(int callSeat, IGameRequest gameRequest)
        {
            return gameRequest switch
            {
                KickPlayer kickPlayer => callSeat == PlayerLeaderSeat && LeaveGame(kickPlayer.Seat),
                Leave _ => LeaveGame(callSeat),
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

        public void GoATick()
        {
        }

        public PlayGroundGoTickResult GamePlayGoATick(Dictionary<int, Operate> opDic)
        {
            var groupBy = NowGamePlayers.Values.GroupBy(x => x.InPlayGround);

            var playGroundGoTickResults = groupBy.Select(rp =>
            {
                var valuePairs = opDic.Where(x => rp.Select(r => r.Player.GetId()).Contains(x.Key))
                    .ToDictionary(p => p.Key, p => p.Value);
                if (!rp.Key.IsClearAndSave()) return rp.Key.PlayGroundGoATick(valuePairs);
                foreach (var rogueGamePlayer in rp)
                {
                    rogueGamePlayer.PlayerGoSave();
                }

                return rp.Key.PlayGroundGoATick(valuePairs);
            });
            var playGroundGoTickResult = PlayGroundGoTickResult.Sum(playGroundGoTickResults);
            var (_, _, _, teleportTo)
                = playGroundGoTickResult;
            foreach (var kv in teleportTo)
            {
                if (NowGamePlayers.TryGetValue(kv.Key, out var gamePlayer) &&
                    NowChapter.MGidToMap.TryGetValue(kv.Value, out var pveMap))
                {
                    gamePlayer.Teleport(pveMap);
                }

                throw new DirectoryNotFoundException();
            }

            return playGroundGoTickResult;
        }
    }
}