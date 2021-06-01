using System.Numerics;

namespace game_stuff
{
    public interface IMoveBattleAttrModel
    {
        float MaxMoveSpeed { get; }
        float MinMoveSpeed { get; }
        float AddMoveSpeed { get; }

        int BaseAttrId { get; }
        SurvivalStatus SurvivalStatus { get; }
        AbsorbStatus AbsorbStatus { get; }
        RegenEffectStatus RegenEffectStatus { get; }
        void SurvivalStatusRefresh(float[] survivalAboutPassiveEffects);
        AttackStatus AttackStatus { get; }
        float TrapAtkMulti { get; set; }
        float TrapSurvivalMulti { get; set; }
        void AttackStatusRefresh(float[] atkAboutPassiveEffects);
        void OtherStatusRefresh(float[] otherAttrPassiveEffects);
        void AddAmmo(int ammoAdd);

        void PassiveEffectChangeOther(float[] otherAttrPassiveEffects,
            (int MaxAmmo, float MoveMaxSpeed, float MoveMinSpeed, float MoveAddSpeed, int StandardPropMaxStack, float
                RecycleMulti) otherBaseStatus);

        void PassiveEffectChangeTrap(float[] trapAdd, (float TrapAtkMulti, float TrapSurvivalMulti) trapBaseAttr);
    }

    public static class BattleUnitMoverStandard
    {
        public static void PassiveEffectChangeTrap(float[] trapAdd,
            (float TrapAtkMulti, float TrapSurvivalMulti) trapBaseAttr, IMoveBattleAttrModel moveBattleAttrModel
        )
        {
            var (trapAtkMulti, trapSurvivalMulti) = trapBaseAttr;
            moveBattleAttrModel.TrapAtkMulti = trapAtkMulti * (1 + trapAdd[0]);
            moveBattleAttrModel.TrapSurvivalMulti = trapSurvivalMulti * (1 + trapAdd[1]);
        }

        public static void SurvivalStatusRefresh(float[] survivalAboutPassiveEffects,
            IMoveBattleAttrModel moveBattleAttrModel)
        {
            var (baseSurvivalStatus, _) =
                GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
                moveBattleAttrModel.SurvivalStatus
                .SurvivalPassiveEffectChange(survivalAboutPassiveEffects,
                    baseSurvivalStatus);
        }

        public static void AtkStatusRefresh(float[] atkAboutPassiveEffects,
            IMoveBattleAttrModel moveBattleAttrModel)
        {
            var (_, baseAtkStatus) =
                GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
            moveBattleAttrModel.AttackStatus.PassiveEffectChangeAtk(atkAboutPassiveEffects, baseAtkStatus);
        }

        public static void OtherStatusRefresh(float[] otherAttrPassiveEffects,
            IMoveBattleAttrModel moveBattleAttrModel)
        {
            var otherBaseStatus =
                GameTools.GenOtherBaseStatusByAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
            moveBattleAttrModel.PassiveEffectChangeOther(otherAttrPassiveEffects, otherBaseStatus);
        }

        public static void TrapAboutRefresh(float[] trapAdd, IMoveBattleAttrModel moveBattleAttrModel)
        {
            var trapBaseAttr =
                GameTools.GenTrapAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
            moveBattleAttrModel.PassiveEffectChangeTrap(trapAdd, trapBaseAttr);
        }

        public static void RegenStatusRefresh(float[] regenAttrPassiveEffects,
            IMoveBattleAttrModel moveBattleAttrModel)
        {
            var regenBaseAttr =
                RegenEffectStatus.GenBaseByAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
            moveBattleAttrModel.RegenEffectStatus.PassiveEffectChange(regenAttrPassiveEffects, regenBaseAttr);
        }

        public static void AbsorbStatusRefresh(float[] vector, IMoveBattleAttrModel moveBattleAttrModel)
        {
            var regenBaseAttr =
                AbsorbStatus.GenBaseByAttr(GameTools.GenBaseAttrById(moveBattleAttrModel.BaseAttrId));
            moveBattleAttrModel.AbsorbStatus.PassiveEffectChange(vector, regenBaseAttr);
        }
    }
}