using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Operate
    {
        public SkillAction? Action;
        public TwoDVector? Move;
        public TwoDVector Aim;
        
    }

    public enum SkillAction
    {
        A1,
        A2,
        A3
    }

}