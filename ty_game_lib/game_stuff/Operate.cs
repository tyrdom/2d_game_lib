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

        public MapInteractive? MapInteractive { get; }

        public SpecialAction? SpecialAction { get; }

        public Operate(TwoDVector? aim, SkillAction? action, TwoDVector? move, SpecialAction? specialAction = null,
            MapInteractive? mapInteractive = null,
            SnipeAction? snipeAction = null)
        {
            Aim = aim;
            Action = action;
            Move = move;
            SpecialAction = specialAction;
            MapInteractive = mapInteractive;
            SnipeAction = snipeAction;
        }

        public SpecialAction? GetSpecialAction()
        {
            return SpecialAction;
        }

        public MapInteractive? GetMapInteractive()
        {
            if (Action == null && SnipeAction == null)
            {
                return MapInteractive;
            }

            return null;
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
        CatchTrick //CantOperateInput
    }

    public enum SpecialAction
    {
        UseProp,
        OutVehicle
    }


    public enum MapInteractive
    {
        RecycleCall,
        PickOrInVehicle
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