using System.Collections.Immutable;
using System.Linq;

namespace game_stuff
{
    public class Weapon
    {
        public readonly ImmutableDictionary<OpAction, ImmutableDictionary<int, Skill>> SkillGroups;

        public Weapon(ImmutableDictionary<OpAction, ImmutableDictionary<int, Skill>> skillGroups)
        {
            SkillGroups = skillGroups;
        }

        public void PickedBySomebody(CharacterStatus characterStatus)
        {
            foreach (var skill in SkillGroups.Select(keyValuePair => keyValuePair.Value)
                .SelectMany(immutableDictionary => immutableDictionary.Select(valuePair => valuePair.Value)))
            {
                skill.PickedBySomeOne(characterStatus);
            }
        }
    }
}