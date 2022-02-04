using System;
using System.Linq;
using System.Transactions;
using collision_and_rigid;
using game_stuff;

namespace game_bot
{
    public static class BehaviorTreeFunc
    {
        public static Random Random { get; } = new();


        public static (bool, Operate?) GetComboAct(IAgentStatus[] botAgent)
        {
            var firstOrDefault = botAgent.OfType<BotMemory>().First();
            return (firstOrDefault.ComboCtrl.TryGetNextSkillAction(out var skillAction)
                , new Operate(skillAction: skillAction));
        }

        public static bool CheckHitSth(IAgentStatus[] botAgent)
        {
            var checkHitSth = botAgent.OfType<HitSth>().Any();

            return checkHitSth;
        }

        public static (bool, Operate?) SetComboStatus(IAgentStatus[] botAgent)
        {
            var botMemory = botAgent.OfType<BotMemory>().First();
            var botMemoryFirstSkillCtrl = botMemory.FirstSkillCtrl;
            var genNextSkillAction = botMemory.ComboCtrl.GenNextSkillAction(botMemoryFirstSkillCtrl, Random);
            return (genNextSkillAction, null);
        }

        public static (bool, Operate?) Trace(IAgentStatus[] botAgents)
        {
            var traceToPtMsg = botAgents.OfType<TraceToPtMsg>().FirstOrDefault();
            if (traceToPtMsg != null)
            {
                var twoDPoint = traceToPtMsg.TracePt;
                var bodyStatus = botAgents.OfType<BodyStatus>().First();
                var dPoint = bodyStatus.CharacterBody.GetAnchor();
                var twoDVector = new TwoDVector(dPoint, twoDPoint).GetUnit();
                var operate = new Operate(move: twoDVector);
                return (true, operate);
            }

            var traceToAimMsg = botAgents.OfType<TraceToAimMsg>().FirstOrDefault();
            if (traceToAimMsg == null) return (false, null);
            {
                var twoDVector = traceToAimMsg.Aim;
                var operate = new Operate(twoDVector);
                return (true, operate);
            }
        }

        public static (bool, Operate?) TargetApproach(IAgentStatus[] botAgent)
        {
            var ofType = botAgent.OfType<TargetMsg>().ToArray();
            if (!ofType.Any())
            {
                return (false, null);
            }

            var targetMsg = ofType.First();
            var twoDPoint = targetMsg.Target.GetAnchor();
            var bodyStatus = botAgent.OfType<BodyStatus>().First();
            var dPoint = bodyStatus.CharacterBody.GetAnchor();
            var twoDVector = new TwoDVector(dPoint, twoDPoint).GetUnit();
            var operate = new Operate(move: twoDVector);
            return (true, operate);
        }

        public static (bool, Operate?) CanUseWeaponToTarget(IAgentStatus[] botAgent)
        {
            var ofType = botAgent.OfType<TargetMsg>().ToArray();
            if (!ofType.Any())
            {
                return (false, null);
            }

            var targetMsg = ofType.FirstOrDefault();
            var bodyStatus = botAgent.OfType<BodyStatus>().FirstOrDefault();

            var twoDPoint = targetMsg.Target.GetAnchor();
            var bodyStatusCharacterBody = bodyStatus.CharacterBody;
            var dPoint = bodyStatusCharacterBody.GetAnchor();
            var distance = twoDPoint.GetDistance(dPoint);

            var valueTuples = bodyStatus.NowRangeToWeapon.Where(x =>
                x.range > distance && x.maxAmmoUse <= bodyStatusCharacterBody.CharacterStatus.GetAmmo()).ToArray();
            if (!valueTuples.Any()) return (false, null);
            {
                var characterStatusNowWeapon = bodyStatusCharacterBody.CharacterStatus.NowWeapon;
                var any = valueTuples.Any(x => x.weaponIndex == characterStatusNowWeapon);

                if (any)
                {
                    var botMemories = botAgent.OfType<BotMemory>().First();
                    var goATick = botMemories.FirstSkillCtrl.NoThinkAct(Random);
                    var operate = new Operate(skillAction: goATick);
                    return (true, operate);
                }
                else
                {
                    var operate = new Operate(skillAction: SkillAction.Switch);
                    return (true, operate);
                }
            }
        }

        public static bool InActingFunc(
            IAgentStatus[] botAgent)
        {
            var bodyStatus = botAgent.OfType<BodyStatus>().First();
            var characterBodyCharacterStatus = bodyStatus.CharacterBody.CharacterStatus;
            return characterBodyCharacterStatus.NowCastAct != null;
        }

        public static bool IsStunFunc(
            IAgentStatus[] botAgent)
        {
            var bodyStatus = botAgent.OfType<BodyStatus>().First();
            var characterBodyCharacterStatus = bodyStatus.CharacterBody.CharacterStatus;
            return characterBodyCharacterStatus.StunBuff != null;
        }

        public static bool IsDeadFunc(
            IAgentStatus[] botAgent)
        {
            var bodyStatus = botAgent.OfType<BodyStatus>().First();
            var characterBodyCharacterStatus = bodyStatus.CharacterBody.CharacterStatus;
            var isDeadOrCantDmg = characterBodyCharacterStatus.SurvivalStatus.IsDead();
            return isDeadOrCantDmg;
        }
    }

    public struct BotMemory : IAgentStatus
    {
        public PatrolCtrl PatrolCtrl { get; }
        public ComboCtrl ComboCtrl { get; }
        public FirstSkillCtrl FirstSkillCtrl { get; }
    }
}