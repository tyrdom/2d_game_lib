using game_stuff;

namespace game_bot
{
    public static class NormalBotBehaviorTree
    {
        public static BehaviorTreeSelectBranch Root { get; }

        

        public static (bool result, Operate? operate) BattleFunc(
            BotAgent botAgent)
        {
            return (false, null);
        }
    }
}