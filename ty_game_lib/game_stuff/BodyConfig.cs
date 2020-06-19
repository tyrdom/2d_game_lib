using System;
using System.Collections.Generic;
using System.Linq;

namespace game_stuff
{
    public class BodyConfig
    {
    }

    public class WeaponConfig
    {
        public Dictionary<WeaponSkillStatus, SkillConfig> SkillGroup1;
        public Dictionary<WeaponSkillStatus, SkillConfig> SkillGroup2;
        public Dictionary<WeaponSkillStatus, SkillConfig> SkillGroup3;

        public WeaponConfig(Dictionary<WeaponSkillStatus, SkillConfig> skillGroup1, Dictionary<WeaponSkillStatus, SkillConfig> skillGroup2, Dictionary<WeaponSkillStatus, SkillConfig> skillGroup3)
        {
            SkillGroup1 = skillGroup1;
            SkillGroup2 = skillGroup2;
            SkillGroup3 = skillGroup3;
        }
    }


    public class DamageBuffConfig
    {
    }
}