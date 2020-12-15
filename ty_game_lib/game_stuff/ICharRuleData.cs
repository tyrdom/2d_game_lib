using System.Collections.Generic;

namespace game_stuff
{
    public interface ICharRuleData
    {
        public List<CharacterBody> NowKills { get; }
        void ClearTemp();
        void AddAKill(CharacterBody characterBody);
        int GetScore();
    }
}