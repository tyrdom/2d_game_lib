using System.Collections.Generic;

namespace game_stuff
{
    public class CharKillScoreRuleData : ICharRuleData
    {
        public List<CharacterBody> NowKills { get; }
        private int KillScore { get; set; }

      

        public CharKillScoreRuleData()
        {
            NowKills = new List<CharacterBody>();
            KillScore = 0;
           
        }


        public void AddAKill(CharacterBody characterBody)
        {
            NowKills.Add(characterBody);
            KillScore++;
           
        }

        public int GetScore()
        {
            return KillScore;
        }

        public void ClearTemp()
        {
            NowKills.Clear();
            
        }
    }
}