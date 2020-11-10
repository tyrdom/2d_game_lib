using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class PassiveTrait : ICanPutInMapInteractable
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
            return CanPutInMapInteractableStandard.GenIMapInteractable(pos, this);
        }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return true;
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return false;
        }

        public IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.RecycleCall:
                    characterStatus.RecyclePass(this);
                    return new List<IMapInteractable>();
                case MapInteract.PickCall:
                    characterStatus.PickAPassive(this);
                    return new List<IMapInteractable>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }
        }
    }
}