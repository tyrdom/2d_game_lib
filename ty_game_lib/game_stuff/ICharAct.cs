using System;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface ICharAct
    {
        (ITwoDTwoP? move, IEffectMedia? bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage, MapInteract interactive)
            GoATick(TwoDPoint getPos,
                TwoDVector sightAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV);

        int GetIntId();
        int NowTough { get; set; }
        uint NowOnTick { get; set; }
        uint TotalTick { get; }
        SkillPeriod InWhichPeriod();
        int? ComboInputRes();
        action_type GetTypeEnum();
    }
}