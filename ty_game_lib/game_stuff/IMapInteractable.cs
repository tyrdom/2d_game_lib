using collision_and_rigid;

namespace game_stuff
{
    public interface IMapInteractable : IAaBbBox
    {
        Round CanInterActiveRound { get; }
        public CharacterBody? NowInterUser { get; set; }
        public Interaction CharAct { get; }
    }
}