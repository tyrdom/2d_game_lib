using game_stuff;

namespace rogue_game
{
    public class WantedBonus
    {
        public GameItem[] AllBonus { get; }
        public GameItem[] KillBonus { get; }
        public IMapInteractable? MapInteractableDrop { get; }

        public WantedBonus(GameItem[] allBonus, GameItem[] killBonus, IMapInteractable? mapInteractableDrop)
        {
            AllBonus = allBonus;
            KillBonus = killBonus;
            MapInteractableDrop = mapInteractableDrop;
        }
    }
}