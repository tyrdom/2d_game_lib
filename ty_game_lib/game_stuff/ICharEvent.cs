using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface ICharEvent
    {
    }

    public class PickWeapon : ICharEvent
    {
        public PickWeapon(weapon_id weaponWId)
        {
            WeaponId = weaponWId;
        }

        public weapon_id WeaponId { get; }
    }

    public class GetStunBuff : ICharEvent
    {
        public GetStunBuff(uint stunBuffRestTick)
        {
            RestTick = stunBuffRestTick;
        }

        public uint RestTick { get; }
    }

    public class SightAngleChange : ICharEvent
    {
        public SightAngleChange(float theta)
        {
            Theta = theta;
        }

        public float Theta { get; }
    }

    public class SightRChange : ICharEvent
    {
        public SightRChange(float sightNowR)
        {
            SightR = sightNowR;
        }

        public float SightR { get; }
    }

    public class AimChange : ICharEvent
    {
        public AimChange(TwoDVector sightAim)
        {
            Aim = sightAim;
        }

        public TwoDVector Aim { get; }
    }

    public class PosChange : ICharEvent
    {
        public PosChange(TwoDPoint characterBodyNowPos)
        {
            Pos = characterBodyNowPos;
        }

        public TwoDPoint Pos { get; }
    }


    public enum BaseChangeMark
    {
        PosC,
        AimC,
        NowRc,
        ThetaC
    }


    public class HitMark : ICharEvent
    {
        public HitMark(TwoDVector twoDVectorByPt)
        {
            HitDirV = twoDVectorByPt;
        }

        public TwoDVector HitDirV { get; }
    }

    public class TickSnipeActionLaunch : ICharEvent
    {
        public TickSnipeActionLaunch(SnipeAction snipeAction)
        {
            SniperAction = snipeAction;
        }

        public SnipeAction SniperAction { get; }
    }

    public class StartAct : ICharEvent
    {
        public StartAct(action_type getTypeEnum, int getIntId, uint charActNowOnTick)
        {
            TypeEnum = getTypeEnum;
            IntId = getIntId;
            OnTick = charActNowOnTick;
        }

        public action_type TypeEnum { get; }
        public int IntId { get; }
        public uint OnTick { get; }
    }

    public class GetPauseTick : ICharEvent
    {
        public GetPauseTick(int tick)
        {
            GetTick = tick;
        }

        public int GetTick { get; }
    }
}