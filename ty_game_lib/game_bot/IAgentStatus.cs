using System.Collections.Generic;
using collision_and_rigid;
using game_stuff;

namespace game_bot
{
    public interface IAgentStatus
    {
    }

    public class TargetMsg : IAgentStatus
    {
        public ICanBeAndNeedHit Target { get; }

        public TargetMsg(ICanBeAndNeedHit target)
        {
            Target = target;
        }
    }

    public readonly struct BodyStatus : IAgentStatus
    {
        public List<(float range, int maxAmmoUse, int weaponIndex)> NowRangeToWeapon { get; }

        public CharacterBody CharacterBody { get; }


        public BodyStatus(List<(float range, int maxAmmoUse, int weaponIndex)> nowRangeToWeapon,
            CharacterBody characterBody)
        {
            NowRangeToWeapon = nowRangeToWeapon;

            CharacterBody = characterBody;
        }
    }

    public record TraceToPtMsg : IAgentStatus
    {
        public TwoDPoint TracePt { get; }


        public TraceToPtMsg(TwoDPoint tracePt)
        {
            TracePt = tracePt;
        }
    }

    public record TraceToAimMsg : IAgentStatus
    {
        public TraceToAimMsg(TwoDVector aim)
        {
            Aim = aim;
        }

        public TwoDVector Aim { get; }
    }
    
    public record BotMemory : IAgentStatus
    {
        public ComboCtrl ComboCtrl { get; }
        public FirstSkillCtrl FirstSkillCtrl { get; }

        public BotMemory(ComboCtrl comboCtrl, FirstSkillCtrl firstSkillCtrl)
        {
            ComboCtrl = comboCtrl;
            FirstSkillCtrl = firstSkillCtrl;
        }
    }

    public record HitSth : IAgentStatus;
}