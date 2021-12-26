using System.Linq;
using System.Transactions;
using game_stuff;

namespace game_bot
{
    public static class NormalBotBehaviorTree
    {
        public static bool ActingFunc(
            IAgentStatus[] botAgent)
        {
            return botAgent.Any(x => x is BotActing);
        }

        public static BehaviorTreeCondLeaf Acting { get; } = new BehaviorTreeCondLeaf(ActingFunc);

        public static BehaviorTreeSelectBranch OpForbidden { get; }
            = new BehaviorTreeSelectBranch(
                new IBehaviorTreeNode[]
                {
                    Acting
                });

        public static BehaviorTreeSelectBranch OpActs { get; }
            = new BehaviorTreeSelectBranch(
                new IBehaviorTreeNode[]
                {
                });

        public static BehaviorTreeSelectBranch Root { get; } = new BehaviorTreeSelectBranch(
            new IBehaviorTreeNode[]
            {
                OpForbidden, OpActs
            }
        );
    }
}