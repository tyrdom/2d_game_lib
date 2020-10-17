using collision_and_rigid;

namespace game_stuff
{
    public interface ICharAct
    {
        (ITwoDTwoP? move, IHitStuff? bullet, bool snipeOff, ICanPutInCage? getFromCage, MapInteract interactive)
            GoATick(
                TwoDPoint getPos,
                TwoDVector sightAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV);


        int NowTough { get; set; }
        uint NowOnTick { get; set; }
        uint TotalTick { get; }
        SkillPeriod InWhichPeriod();
        int? ComboInputRes();
    }
}