using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class PassiveTrait : ICanPutInCage
    {
        public PassiveTrait(uint passId, uint level, IPassiveTraitEffect passiveTraitEffect)
        {
            PassId = passId;
            Level = level;
            PassiveTraitEffect = passiveTraitEffect;
        }

        public uint PassId { get; }

        public void AddLevel(uint num)
        {
            Level += num;
        }

        public void RemoveLevel(uint num)
        {
            Level = Level <= num ? 0 : Level - num;
        }

        public void ResetLevel()
        {
            Level = 0;
        }

        public uint Level { get; set; }
        public IPassiveTraitEffect PassiveTraitEffect { get; }
        public IMapInteractable? InWhichMapInteractive { get; set; }

        public IMapInteractable GenIMapInteractable(TwoDPoint pos)
        {
            throw new System.NotImplementedException();
        }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            throw new System.NotImplementedException();
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            throw new System.NotImplementedException();
        }
    }
}