using System;
using System.Linq;
using System.Transactions;
using game_stuff;

namespace game_bot
{
    public static class NormalBotBehaviorTree
    {
        public static Random Random { get; } = new Random();


        public static (bool, Operate?) ComboAct(IAgentStatus[] botAgent)
        {
            return (false, null);
        }

        public static (bool, Operate?) CanUseWeapon(IAgentStatus[] botAgent)
        {
            var ofType = botAgent.OfType<TargetMsg>().ToArray();
            if (!ofType.Any())
            {
                return (false, null);
            }

            var targetMsg = ofType.FirstOrDefault();
            var bodyStatus = botAgent.OfType<BodyStatus>().FirstOrDefault();

            var twoDPoint = targetMsg.Target.GetAnchor();
            var bodyStatusCharacterBody = bodyStatus.CharacterBody;
            var dPoint = bodyStatusCharacterBody.GetAnchor();
            var distance = twoDPoint.GetDistance(dPoint);

            var valueTuples = bodyStatus.NowRangeToWeapon.Where(x =>
                x.range > distance && x.maxAmmoUse <= bodyStatusCharacterBody.CharacterStatus.GetAmmo()).ToArray();
            if (valueTuples.Any())
            {
                var characterStatusNowWeapon = bodyStatusCharacterBody.CharacterStatus.NowWeapon;
                var any = valueTuples.Any(x => x.weaponIndex == characterStatusNowWeapon);

                if (any)
                {
                    var botMemories = botAgent.OfType<BotMemory>().First();
                    var goATick = botMemories.FirstSkillCtrl.NoThinkAct(Random);
                    var operate = new Operate(skillAction: goATick);
                    return (true, operate);
                }
                else
                {
                    var operate = new Operate(skillAction: SkillAction.Switch);
                    return (true, operate);
                }
            }

            return (false, null);
        }

        public static bool ActingOrStunFunc(
            IAgentStatus[] botAgent)
        {
            var bodyStatus = botAgent.OfType<BodyStatus>().First();
            var characterBodyCharacterStatus = bodyStatus.CharacterBody.CharacterStatus;
            var b = characterBodyCharacterStatus.NowCastAct != null || characterBodyCharacterStatus.StunBuff != null;
            return b;
        }

        public static BehaviorTreeCondLeaf CantAct { get; } = new BehaviorTreeCondLeaf(ActingOrStunFunc);

        public static BehaviorTreeActLeaf CanUseWeaponAct { get; } = new BehaviorTreeActLeaf(CanUseWeapon);

        public static BehaviorTreeSelectBranch OpForbidden { get; }
            = new BehaviorTreeSelectBranch(
                new IBehaviorTreeNode[]
                {
                    CantAct
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

    public struct BotMemory : IAgentStatus
    {
        public PatrolCtrl PatrolCtrl { get; }
        public ComboCtrl ComboCtrl { get; }
        public FirstSkillCtrl FirstSkillCtrl { get; }
    }
}