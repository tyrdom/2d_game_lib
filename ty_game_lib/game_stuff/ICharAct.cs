using System;
using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface ICharAct
    {
        (ITwoDTwoP? move, IEnumerable<IEffectMedia> bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage, MapInteract interactive)
            GoATick(CharacterStatus caster,
                TwoDVector? rawMoveVector, TwoDVector? limitV, SkillAction? skillAction);

        int GetIntId();
        int NowTough { get; set; }
        uint NowOnTick { get; set; }
        uint TotalTick { get; }
        SkillPeriod InWhichPeriod();
        int? ComboInputRes();
        action_type GetTypeEnum();
    }
}