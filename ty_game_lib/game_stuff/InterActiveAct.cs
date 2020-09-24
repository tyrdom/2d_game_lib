using collision_and_rigid;

namespace game_stuff
{
    public class InterActiveAct : ICharAct
    {
        public (TwoDVector? move, IHitStuff? bullet, bool snipeOff) GoATick(TwoDPoint getPos, TwoDVector sightAim,
            TwoDVector? limitV)
        {
            throw new System.NotImplementedException();
        }

        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }

        public uint TotalTick { get; }

        public SkillPeriod? InWhichPeriod()
        {
            throw new System.NotImplementedException();
        }

        public int? ComboInputRes()
        {
            throw new System.NotImplementedException();
        }
    }
}