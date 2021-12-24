using System;
using game_stuff;

namespace game_bot
{
    public class BehaviorTreeBot
    {
        private IBehaviorTreeNode StartNode { get; }

        public BotMemory BotMemory { get; }


        // public IAgentStatus AgentStatus()
        // {
        //     
        // }
        //
        // public Operate? GoATick()
        // {
        //     StartNode.DoNode()
        // }
    }

    public class BotAgent : IAgentStatus
    {
    }

    public struct BotMemory
    {
        private PatrolCtrl PatrolCtrl { get; }
        private ComboCtrl ComboCtrl { get; }
        private FirstSkillCtrl FirstSkillCtrl { get; }
        private CharacterBody CharacterBody { get; }
    }
}