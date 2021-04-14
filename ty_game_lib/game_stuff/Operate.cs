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
            return SkillAction;
        }

        public SnipeAction? GetSnipe()
        {
            return SnipeAction;
        }

        public TwoDVector? GetMove()
        {
            return SkillAction == null ? Move : null;
        }
    }

    public enum SkillAction
    {
        //Skill
        Op1,
        Op2,
        Op3, //备用
        Switch,
        CatchTrick //CantOperateInput
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
        SnipeOff
    }
}