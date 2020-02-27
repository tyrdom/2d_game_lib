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

        public int? GidWhoSkillLocks;

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

       
        private Dictionary<int, BulletConfig> launchTickToBullet;
        private TwoDVector[] Moves;
        private int MoveStartTick;
        private int? HomingStartTick;
        private int? HomingEndTick;

        private int totalTick;

        public Skill(int nowOnTick, int nowTough, Dictionary<int, BulletConfig> launchTickToBullet, TwoDVector[] moves, int moveStartTick, int? homingStartTick, int? homingEndTick, int totalTick)
        {
            NowOnTick = nowOnTick;
            NowTough = nowTough;
            this.launchTickToBullet = launchTickToBullet;
            Moves = moves;
            MoveStartTick = moveStartTick;
            HomingStartTick = homingStartTick;
            HomingEndTick = homingEndTick;
            this.totalTick = totalTick;
        }
    }
}