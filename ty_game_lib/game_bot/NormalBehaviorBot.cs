namespace game_bot
{
    public static class NormalBehaviorBot
    {
        public static BehaviorTreeCondLeaf IsStun { get; } = new(BehaviorTreeFunc.IsStunFunc);

        public static BehaviorTreeCondLeaf IsActing { get; } = new(BehaviorTreeFunc.InActingFunc);

        public static BehaviorTreeCondLeaf IsDead { get; } = new(BehaviorTreeFunc.IsDeadFunc);

        public static BehaviorTreeCondLeaf IsHitSth { get; } = new(BehaviorTreeFunc.CheckHitSth);

        public static BehaviorTreeActLeaf SetCombo { get; } = new(BehaviorTreeFunc.SetComboStatus);

        public static BehaviorTreeActLeaf CanUseWeaponAct { get; } = new(BehaviorTreeFunc.CanUseWeaponToTarget);


        public static BehaviorTreeActLeaf ApproachingAct { get; } = new(BehaviorTreeFunc.TargetApproach);

        public static BehaviorTreeActLeaf TraceAct { get; } = new(BehaviorTreeFunc.Trace);

        public static BehaviorTreeSequenceBranch ComboSetBranch { get; } = new(new AlwaysDecorator(true),
            new IBehaviorTreeNode[]
            {
                IsHitSth, SetCombo
            });

        public static BehaviorTreeSequenceBranch ActingBranch { get; } =
            new(new IBehaviorTreeNode[]
            {
                IsActing, ComboSetBranch
            });


        public static BehaviorTreeSelectBranch OpForbidden { get; }
            = new(
                new IBehaviorTreeNode[]
                {
                    IsStun, IsDead, ActingBranch
                });

        public static BehaviorTreeActLeaf
            GetCombo { get; } = new(BehaviorTreeFunc.GetComboAct);

        public static BehaviorTreeSelectBranch OpActs { get; }
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