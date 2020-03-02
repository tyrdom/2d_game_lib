using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Skill
    {
        private int NowOnTick;

        private int NowTough;


        private Dictionary<int, BulletConfig> launchTickToBullet;
        private TwoDVector[] Moves;
        private int MoveStartTick;
        private int? HomingStartTick;
        private int? HomingEndTick;

        private int SkillTick;
        private int ComboTick;
        
        

        public Skill(int nowOnTick, int nowTough, Dictionary<int, BulletConfig> launchTickToBullet, TwoDVector[] moves,
            int moveStartTick, int? homingStartTick, int? homingEndTick, int skillTick,int comboTick)
        {
            NowOnTick = nowOnTick;
            NowTough = nowTough;
            this.launchTickToBullet = launchTickToBullet;
            Moves = moves;
            MoveStartTick = moveStartTick;
            HomingStartTick = homingStartTick;
            HomingEndTick = homingEndTick;
            SkillTick = skillTick;
            ComboTick = comboTick;
        }

        public void GoTick()
        {
        }
    }
}