using System;
using System.Collections.Generic;
using game_stuff;

namespace game_bot
{
    public class LocalBehaviorTreeBot
    {
        private IBehaviorTreeNode StartNode { get; }


        public PatrolCtrl PatrolCtrl { get; }
        public ComboCtrl ComboCtrl { get; }
        public FirstSkillCtrl FirstSkillCtrl { get; }
        private CharacterBody BotBody { get; }

        private ICanBeAndNeedHit? LockBody { get; set; }

        private int NowLockTraceTick { get; set; }

        public IAgentStatus[] GenAgentStatus()
        {
            var agentStatusList = new List<IAgentStatus>();
            var characterBodyCharacterStatus = BotBody.CharacterStatus;
            var b = characterBodyCharacterStatus.StunBuff == null;
            if (b)
            {
                agentStatusList.Add(new Stun());
                
                return agentStatusList.ToArray();
            }

            var nowCastAct = characterBodyCharacterStatus.NowCastAct;
            var b1 = nowCastAct != null && nowCastAct.InWhichPeriod() == SkillPeriod.Casting;
            if (b1)
            {
                agentStatusList.Add(new BotActing());
                
                return agentStatusList.ToArray();
            }

            
            
            return agentStatusList.ToArray();
        }
        //
        // public Operate? GoATick()
        // {
        //     StartNode.DoNode()
        // }
    }

    public class Stun : IAgentStatus
    {
    }

    public class BotActing : IAgentStatus
    {
    }

    public struct BotMemory
    {
    }
}