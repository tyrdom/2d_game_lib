namespace game_bot
{
    public static class NormalBotBehaviorTree
    {
        private static BehaviorTreeCondLeaf IsStun { get; } = new(BehaviorTreeFunc.IsStunFunc);

        private static BehaviorTreeCondLeaf IsActing { get; } = new(BehaviorTreeFunc.InActingFunc);

        private static BehaviorTreeCondLeaf IsDead { get; } = new(BehaviorTreeFunc.IsDeadFunc);

        private static BehaviorTreeCondLeaf IsHitSth { get; } = new(BehaviorTreeFunc.CheckHitSth);

        private static BehaviorTreeActLeaf SetCombo { get; } = new(BehaviorTreeFunc.SetComboStatus);

        private static BehaviorTreeActLeaf CanUseWeaponAct { get; } = new(BehaviorTreeFunc.CanUseWeaponToTarget);


        private static BehaviorTreeActLeaf ApproachingAct { get; } = new(BehaviorTreeFunc.TargetApproach);

        private static BehaviorTreeActLeaf TraceAct { get; } = new(BehaviorTreeFunc.Trace);

        private static BehaviorTreeSequenceBranch ComboSetBranch { get; } = new(new AlwaysDecorator(true),
            new IBehaviorTreeNode[]
            {
                IsHitSth, SetCombo
            });

        private static BehaviorTreeSequenceBranch ActingBranch { get; } =
            new(new IBehaviorTreeNode[]
            {
                IsActing, ComboSetBranch
            });


        private static BehaviorTreeSelectBranch OpForbidden { get; }
            = new(
                new IBehaviorTreeNode[]
                {
                    IsStun, IsDead, ActingBranch
                });

        private static BehaviorTreeActLeaf
            GetCombo { get; } = new(BehaviorTreeFunc.GetComboAct);

        private static BehaviorTreeSelectBranch OpActs { get; }
            = new(
                new IBehaviorTreeNode[]
                {
                    GetCombo, CanUseWeaponAct, ApproachingAct, TraceAct,
                });


        public static BehaviorTreeSelectBranch Root { get; } = new(
            new IBehaviorTreeNode[]
            {
                OpForbidden, OpActs
            }
        );
    }
}