using System;
using game_stuff;

namespace game_bot
{
    public enum BehaviorTreeBranchType
    {
        Select,
        Sequence
    }

    public enum NodeResult
    {
        Success,
        Fail,
        Acting
    }

    public class ActingOperation
    {
        public IBehaviorTreeNode OnNode { get; }

        public Operate Operate { get; }
    }

    public interface IBehaviorTreeNode
    {
        public NodeResult DoNode(IAgentStatus agentStatus, out ActingOperation? operate);
    }

    public class BehaviorTreeBranch : IBehaviorTreeNode
    {
        public BehaviorTreeBranchType BehaviorTreeBranchType { get; }

        public IBehaviorTreeNode[] BehaviorTreeNodes { get; }

        public NodeResult DoNode(IAgentStatus agentStatus, out ActingOperation? operate)
        {
            switch (BehaviorTreeBranchType)
            {
                case BehaviorTreeBranchType.Select:
                    foreach (var behaviorTreeNode in BehaviorTreeNodes)
                    {
                        var nodeResult = behaviorTreeNode.DoNode(agentStatus, out operate);
                        switch (nodeResult)
                        {
                            case NodeResult.Success:
                                operate = null;
                                return NodeResult.Success;
                            case NodeResult.Fail:
                                operate = null;
                                continue;
                            case NodeResult.Acting:

                                return NodeResult.Acting;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    operate = null;
                    return NodeResult.Fail;

                case BehaviorTreeBranchType.Sequence:
                    foreach (var behaviorTreeNode in BehaviorTreeNodes)
                    {
                        switch (behaviorTreeNode.DoNode(agentStatus, out operate))
                        {
                            case NodeResult.Success:
                                operate = null;
                                continue;
                            case NodeResult.Fail:
                                operate = null;
                                return NodeResult.Fail;
                            case NodeResult.Acting:
                                return NodeResult.Acting;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    operate = null;
                    return NodeResult.Success;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public interface IBehaviorTreeLeaf : IBehaviorTreeNode
    {
    }

    public static class BehaviorTreeHelper
    {
        // public static void GoATickAndIsReturnRoot(this IBehaviorTreeNode behaviorTreeNode, IAgentStatus agentStatus,
        //     out ActingOperation? actingOperation)
        // {
        //     var nodeResult = behaviorTreeNode.DoNode(agentStatus, out actingOperation);
        // }
    }

    public interface IAgentStatus
    {
    }
}