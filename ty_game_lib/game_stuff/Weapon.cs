
using System.Collections.Immutable;

namespace game_stuff
{
    public class Weapon
    {
        public readonly ImmutableDictionary<OpAction, ImmutableDictionary<int, Skill>> SkillGroups;

        public Weapon(ImmutableDictionary<OpAction, ImmutableDictionary<int, Skill>> skillGroups)
        {
            SkillGroups = skillGroups;
        }

        public void PickWeapon(CharacterStatus characterStatus)
        {
            
        }
        
    }
}