using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    [Serializable]
    public class Operate
    {
        public SkillAction? Action;
        public TwoDVector? Move;
        public TwoDVector? Aim;

        public Operate(SkillAction? action, TwoDVector? move, TwoDVector aim)
        {
            Action = action;
            Move = move;
            Aim = aim;
        }
    }

    public enum SkillAction
    {
        A1,
        A2,
        A3,
        Switch
        
    }

}