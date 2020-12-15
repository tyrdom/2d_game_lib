using System.Collections.Generic;

namespace game_stuff
{
    public class CharKillScoreRuleData : ICharRuleData
    {
        public List<CharacterBody> NowKills { get; }

        public CharKillScoreRuleData()
        {
            NowKills = new List<CharacterBody>();
            KillScore = 0;
        }

        private int KillScore { get; set; }

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