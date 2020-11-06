using System.Collections.Generic;
using game_config;

namespace game_stuff
{
    public interface IBattleUnit
    {
        float MaxMoveSpeed { get; }
        float MinMoveSpeed { get; }
        float AddMoveSpeed { get; }

        base_attr_id BaseAttrId { get; }
        SurvivalStatus SurvivalStatus { get; }
        void SurvivalStatusRefresh(IEnumerable<SurvivalAboutPassiveEffect> survivalAboutPassiveEffects);
        AttackStatus AttackStatus { get; }
        void AttackStatusRefresh(IEnumerable<AtkAboutPassiveEffect> atkAboutPassiveEffects);
    }

    public static class BattleUnitStandard
    {
        public static void SurvivalStatusRefresh(IEnumerable<SurvivalAboutPassiveEffect> survivalAboutPassiveEffects,
            IBattleUnit battleUnit)
        {
            var (baseSurvivalStatus, _) = GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(battleUnit.BaseAttrId));
            battleUnit.SurvivalStatus.SurvivalPassiveEffectChange(survivalAboutPassiveEffects, baseSurvivalStatus);
        }

        public static void AtkStatusRefresh(IEnumerable<AtkAboutPassiveEffect> atkAboutPassiveEffects,
            IBattleUnit battleUnit)
        {
            var (_, baseAtkStatus) = GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(battleUnit.BaseAttrId));
            battleUnit.AttackStatus.PassiveEffectChangeAtk(atkAboutPassiveEffects, baseAtkStatus);
        }
    }
}