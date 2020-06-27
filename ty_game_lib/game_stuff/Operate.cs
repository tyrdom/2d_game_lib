using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    [Serializable]
    public class Operate
    {
        public OpAction? Action;
        public TwoDVector? Move;
        public TwoDVector? Aim;

        public Operate(OpAction? action, TwoDVector? move, TwoDVector aim)
        {
            Action = action;
            Move = move;
            Aim = aim;
        }
    }

    public enum OpAction
    {
        Op1,
        Op2,
        Op3,
        Switch
        
    }

}