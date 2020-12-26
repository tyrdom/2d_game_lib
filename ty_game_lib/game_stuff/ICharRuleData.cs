using System.Collections.Generic;

namespace game_stuff
{
    public interface ICharRuleData
    {
        void ClearTemp();
        void AddAKill();
        int GetScore();
    }
}