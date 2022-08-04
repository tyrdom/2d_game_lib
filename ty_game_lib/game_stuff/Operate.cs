using System;
using collision_and_rigid;


namespace game_stuff
{
    public class Operate
    {
        // public OpAction? Action;
        // public TwoDVector? Move;
        public TwoDVector? Aim { get; }

        public SkillAction? SkillAction { get; }

        public TwoDVector? Move { get; }

        public SnipeAction? SnipeAction { get; }

        public MapInteract? MapInteractive { get; }

        public SpecialAction? SpecialAction { get; }

        public Operate(TwoDVector? aim = null,
            SkillAction? skillAction = null,
            TwoDVector? move = null,
            SpecialAction? specialAction = null,
            MapInteract? mapInteractive = null,
            SnipeAction? snipeAction = null)
        {
            Aim = aim;
            SkillAction = skillAction;
            Move = move;
            SpecialAction = specialAction;
            MapInteractive = mapInteractive;
            SnipeAction = snipeAction;
        }

        public SpecialAction? GetSpecialAction()
        {
            return SpecialAction;
        }

        public MapInteract? GetMapInteractive()
        {
            if (SkillAction == null && SnipeAction == null)
            {
                return MapInteractive;
            }

            return null;
        }

        public SkillAction? GetAction()
        {
            var snipeAction = GetSnipe();
            var snipeToChargeOp = snipeAction != null ? SnipeToChargeOp(snipeAction.Value) : (SkillAction?) null;
            var toChargeOp = SkillAction ?? snipeToChargeOp;
            return toChargeOp;
        }

        public SnipeAction? GetSnipe()
        {
            return SnipeAction;
        }

        private static SkillAction SnipeToChargeOp(SnipeAction snipeAction)
        {
            return snipeAction switch
            {
                game_stuff.SnipeAction.SnipeOn1 => game_stuff.SkillAction.ChargeOp1,
                game_stuff.SnipeAction.SnipeOn2 => game_stuff.SkillAction.ChargeOp2,
                game_stuff.SnipeAction.SnipeOn3 => game_stuff.SkillAction.ChargeOp3,
                game_stuff.SnipeAction.SnipeOff => game_stuff.SkillAction.ChargeOff,
                _ => throw new ArgumentOutOfRangeException(nameof(snipeAction), snipeAction, null)
            };
        }

        public TwoDVector? GetMove()
        {
            return SkillAction == null ? Move : null;
        }
    }

    public enum SkillAction
    {
        //Skill
        Op1 = 0,
        Op2 = 1,
        Op3 = 2, //备用
        Switch = 3,
        ChargeOff = 4,

        CatchTrick = 5, //CantOperateInput

        ChargeOp1 = 6,
        ChargeOp2 = 7,
        ChargeOp3 = 8,
        ChargeSwitch = 9
    }

    public enum SpecialAction
    {
        UseProp,
        OutVehicle
    }


    public enum MapInteract
    {
        RecycleCall,
        PickCall,
        InVehicleCall,
        KickVehicleCall,
        GetInfoCall,
        BuyOrApplyCall
    }

    public enum SnipeAction
    {
        SnipeOn1,
        SnipeOn2,
        SnipeOn3, //备用
        SnipeOff,
        SnipeReset
    }
}