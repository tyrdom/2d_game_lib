using collision_and_rigid;

namespace game_stuff
{
    public interface IEffectMedia
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }
        public bool CanGoNextTick();
        public CharacterStatus? Caster { get; set; }
    }
}