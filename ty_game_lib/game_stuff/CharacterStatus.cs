using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public enum WeaponSkillStatus
    {
        Normal,
        Catching,
        P1Ok,
        P2Ok,
        P3Ok,
        P4Ok,
        P5Ok
    }

    public class CharacterStatus
    {
        public CharacterBody CharacterBody;
        
        public int GId;

        public int PauseTick;

        public int? GidWhoSkillLocks;

        public CharacterStatus? Catching;
        public int NowWeapon;

        public Dictionary<int, WeaponConfig> WeaponConfigs;

        public Skill? NowCast;

        public WeaponSkillStatus WeaponSkillStatus;

        public int ComboTick;

        public int NowTough;

        public AntiActBuff? AntiActBuff;


        public DamageBuff DamageBuff;

        public DamageHealAbout DamageHealAbout;
        public int ProtectTick;

        public TwoDPoint GetPos()
        {
            return CharacterBody.NowPos;
        }
    }

    public class DamageHealAbout
    {
        private int MaxHp;
        private int NowHp;

       public void TakeDamage(Damage damage)
        {
            NowHp -= damage.DamageValue;
        }

       public void GetHeal(Heal heal)
        {
            
        }
    }

    public class Heal
    {
        private int HealValue;
    }


    public class DamageBuff
    {
    }
}