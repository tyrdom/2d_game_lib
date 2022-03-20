namespace game_bot
{
    public static class NormalBotBehaviorTree
    {
        private static BehaviorTreeCondLeaf IsStun { get; } = new BehaviorTreeCondLeaf(BehaviorTreeFunc.IsStunFunc);

        private static BehaviorTreeCondLeaf IsActing { get; } = new BehaviorTreeCondLeaf(BehaviorTreeFunc.InActingFunc);

        private static BehaviorTreeCondLeaf IsDead { get; } = new BehaviorTreeCondLeaf(BehaviorTreeFunc.IsDeadFunc);

        private static BehaviorTreeCondLeaf IsHitSth { get; } = new BehaviorTreeCondLeaf(BehaviorTreeFunc.CheckHitSth);

        private static BehaviorTreeActLeaf SetCombo { get; } = new BehaviorTreeActLeaf(BehaviorTreeFunc.SetComboStatus);

        private static BehaviorTreeActLeaf CanUseWeaponAct { get; } =
            new BehaviorTreeActLeaf(BehaviorTreeFunc.CanUseWeaponToTarget, "UseWeapon");


        private static BehaviorTreeActLeaf TracePt { get; } =
            new BehaviorTreeActLeaf(BehaviorTreeFunc.TracePt, "tracePt");

        private static BehaviorTreeActLeaf TraceAim { get; } =
            new BehaviorTreeActLeaf(BehaviorTreeFunc.TraceAim, "traceAim");

        private static BehaviorTreeSelectBranch TraceAct { get; } = new BehaviorTreeSelectBranch(new IBehaviorTreeNode[]
        {
            TracePt, TraceAim
        });

        private static BehaviorTreeActLeaf UsePropAct { get; } =
            new BehaviorTreeActLeaf(BehaviorTreeFunc.PropUse, "UseProp");

        private static BehaviorTreeSequenceBranch ComboSetBranch { get; } = new BehaviorTreeSequenceBranch(
            new AlwaysDecorator(true),
            new IBehaviorTreeNode[]
            {
                IsHitSth, SetCombo
            }, "Combo");

        private static BehaviorTreeSequenceBranch IsActingBranch { get; } =
            new BehaviorTreeSequenceBranch(new IBehaviorTreeNode[]
                {
                    IsActing, ComboSetBranch
                }
                , "IsActing"
            );


        private static BehaviorTreeSelectBranch OpForbidden { get; }
            = new BehaviorTreeSelectBranch(
                new IBehaviorTreeNode[]
                {
                    IsStun, IsDead, IsActingBranch
                }, "OpForbidden");

        private static BehaviorTreeActLeaf
            GetCombo { get; } = new BehaviorTreeActLeaf(BehaviorTreeFunc.GetComboAct, "GetCombo");

        private static BehaviorTreeSelectBranch OpActs { get; }
            = new BehaviorTreeSelectBranch(
                new IBehaviorTreeNode[]
                {
                    GetCombo, CanUseWeaponAct, UsePropAct, TraceAct
                });


        public static BehaviorTreeSelectBranch Root { get; } = new BehaviorTreeSelectBranch(
            new IBehaviorTreeNode[]
            {
                OpForbidden, OpActs
            }
        );
    }
}