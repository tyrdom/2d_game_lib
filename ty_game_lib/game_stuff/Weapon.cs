
using System.Collections.Immutable;

namespace game_stuff
{
    public class Weapon
    {
        public readonly ImmutableDictionary<OpAction, ImmutableDictionary<WeaponSkillStatus, Skill>> SkillGroups;

        public Weapon(ImmutableDictionary<OpAction, ImmutableDictionary<WeaponSkillStatus, Skill>> skillGroups)
        {
            SkillGroups = skillGroups;
        }
        
        
    }
}