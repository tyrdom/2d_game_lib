using System.Collections.Generic;
using System.Numerics;
using game_config;

namespace game_stuff
{
    public interface IMoveBattleAttrModel
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
            IMoveBattleAttrModel moveBattleAttrModel)
        {
            var (baseSurvivalStatus, _) =
                GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
            moveBattleAttrModel.SurvivalStatus.SurvivalPassiveEffectChange(survivalAboutPassiveEffects,
                baseSurvivalStatus);
        }

        public static void AtkStatusRefresh(Vector<float> atkAboutPassiveEffects,
            IMoveBattleAttrModel moveBattleAttrModel)
        {
            var (_, baseAtkStatus) =
                GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
            moveBattleAttrModel.AttackStatus.PassiveEffectChangeAtk(atkAboutPassiveEffects, baseAtkStatus);
        }

        public static void OtherStatusRefresh(Vector<float> otherAttrPassiveEffects,
            IMoveBattleAttrModel moveBattleAttrModel)
        {
            var otherBaseStatus =
                GameTools.GenOtherBaseStatusByAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
            moveBattleAttrModel.PassiveEffectChangeOther(otherAttrPassiveEffects, otherBaseStatus);
        }
    }
}