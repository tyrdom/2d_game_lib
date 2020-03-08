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
        
    }


    public class DamageBuffConfig
    {
    }
}