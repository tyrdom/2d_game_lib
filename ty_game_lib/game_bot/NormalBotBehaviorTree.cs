using game_stuff;

namespace game_bot
{
    public static class NormalBotBehaviorTree
    {
        public static BehaviorTreeSelectBranch Root { get; } = new BehaviorTreeSelectBranch(
            new[]
            {
                OpForbidden,OpActs
            }
        );

        public static BehaviorTreeSelectBranch OpForbidden { get; }
        
        public static BehaviorTreeSelectBranch OpActs { get; }

        public static (bool result, Operate? operate) BattleFunc(
            BotAgent botAgent)
        {
            return (false, null);
        }
    }
}