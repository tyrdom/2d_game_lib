using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class CharTickMsg
    {
        int Gid;
        TwoDPoint Pos;
        TwoDVector Aim;

        private DamageHealStatus _damageHealStatus;

        private bool SkillOn;
        private Type? AntiBuff;

        public CharTickMsg(int gid, TwoDPoint pos, TwoDVector aim, DamageHealStatus damageHealStatus, bool skillOn, Type? antiBuff)
        {
            Gid = gid;
            Pos = pos;
            Aim = aim;
            _damageHealStatus = damageHealStatus;
            SkillOn = skillOn;
            AntiBuff = antiBuff;
            
        }
    }

    public class CharInitMsg
    {
        int GId;
        TwoDPoint Pos;
        TwoDVector Aim;

        private DamageHealStatus _damageHealStatus;
        private Dictionary<int, WeaponConfig> WeaponConfigs;

        public CharInitMsg(int gId, TwoDPoint pos, TwoDVector aim, DamageHealStatus damageHealStatus, Dictionary<int, WeaponConfig> weaponConfigs)
        {
            GId = gId;
            Pos = pos;
            Aim = aim;
            _damageHealStatus = damageHealStatus;
            WeaponConfigs = weaponConfigs;
        }

       
    }
    
}