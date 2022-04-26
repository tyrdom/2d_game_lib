using System.Collections.Generic;
using collision_and_rigid;
using game_stuff;

namespace game_bot
{
    public interface IAgentStatus
    {
    }

    public class PropUse : IAgentStatus
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

    public class TraceToPtMsg : IAgentStatus
    {
        public TwoDPoint TracePt { get; }

        public bool IsSlow { get; }

        public TraceToPtMsg(TwoDPoint tracePt, bool isSlow)
        {
            TracePt = tracePt;
            IsSlow = isSlow;
        }
    }

    public class TraceToAimMsg : IAgentStatus
    {
        public TraceToAimMsg(TwoDVector aim)
        {
            Aim = aim;
        }

        public TwoDVector Aim { get; }
    }

    public class BotMemory : IAgentStatus
    {
        public ComboCtrl ComboCtrl { get; }
        public FirstSkillCtrl FirstSkillCtrl { get; }

        // public bool AimTracing { get; set; }

        public BotMemory(ComboCtrl comboCtrl, FirstSkillCtrl firstSkillCtrl)
        {
            ComboCtrl = comboCtrl;
            FirstSkillCtrl = firstSkillCtrl;
            // AimTracing = true;
        }
    }

    public class HitSth : IAgentStatus
    {
    }
}