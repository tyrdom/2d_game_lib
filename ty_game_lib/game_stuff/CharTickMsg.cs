using System;
using collision_and_rigid;

namespace game_stuff
{
    public class CharTickMsg
    {
        public int Gid;
        public TwoDPoint Pos;
        public TwoDVector Aim;
        public float Speed;
        private DamageHealStatus _damageHealStatus;
        public bool IsPause;
        public float SightR;
        public bool SkillOn;
        public bool SkillAct;
        public bool IsStun;
        public TwoDVector? IsBeHit;
        public bool IsHitSome;

        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, DamageHealStatus damageHealStatus, bool skillOn,
            bool antiBuff, float speed, float sightR, bool isPause, bool skillAct, TwoDVector? isBeHit, bool isHitSome)
        {
            Gid = gid;
            Pos = pos;
            Aim = aim;
            _damageHealStatus = damageHealStatus;
            SkillOn = skillOn;
            IsStun = antiBuff;
            Speed = speed;
            SightR = sightR;
            IsPause = isPause;
            SkillAct = skillAct;
            IsBeHit = isBeHit;
            IsHitSome = isHitSome;
        }
    }
}