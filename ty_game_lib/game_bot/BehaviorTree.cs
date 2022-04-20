using System;
using game_stuff;

namespace game_bot
{
    public interface IBehaviorTreeNode
    {
        public string Name { get; }
        public IDecorator Decorator { get; }

        public (bool Result, IBehaviorTreeNode? NextLoopStart) DoNodeWithDecorator(IAgentStatus[] agentStatus,
            out Operate? operate);
    }

    public class BehaviorTreeSequenceBranch : IBehaviorTreeNode
    {
        public string Name { get; }
        public IDecorator Decorator { get; }
        public IBehaviorTreeNode[] BehaviorTreeNodes { get; }

        public BehaviorTreeSequenceBranch(IDecorator decorator, IBehaviorTreeNode[] behaviorTreeNodes,
            string name = "SeqB")
        {
            Name = name;
            Decorator = decorator;
            BehaviorTreeNodes = behaviorTreeNodes;
        }

        public BehaviorTreeSequenceBranch(IBehaviorTreeNode[] behaviorTreeNodes, string name = "SeqB")
        {
            Name = name;
            Decorator = new NoneDecorator();
            BehaviorTreeNodes = behaviorTreeNodes;
        }

        public (bool Result, IBehaviorTreeNode? NextLoopStart) DoNodeWithDecorator(IAgentStatus[] agentStatus,
            out Operate? operate)
        {
#if DEBUG
            Console.Out.WriteLine($"OnNode : {Name}");
#endif

            operate = null;
            foreach (var behaviorTreeNode in BehaviorTreeNodes)
            {
                var (result, nodeResultNextLoopStart) =
                    behaviorTreeNode.DoNodeWithDecorator(agentStatus, out operate);
                if (nodeResultNextLoopStart != null)
                {
                    return (result, nodeResultNextLoopStart);
                }

                var useDecorator = Decorator.UseDecorator(result, out var nextLoopThisStart);
                if (nextLoopThisStart)
                {
                    return (useDecorator, this);
                }

                switch (result)
                {
                    case false:
                        return (useDecorator, null);
                    case true:
                        continue;
                }
            }

            return (true, null);
        }
    }

    public class BehaviorTreeSelectBranch : IBehaviorTreeNode
    {
        public string Name { get; }
        public IDecorator Decorator { get; }

        public BehaviorTreeSelectBranch(IBehaviorTreeNode[] behaviorTreeNodes, string name = "SelB")
        {
            Name = name;
            BehaviorTreeNodes = behaviorTreeNodes;
            Decorator = new NoneDecorator();
        }

        public BehaviorTreeSelectBranch(IBehaviorTreeNode[] behaviorTreeNodes,
            IDecorator decorator, string name = "SelB")
        {
            Name = name;
            BehaviorTreeNodes = behaviorTreeNodes;
            Decorator = decorator;
        }


        public IBehaviorTreeNode[] BehaviorTreeNodes { get; }

        public (bool Result, IBehaviorTreeNode? NextLoopStart) DoNodeWithDecorator(IAgentStatus[] agentStatus,
            out Operate? operate)
        {
#if DEBUG
            Console.Out.WriteLine($"OnNode : {Name}");
#endif

            operate = null;
            foreach (var behaviorTreeNode in BehaviorTreeNodes)
            {
                var (result, nodeResultNextLoopStart) =
                    behaviorTreeNode.DoNodeWithDecorator(agentStatus, out operate);
                if (nodeResultNextLoopStart != null)
                {
                    return (result, nodeResultNextLoopStart);
                }

                var useDecorator = Decorator.UseDecorator(result, out var nextLoopThisStart);
                if (nextLoopThisStart)
                {
                    return (useDecorator, this);
                }

                switch (result)
                {
                    case true:
                        return (useDecorator, null);
                    case false:
                        continue;
                }
            }

            return (false, null);
        }
    }

    public class AlwaysDecorator : IDecorator
    {
        public AlwaysDecorator(bool alwaysSet)
        {
            AlwaysSet = alwaysSet;
        }

        private bool AlwaysSet { get; }

        public bool UseDecorator(bool nodeResult, out bool nextLoopThisStart)
        {
            nextLoopThisStart = false;
            return AlwaysSet;
        }
    }

    public interface IDecorator
    {
        bool UseDecorator(bool nodeResult, out bool nextLoopThisStart);
    }

    public class NotDecorator : IDecorator
    {
        public bool UseDecorator(bool nodeResult, out bool nextLoopThisStart)
        {
            nextLoopThisStart = false;
            return !nodeResult;
        }
    }

    public class NoneDecorator : IDecorator
    {
        public bool UseDecorator(bool nodeResult, out bool nextLoopThisStart)
        {
            nextLoopThisStart = false;
            return nodeResult;
        }
    }


    public class LoopUntil : IDecorator
    {
        public LoopUntil(bool untilSet)
        {
            UntilSet = untilSet;
        }

        public bool UntilSet { get; }

        public bool UseDecorator(bool nodeResult, out bool nextLoopThisStart)
        {
            if (nodeResult == UntilSet)
            {
                nextLoopThisStart = false;
                return nodeResult;
            }

            nextLoopThisStart = true;
            return nodeResult;
        }
    }

    public interface IBehaviorTreeLeaf : IBehaviorTreeNode
    {
    }

    public class BehaviorTreeCondLeaf : IBehaviorTreeLeaf
    {
        public BehaviorTreeCondLeaf(Func<IAgentStatus[], bool> func, IDecorator decorator, string name = "CondL")
        {
            Name = name;
            Func = func;
            Decorator = decorator;
        }

        public BehaviorTreeCondLeaf(Func<IAgentStatus[], bool> func, string name = "CondL")
        {
            Name = name;
            Func = func;
            Decorator = new NoneDecorator();
        }

        private Func<IAgentStatus[], bool> Func { get; }


        public (bool Result, IBehaviorTreeNode? NextLoopStart) DoNodeWithDecorator(IAgentStatus[] agentStatus,
            out Operate? operate)
        {
#if DEBUG
            Console.Out.WriteLine($"OnNode : {Name}");
#endif
            operate = null;
            var func = Func(agentStatus);
            var useDecorator = Decorator.UseDecorator(func, out var nextLoopThisStart);
            return nextLoopThisStart ? (useDecorator, this) : (useDecorator, null);
        }

        public string Name { get; }
        public IDecorator Decorator { get; }
    }

    public class BehaviorTreeActLeaf : IBehaviorTreeLeaf
    {
        public BehaviorTreeActLeaf(Func<IAgentStatus[], (bool, Operate?)> func, IDecorator decorator,
            string name = "ActL")
        {
            Name = name;
            Func = func;
            Decorator = decorator;
        }

        public BehaviorTreeActLeaf(Func<IAgentStatus[], (bool, Operate?)> func, string name = "ActL")
        {
            Name = name;
            Func = func;
            Decorator = new NoneDecorator();
        }

        private Func<IAgentStatus[], (bool, Operate?)> Func { get; }


        public (bool Result, IBehaviorTreeNode? NextLoopStart) DoNodeWithDecorator(IAgentStatus[] agentStatus,
            out Operate? operate)
        {
#if DEBUG
            Console.Out.WriteLine($"OnNode : {Name}");
#endif


            var (item1, item2) = Func(agentStatus);
            operate = item2;
            var useDecorator = Decorator.UseDecorator(item1, out var nextLoopThisStart);

            return nextLoopThisStart ? (useDecorator, this) : (useDecorator, null);
        }

        public string Name { get; }
        public IDecorator Decorator { get; }
    }
}