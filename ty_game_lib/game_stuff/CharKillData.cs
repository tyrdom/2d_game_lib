using System.Collections.Generic;

namespace game_stuff
{
    public class CharKillData : ICharRuleData
    {
        private int KillScore { get; set; }
        public CharKillData()
        {
            KillScore = 0;
        }
        public void AddAKill()
        {
            KillScore++;
        }
        public int GetScore()
        {
            return KillScore;
        }

        public void ClearTemp()
        {
        }
    }
}