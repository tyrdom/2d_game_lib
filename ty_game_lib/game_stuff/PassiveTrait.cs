using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class PassiveTrait : ISaleStuff, IPutInCage
    {
        public PassiveTrait(passive_id passId, uint level, IPassiveTraitEffect passiveTraitEffect)
        {
            PassId = passId;
            Level = level;
            PassiveTraitEffect = passiveTraitEffect;
        }


        public passive_id PassId { get; }

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

        public uint Level { get; private set; }
        public IPassiveTraitEffect PassiveTraitEffect { get; }

        public IMapInteractable PutInteractable(TwoDPoint pos, bool isActive)
        {
            return CanPutInMapInteractableStandard.PutInMapInteractable(pos, this);
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return true;
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return false;
        }

        public IActResult? ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.RecycleCall:
                    characterStatus.RecyclePassive(this);
                    return null;
                case MapInteract.PickCall:
                    characterStatus.PickAPassive(this);
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }
        }

        public int GetId()
        {
            return (int) PassId;
        }

        public int GetNum()
        {
            return 1;
        }

        public static PassiveTrait GenById(string i, uint level)
        {
            if (i.TryStringToEnum(out passive_id i1))
            {
                return GenById(i1, level);
            }

            throw new Exception($"not such passive id{i}");
        }

        public static PassiveTrait GenById(passive_id i, uint level)
        {
            return new PassiveTrait(i, level, PassiveEffectStandard.GenById(i));
        }
    }
}