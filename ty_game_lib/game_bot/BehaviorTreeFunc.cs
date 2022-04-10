using System;
using System.Linq;
using collision_and_rigid;
using game_stuff;
using game_config;

namespace game_bot
{
    public static class BehaviorTreeFunc
    {
        public static Random Random { get; } = new Random();

        public static SkillAction CovOp(botOp botOp)
        {
            return botOp switch
            {
                botOp.op1 => SkillAction.Op1,
                botOp.op2 => SkillAction.Op2,
                botOp.none => SkillAction.Op1,
                _ => throw new ArgumentOutOfRangeException(nameof(botOp), botOp, null)
            };
        }

        public static (bool, Operate?) GetComboAct(IAgentStatus[] botAgent)
        {
            var firstOrDefault = botAgent.OfType<BotMemory>().First();
            var firstSkillCtrl = firstOrDefault.FirstSkillCtrl;
            var tryGetNextSkillAction =
                firstOrDefault.ComboCtrl.TryGetNextSkillAction(firstSkillCtrl, Random, out var skillAction);

            var orDefault = botAgent.OfType<TargetMsg>().FirstOrDefault();
            if (orDefault == null)
                return (tryGetNextSkillAction
                    , new Operate(skillAction: skillAction));
            var bodyStatus = botAgent.OfType<BodyStatus>().First();
            var dPoint = bodyStatus.CharacterBody.GetAnchor();
            var twoDPoint = orDefault.Target.GetAnchor();
            var twoDVector = new TwoDVector(dPoint, twoDPoint).GetUnit2();
            return (tryGetNextSkillAction
                , new Operate(skillAction: skillAction, aim: twoDVector));
        }

        public static bool CheckHitSth(IAgentStatus[] botAgent)
        {
            var checkHitSth = botAgent.OfType<HitSth>().Any();
#if DEBUG
            if (checkHitSth)
            {
                Console.Out.WriteLine("hit sth");
            }
#endif
            return checkHitSth;
        }

        public static (bool, Operate?) SetComboStatus(IAgentStatus[] botAgent)
        {
            var botMemory = botAgent.OfType<BotMemory>().First();
            botMemory.ComboCtrl.SetComboStart();
            return (true, null);
        }

        public static (bool, Operate?) TracePt(IAgentStatus[] botAgents)
        {
            var traceToPtMsg = botAgents.OfType<TraceToPtMsg>().FirstOrDefault();
            if (traceToPtMsg == null) return (false, null);
            var twoDPoint = traceToPtMsg.TracePt;
            var isSlow = traceToPtMsg.IsSlow;
            var bodyStatus = botAgents.OfType<BodyStatus>().First();
            var dPoint = bodyStatus.CharacterBody.GetAnchor();

            Console.Out.WriteLine($"bot:: {bodyStatus.CharacterBody.GetId()} trace pt {twoDPoint}");

            var twoDVector = new TwoDVector(dPoint, twoDPoint).GetUnit();
            var patrolSlowMulti = BotLocalConfig.BotOtherConfig.PatrolSlowMulti;
            var dVector = twoDVector.Multi(patrolSlowMulti);
            var vector = isSlow ? dVector : twoDVector;
            var operate = new Operate(move: vector);
            return (true, operate);
        }

        public static (bool, Operate?) TraceAim(IAgentStatus[] botAgents)
        {
            var traceToAimMsg = botAgents.OfType<TraceToAimMsg>().FirstOrDefault();
            var botMemory = botAgents.OfType<BotMemory>().First();
            if (traceToAimMsg == null) return (false, null);
            {
                var twoDVector = traceToAimMsg.Aim;
                var operate = new Operate(twoDVector);
                botMemory.AimTraced = true;
                return (true, operate);
            }
        }


        public static (bool, Operate?) PropUse(IAgentStatus[] botAgents)
        {
            var any = botAgents.OfType<PropUse>().Any();
            return (any,
                any
                    ? new Operate(specialAction: SpecialAction.UseProp)
                    : null);
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

            var targetMsg = ofType.First();
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
                    var goATick = botMemories.FirstSkillCtrl.GetAct(Random);
                    var twoDVector = new TwoDVector(dPoint, twoDPoint).GetUnit2();
                    var operate = new Operate(skillAction: goATick, aim: twoDVector);
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

            return characterBodyCharacterStatus.NowCastAct != null &&
                   characterBodyCharacterStatus.NowCastAct.InWhichPeriod() == SkillPeriod.Casting;
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
}