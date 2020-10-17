using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface ICanPutInCage
    {
        IMapInteractable? InWhichMapInteractive { get; set; }

        public IMapInteractable GenIMapInteractable(TwoDPoint pos);
        bool CanInterActOneBy(CharacterStatus characterStatus);

        bool CanInterActTwoBy(CharacterStatus characterStatus);
        IEnumerable<IMapInteractable> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive);
    }
}