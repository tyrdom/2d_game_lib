using System;
using collision_and_rigid;

namespace game_stuff
{
    public class CharTickMsg
    {
        public int Gid;
        public TwoDPoint Pos;
        public TwoDVector Aim;

        private DamageHealStatus _damageHealStatus;

        private bool SkillOn;
        private Type? AntiBuff;

        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, DamageHealStatus damageHealStatus, bool skillOn,
            Type? antiBuff)
        {
            Gid = gid;
            Pos = pos;
            Aim = aim;
            _damageHealStatus = damageHealStatus;
            SkillOn = skillOn;
            AntiBuff = antiBuff;
        }
    }
}