using System.Collections.Generic;
using System.Numerics;
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
        void SurvivalStatusRefresh(Vector<float> survivalAboutPassiveEffects);
        AttackStatus AttackStatus { get; }
        void AttackStatusRefresh(Vector<float> atkAboutPassiveEffects);
        void OtherStatusRefresh(Vector<float> otherAttrPassiveEffects);
        void AddAmmo(int ammoAdd);

        void PassiveEffectChangeOther(Vector<float> otherAttrPassiveEffects,
            (int MaxAmmo, float MoveMaxSpeed, float MoveMinSpeed, float MoveAddSpeed, int StandardPropMaxStack, float
                RecycleMulti) otherBaseStatus);
    }

    public static class BattleUnitStandard
    {
        public static void SurvivalStatusRefresh(Vector<float> survivalAboutPassiveEffects,
            IBattleUnit battleUnit)
        {
            

            var (baseSurvivalStatus, _) = GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(battleUnit.BaseAttrId));
            battleUnit.SurvivalStatus.SurvivalPassiveEffectChange(survivalAboutPassiveEffects, baseSurvivalStatus);
        }

        public static void AtkStatusRefresh(Vector<float> atkAboutPassiveEffects,
            IBattleUnit battleUnit)
        {
            var (_, baseAtkStatus) = GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(battleUnit.BaseAttrId));
            battleUnit.AttackStatus.PassiveEffectChangeAtk(atkAboutPassiveEffects, baseAtkStatus);
        }

        public static void OtherStatusRefresh(Vector<float> otherAttrPassiveEffects,
            IBattleUnit battleUnit)
        {
            var otherBaseStatus =
                GameTools.GenOtherBaseStatusByAttr(GameTools.GenBaseAttrById(battleUnit.BaseAttrId));
            battleUnit.PassiveEffectChangeOther(otherAttrPassiveEffects, otherBaseStatus);
        }
    }
}