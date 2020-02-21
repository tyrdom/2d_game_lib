using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public enum WeaponSkillStatus
    {
        Normal,
        Casting,
        Catching,
        P1Ok,
        P2Ok,
        P3Ok,
        P4Ok,
        P5Ok
    }

    public class CharacterStatus
    {
        public int GId;

        public int PauseTick;

        public int? GidWhoLocks;

        public Weapon Weapon1;

        public Weapon Weapon2;

        public Skill NowCast;
        
    }

    public class Weapon
    {
        public WeaponSkillStatus WeaponSkillStatus;
        public Dictionary<WeaponSkillStatus, SkillConfig> SkillGroup1;
        public Dictionary<WeaponSkillStatus, SkillConfig> SkillGroup2;
    }


    public class Skill
    {
        private int NowOnTick;
        private int NowTough;

        private int BaseTough;
        private Dictionary<int, Bullet> launchTickToBullet;
        private List<TwoDVector> Moves;
        private int MoveStartTick;
        private int? HomingTicks;

        private int totalTick;
    }
}