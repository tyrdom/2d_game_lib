using collision_and_rigid;

namespace game_stuff
{
    public interface ICharAct
    {
        (TwoDVector? move, IHitStuff? bullet, bool snipeOff) GoATick(TwoDPoint getPos, TwoDVector sightAim,
            TwoDVector? RawMoveVector);


        int NowTough { get; set; }
        uint NowOnTick { get; set; }
        uint TotalTick { get; }
        SkillPeriod InWhichPeriod();
        int? ComboInputRes();
    }
}