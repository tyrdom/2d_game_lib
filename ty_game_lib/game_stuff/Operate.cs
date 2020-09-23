using System;
using collision_and_rigid;


namespace game_stuff
{
    public class Operate
    {
        // public OpAction? Action;
        // public TwoDVector? Move;
        public TwoDVector? Aim { get; }


        public SkillAction? Action { get; }

        public TwoDVector? Move { get; }

        public SnipeAction? SnipeAction { get; }

        public Operate(TwoDVector? aim, SkillAction? action, TwoDVector? move, SnipeAction? snipeAction = null)
        {
            Aim = aim;
            Action = action;
            Move = move;
            SnipeAction = snipeAction;
        }


        public SkillAction? GetAction()
        {
            return SnipeAction == null ? Action : null;
        }

        public SnipeAction? GetSnipe()
        {
            return SnipeAction;
        }

        public TwoDVector? GetMove()
        {
            return Action == null ? Move : null;
        }
    }

    public enum SkillAction
    {
        //Skill
        Op1,
        Op2,
        Op3,
        Switch,
        CatchTrick, //CantOperateInput
        
        
        UseProp,
        RecycleCall,
        PickOrInVehicle,

        //  TODO
        OutVehicle
    }

    public enum SnipeAction
    {
        //Snipe
        SnipeOn1,
        SnipeOn2,
        SnipeOn3,
        SnipeOff
    }
}