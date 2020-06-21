using System.Collections.Generic;

namespace game_stuff
{
    public class Weapon
    {
        public Dictionary<WeaponSkillStatus, Skill> SkillGroup1;
        public Dictionary<WeaponSkillStatus, Skill> SkillGroup2;
        public Dictionary<WeaponSkillStatus, Skill> SkillGroup3;

        public Weapon(Dictionary<WeaponSkillStatus, Skill> skillGroup1, Dictionary<WeaponSkillStatus, Skill> skillGroup2, Dictionary<WeaponSkillStatus, Skill> skillGroup3)
        {
            SkillGroup1 = skillGroup1;
            SkillGroup2 = skillGroup2;
            SkillGroup3 = skillGroup3;
        }
    }
}