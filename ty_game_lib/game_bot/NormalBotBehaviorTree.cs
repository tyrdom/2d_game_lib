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

        private static BehaviorTreeActLeaf TracePt { get; } = new(BehaviorTreeFunc.TracePt);

        private static BehaviorTreeActLeaf TraceAim { get; } = new(BehaviorTreeFunc.TraceAim);

        private static BehaviorTreeSelectBranch TraceAct { get; } = new(new IBehaviorTreeNode[]
        {
            TracePt, TraceAim
        });

        private static BehaviorTreeActLeaf UsePropAct { get; } = new(BehaviorTreeFunc.PropUse);

        private static BehaviorTreeSequenceBranch ComboSetBranch { get; } = new(new AlwaysDecorator(true),
            new IBehaviorTreeNode[]
            {
                IsHitSth, SetCombo
            });

        private static BehaviorTreeSequenceBranch IsActingBranch { get; } =
            new(new IBehaviorTreeNode[]
            {
                IsActing, ComboSetBranch
            });


        private static BehaviorTreeSelectBranch OpForbidden { get; }
            = new(
                new IBehaviorTreeNode[]
                {
                    IsStun, IsDead, IsActingBranch
                });

        private static BehaviorTreeActLeaf
            GetCombo { get; } = new(BehaviorTreeFunc.GetComboAct);

        private static BehaviorTreeSelectBranch OpActs { get; }
            = new(
                new IBehaviorTreeNode[]
                {
                    GetCombo, CanUseWeaponAct, UsePropAct, TraceAct
                });


        public static BehaviorTreeSelectBranch Root { get; } = new(
            new IBehaviorTreeNode[]
            {
                OpForbidden, OpActs
            }
        );
    }
}