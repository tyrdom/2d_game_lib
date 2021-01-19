using System.Collections.Generic;

namespace game_stuff
{
    public class CharKillData : IScoreData
    {
        private int KillScore { get; set; }

        public CharKillData()
        {
            KillScore = 0;
        }

        public void AddScore()
        {
            KillScore++;
        }

        public int GetScore()
        {
            return KillScore;
        }
    }
}