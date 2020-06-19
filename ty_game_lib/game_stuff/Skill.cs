using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Skill
    {
        private int NowOnTick;

        private int NowTough;

        private readonly int BaseTough;

        private readonly Dictionary<int, BulletConfig> LaunchTickToBullet;
        private readonly TwoDVector[] Moves;
        private readonly int MoveStartTick;

        private bool IsHoming;
        private int HomingStartTick;
        private int HomingEndTick;

        private int SkillTick;
        private int ComboTick;


        public Skill(int nowOnTick, int nowTough, Dictionary<int, BulletConfig> launchTickToBullet, TwoDVector[] moves,
            int moveStartTick, int homingStartTick, int homingEndTick, int skillTick, int comboTick)
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
            CharacterStatus caster,
            TwoDPoint? objPos)
        {
// GenVector
            var lockDistance = objPos == null ? null : casterPos.GenVector(objPos);
            TwoDVector? twoDVector = null;
            if
            (lockDistance != null && IsHoming &&
             NowOnTick >= HomingStartTick && NowOnTick < HomingEndTick)
            {
                var rest = HomingEndTick - NowOnTick;

                twoDVector = lockDistance.Multi(1f / rest);
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