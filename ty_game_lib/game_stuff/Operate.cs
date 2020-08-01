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

        public OpAction? Action;

        public TwoDVector? Move;


        public Operate(TwoDVector? aim, OpAction? action, TwoDVector? move)
        {
            Aim = aim;
            Action = action;
            Move = move;
        }

       
        public OpAction? GetAction()
        {
            return Action;
        }

        public TwoDVector? GetMove()
        {
            return Action == null ? Move : null;
        }
    }

    public enum OpAction
    {
        Op1,
        Op2,
        Op3,
        Switch,
        Pick //  far away TODO
    }
}