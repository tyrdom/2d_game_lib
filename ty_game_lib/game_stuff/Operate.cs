using System;
using collision_and_rigid;


namespace game_stuff
{
    [Serializable]
    public class Operate
    {
        // public OpAction? Action;
        // public TwoDVector? Move;
        public TwoDVector? Aim;

        public SkillAction? Action;

        public TwoDVector? Move;


        public Operate(TwoDVector? aim, SkillAction? action, TwoDVector? move)
        {
            Aim = aim;
            Action = action;
            Move = move;
        }


        public SkillAction? GetAction()
        {
            return Action;
        }

        public TwoDVector? GetMove()
        {
            return Action == null ? Move : null;
        }
    }

    public enum SkillAction
    {
        Op1,
        Op2,
        Op3,
        Switch,
        CatchTrick,
        Pick //  far away TODO
    }
}