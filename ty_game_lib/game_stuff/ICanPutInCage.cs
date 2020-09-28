using collision_and_rigid;

namespace game_stuff
{
    public interface ICanPutInCage
    {
        IMapInteractable? InWhichMapInteractive { get; set; }

        public IMapInteractable GenIMapInteractable(TwoDPoint pos);
        bool CanPick(CharacterStatus characterStatus);
    }
}