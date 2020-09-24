using collision_and_rigid;

namespace game_stuff
{
    public class InterActiveAct : ICharAct
    {
        public InterActiveAct(uint totalTick)
        {
            TotalTick = totalTick;
        }

        public (TwoDVector? move, IHitStuff? bullet, bool snipeOff) GoATick(TwoDPoint getPos, TwoDVector sightAim,
            TwoDVector? RawMoveVector)
        {
            return (null, null, false);
        }

        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }
        public uint TotalTick { get; }

        public SkillPeriod InWhichPeriod()
        {
            return NowOnTick < TotalTick ? SkillPeriod.Casting : SkillPeriod.End;
        }

        public int? ComboInputRes()
        {
            return null;
        }
    }
}