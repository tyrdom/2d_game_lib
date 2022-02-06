using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using collision_and_rigid;
using cov_path_navi;
using game_config;
using game_stuff;

namespace game_bot
{
    public class NormalBehaviorBot
    {
        public LocalBehaviorTreeBotAgent LocalBehaviorTreeBotAgent { get; }

        public BehaviorTreeSelectBranch Root { get; }

        private IBehaviorTreeNode? Next { get; set; }

        private NormalBehaviorBot(LocalBehaviorTreeBotAgent localBehaviorTreeBotAgent, BehaviorTreeSelectBranch root)
        {
            LocalBehaviorTreeBotAgent = localBehaviorTreeBotAgent;
            Root = root;
            Next = root;
        }

        public static NormalBehaviorBot GenById(int botId, CharacterBody body, PathTop pathTop)
        {
            return CommonConfig.Configs.battle_bots.TryGetValue(botId, out var battleNpc)
                ? GenByConfig(battleNpc, body, pathTop)
                : throw new KeyNotFoundException($"not found id :: {botId}");
        }

        private static NormalBehaviorBot GenByConfig(battle_bot battleNpc, CharacterBody body,
            PathTop pathTop)
        {
            var localBehaviorTreeBotAgent = LocalBehaviorTreeBotAgent.GenByConfig(battleNpc, body, pathTop);
            var behaviorTreeSelectBranch = NormalBotBehaviorTree.Root;
            var normalBehaviorBot = new NormalBehaviorBot(localBehaviorTreeBotAgent, behaviorTreeSelectBranch);
            return normalBehaviorBot;
        }

        public TwoDPoint? GetStartPt()
        {
            var ptNum = LocalBehaviorTreeBotAgent.PatrolCtrl.GetPtNum();
            var next =
                ptNum <= 0
                    ? null
                    : LocalBehaviorTreeBotAgent.PatrolCtrl.GetPt(
                        BehaviorTreeFunc.Random.Next(ptNum));
            return next;
        }

        public Operate? GoATick(PlayerTickSense perceivable,
            ImmutableHashSet<IHitMsg> immutableHashSet, PathTop? pathTop)
        {
            var agentStatusArray = LocalBehaviorTreeBotAgent.GenAgentStatus(perceivable, immutableHashSet, pathTop);
            var behaviorTreeNode = Next ?? Root;
            var (_, nextLoopStart) = behaviorTreeNode.DoNodeWithDecorator(agentStatusArray, out var operate);
            var b = nextLoopStart == null;
            Next = b ? Root : nextLoopStart;
            return operate;
        }
    }
}