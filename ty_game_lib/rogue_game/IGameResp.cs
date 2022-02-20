using System;
using rogue_chapter_maker;

namespace rogue_game
{
    public interface IGameResp
    {
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
                KickPlayer _ => GameRequestType.KickPlayer,
                Leave _ => GameRequestType.Leave,
                RebornPlayer _ => GameRequestType.RebornPlayer,
#if DEBUG
                ForcePassChapter _ => GameRequestType.ForcePassChapter,
#endif
                _ => throw new ArgumentOutOfRangeException(nameof(gameRequest))
            };

            GameRequestType = gameRequestType;
        }

        public GameRequestType GameRequestType { get; }

        public int PlayerGid { get; }
    }

    public class PushChapterGoNext : IGameResp
    {
        public PushChapterGoNext(YSlot[][] ySlotArray)
        {
            YSlotArray = ySlotArray;
        }

        public YSlot[][] YSlotArray { get; }
    }

    public class InitChapter : IGameResp
    {
        public InitChapter(int restChapterIds, int[] reachedMap)
        {
            RestChapterIds = restChapterIds;
            ReachedMap = reachedMap;
        }

        public int RestChapterIds { get; }
        public int[] ReachedMap { get; }
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
        GameFinish
    }
}