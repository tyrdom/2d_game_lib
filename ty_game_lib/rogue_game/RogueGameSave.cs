using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_stuff;


namespace rogue_game
{
    [Serializable]
    public class RogueGameSave
    {
        private Dictionary<int, PlayerStatusSave> PlayerSaves { get; }

        private ChapterSave NowChapterSave { get; }

        private int RestChapterNum { get; }

        private CharacterInitData[] CharacterInitDataS { get; }

        private int InMapGid { get; }

        public RogueGameSave(Dictionary<int, PlayerStatusSave> playerSaves, ChapterSave nowChapterSave,
            int restChapterNum, CharacterInitData[] characterInitDataS, int inMapGid)
        {
            PlayerSaves = playerSaves;
            NowChapterSave = nowChapterSave;
            RestChapterNum = restChapterNum;
            CharacterInitDataS = characterInitDataS;
            InMapGid = inMapGid;
        }

        public static RogueGameSave Save(RogueGame rogueGame)
        {
            var playerStatusSaves = rogueGame.NowGamePlayers
                .ToDictionary(x => x.Key,
                    x => x.Value.PlayerSave());
            var chapterSave = ChapterSave.Save(rogueGame.NowChapter);
            var rogueGameCharacterInitDataS = rogueGame.CharacterInitDataS.ToArray();
            var chapterIdsCount = rogueGame.ChapterIds.Count;
            var playGroundMgId = rogueGame.NowPlayMap.PlayGround.MgId;
            var rogueGameSave = new RogueGameSave(playerStatusSaves, chapterSave, chapterIdsCount,
                rogueGameCharacterInitDataS, playGroundMgId);
            return rogueGameSave;
        }

        public RogueGame Load(int leaderId)
        {
            var rogueGame = new RogueGame(CharacterInitDataS.IeToHashSet(), leaderId, NowChapterSave, PlayerSaves,
                RestChapterNum, InMapGid);
            return rogueGame;
        }
    }
}