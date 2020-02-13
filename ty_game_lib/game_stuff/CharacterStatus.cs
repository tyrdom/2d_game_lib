using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    internal enum SkillStatus
    {
        Normal,
        Casting,
        P1,
        P2,
        P3,
        P4
    }

    public class CharacterStatus
    {
        public int GId;
        private SkillStatus SkillStatus;

        public Weapon Weapon1;

        public Weapon Weapon2;
    }

    public class Weapon
    {
        public SkillGroup SkillGroup1;
        public SkillGroup SkillGroup2;
    }

    public class SkillGroup
    {
        private Dictionary<SkillStatus, Skill> CastDic;
    }

    internal class Skill
    {
        private int BaseTough;
        private Dictionary<int, Bullet> launchTick;

        private int MoveFinishTick;

        private TwoDVector MoveSpeed;
        private int MoveStartTick;
        private int totalTick;
    }
}