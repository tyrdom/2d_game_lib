using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Skill
    {
        private int NowOnTick;

        private int NowTough;


        private Dictionary<int, BulletConfig> LaunchTickToBullet;
        private TwoDVector[] Moves;
        private int MoveStartTick;
        private int? HomingStartTick;
        private int? HomingEndTick;

        private int SkillTick;
        private int ComboTick;


        public Skill(int nowOnTick, int nowTough, Dictionary<int, BulletConfig> launchTickToBullet, TwoDVector[] moves,
            int moveStartTick, int? homingStartTick, int? homingEndTick, int skillTick, int comboTick)
        {
            NowOnTick = nowOnTick;
            NowTough = nowTough;
            LaunchTickToBullet = launchTickToBullet;
            Moves = moves;
            MoveStartTick = moveStartTick;
            HomingStartTick = homingStartTick;
            HomingEndTick = homingEndTick;
            SkillTick = skillTick;
            ComboTick = comboTick;
        }

        public (TwoDVector?, Bullet?, int?) GoATick(TwoDPoint casterPos, TwoDVector casterAim,
             CharacterInBattle caster,
            TwoDPoint? objPos)
        {
// GenVector
            var lockDistance = objPos == null ? null : casterPos.GenVector(objPos);
            TwoDVector? twoDVector = null;
            if
            (lockDistance != null && HomingStartTick != null && HomingEndTick != null &&
             NowOnTick >= HomingStartTick.Value && NowOnTick < HomingEndTick.Value)
            {
                var rest = HomingEndTick.Value - NowOnTick;

                twoDVector = lockDistance.Multi(1 / rest);
            }

            else if (NowOnTick >= MoveStartTick && NowOnTick < MoveStartTick + Moves.Length)

            {
                var moveStartTick = NowOnTick - MoveStartTick;

                twoDVector = Moves[moveStartTick];
            }

// GenBulleft
            Bullet bullet = null;
            if (LaunchTickToBullet.TryGetValue(NowOnTick, out var bulletConfig))
            {
                bullet = bulletConfig.GenBullet(casterPos, casterAim, ref caster, NowTough);
            }

            //GONext
            if (NowOnTick < SkillTick)
            {
                NowTough += TempConfig.ToughGrowPerTick;
                NowOnTick += 1;

                return (twoDVector, bullet, null);
            }

            return (twoDVector, bullet, ComboTick);
        }
    }
}