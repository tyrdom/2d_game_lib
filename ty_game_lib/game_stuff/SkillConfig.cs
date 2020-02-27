using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class SkillConfig

    {
        private int BaseTough;
        private Dictionary<int, BulletConfig> launchTickToBullet;
        private TwoDVector[] Moves;
        private int MoveStartTick;
        private int HomingStartTick;
        private int HomingEndTick;
        private int totalTick;


        Skill GenSkill(bool isLockObj)
        {
            if (isLockObj)
            {
             
                return new Skill(0, BaseTough, launchTickToBullet, Moves, MoveStartTick, HomingStartTick, HomingEndTick,
                    totalTick);   
            }
            return new Skill(0, BaseTough, launchTickToBullet, Moves, MoveStartTick, null, null,
                totalTick);
        }
    }
}