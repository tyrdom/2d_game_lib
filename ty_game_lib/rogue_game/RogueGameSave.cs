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

        public RogueGameSave(Dictionary<int, PlayerStatusSave> playerSaves, ChapterSave nowChapterSave,
            int restChapterNum, CharacterInitData[] characterInitDataS)
        {
            PlayerSaves = playerSaves;
            NowChapterSave = nowChapterSave;
            RestChapterNum = restChapterNum;
            CharacterInitDataS = characterInitDataS;
        }

        public static RogueGameSave Save(RogueGame rogueGame)
        {
            var playerStatusSaves = rogueGame.NowGamePlayers
                .ToDictionary(x => x.Key,
                    x => x.Value.PlayerSave());
            var chapterSave = ChapterSave.Save(rogueGame.NowChapter);
            var rogueGameCharacterInitDataS = rogueGame.CharacterInitDataS.ToArray();
            var chapterIdsCount = rogueGame.ChapterIds.Count;
            var rogueGameSave = new RogueGameSave(playerStatusSaves, chapterSave, chapterIdsCount,
                rogueGameCharacterInitDataS);
            return rogueGameSave;
        }

        public RogueGame Load(int leaderId)
        {
            var rogueGame = new RogueGame(CharacterInitDataS.IeToHashSet(), leaderId, NowChapterSave, PlayerSaves,
                RestChapterNum);
            return rogueGame;
        }
    }
}