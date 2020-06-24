using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace game_stuff
{
    public class Weapon
    {
        public readonly ImmutableDictionary<SkillAction, ImmutableDictionary<WeaponSkillStatus, Skill>> SkillGroups;

        public Weapon(ImmutableDictionary<SkillAction, ImmutableDictionary<WeaponSkillStatus, Skill>> skillGroups)
        {
            SkillGroups = skillGroups;
        }
    }
}