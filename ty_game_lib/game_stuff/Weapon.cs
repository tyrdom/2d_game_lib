using System;
using System.Collections.Generic;

namespace game_stuff
{
    public class Weapon
    {
        public Dictionary<SkillAction, Dictionary<WeaponSkillStatus, Skill>> SkillGroups;

        public Weapon(Dictionary<SkillAction, Dictionary<WeaponSkillStatus, Skill>> skillGroups)
        {
            SkillGroups = skillGroups;
        }
    }
}