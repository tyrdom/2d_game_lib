using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{ //todo 直接使用skill，不new对象
    public class SkillConfig
    {
        private int BaseTough;
        private Dictionary<int, BulletConfig> launchTickToBullet;
        private TwoDVector[] Moves;
        private int MoveStartTick;
        private int HomingStartTick;
        private int HomingEndTick;
        private int SkillTick;
        private int ComboTick;
        public WeaponSkillStatus NextCombo;
        public Skill GenSkill(bool isLockObj)
        {
            if (isLockObj)
            {
             
                return new Skill(0, BaseTough, launchTickToBullet, Moves, MoveStartTick, HomingStartTick, HomingEndTick,
                    SkillTick,ComboTick);   
            }
            return new Skill(0, BaseTough, launchTickToBullet, Moves, MoveStartTick, 0, 0,
                SkillTick,ComboTick);
        }
    }
}