using collision_and_rigid;

namespace game_stuff
{
    public class NextSkill
    {
        public TwoDVector? Aim { get; }
        public Skill Skill { get; }
        public SkillAction OpAction { get; }

        public NextSkill(TwoDVector? aim, Skill skill, SkillAction opAction)
        {
            Aim = aim;
            Skill = skill;
            OpAction = opAction;
        }
    }
}