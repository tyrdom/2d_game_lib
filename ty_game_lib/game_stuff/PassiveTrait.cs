using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        public void SetLevel(uint level)
        {
            Level = level;
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

        public ImmutableArray<IActResult> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.RecycleCall:
                    characterStatus.RecyclePassive(this);
                    return ImmutableArray<IActResult>.Empty;
                case MapInteract.PickCall:
                    characterStatus.PickAPassive(this);
                    return ImmutableArray<IActResult>.Empty;
                default:
                    return ImmutableArray<IActResult>.Empty;
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

        public static IEnumerable<PassiveTrait> GenManyById(string i, uint level)
        {
            if (i.TryStringToEnum(out passive_id i1))
            {
                return GenManyByPId(i1, level);
            }

            throw new Exception($"not such passive id{i}");
        }

        private static IEnumerable<PassiveTrait> GenManyByPId(passive_id eId, uint level)
        {
            if (!CommonConfig.Configs.passives.TryGetValue(eId, out var passive))
                throw new KeyNotFoundException($"not such passive id {eId}");
            var passiveTraits = passive.AddOns.Select(x => GenById(x, level));
            var genById = GenById(eId, level);
            var enumerable = passiveTraits.Append(genById);
            return enumerable;
        }

        private static PassiveTrait GenById(string i, uint level)
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