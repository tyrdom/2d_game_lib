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

        public float SightR;
        private bool SkillOn;
        private Type? AntiBuff;

        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, DamageHealStatus damageHealStatus, bool skillOn,
            Type? antiBuff, float speed, float sightR)
        {
            Gid = gid;
            Pos = pos;
            Aim = aim;
            _damageHealStatus = damageHealStatus;
            SkillOn = skillOn;
            AntiBuff = antiBuff;
            Speed = speed;
            SightR = sightR;
        }
    }
}