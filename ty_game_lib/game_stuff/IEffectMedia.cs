using collision_and_rigid;

namespace game_stuff
{
    public interface IEffectMedia
    {
        public bool CanGoNextTick();
        public IBattleUnitStatus? Caster { get; set; }
        void Sign(CharacterStatus characterStatus);
    }

    public interface IPosMedia : IEffectMedia
    {
        public TwoDPoint Pos { get; set; }
        public TwoDVector Aim { get; set; }

        IPosMedia Active(TwoDPoint casterPos, TwoDVector casterAim);
    }

    public static class PosMediaStandard
    {
        public static IPosMedia Active(TwoDPoint casterPos, TwoDVector casterAim, IPosMedia posMedia)
        {
            posMedia.Pos = casterPos;
            posMedia.Aim = casterAim;
            return posMedia;
        }
    }
}