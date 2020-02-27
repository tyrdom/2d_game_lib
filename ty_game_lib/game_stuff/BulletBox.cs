using collision_and_rigid;

namespace game_stuff
{
    public class BulletBox

    {
        private Zone Zone;
        private IBulletShape BulletShape;

        public BulletBox(Zone zone, IBulletShape bulletShape)
        {
            Zone = zone;
            BulletShape = bulletShape;
        }

        public BulletBox GenBulletboxByRawBox(TwoDPoint pos, TwoDVector aim)

        {
            var clockTurnAboutZero = Zone.ClockTurnAboutZero(aim).MoveToAnchor(pos);
            return new BulletBox(clockTurnAboutZero, BulletShape);
        }


        public bool IsHit(CharacterBody characterBody, TwoDPoint bPos, TwoDVector bAim)
        {
            var characterBodyNowPos = characterBody.NowPos;
            var genPosInLocal = characterBodyNowPos.GenPosInLocal(bPos, bAim);
            var ptInShape = BulletShape.PtInShape(genPosInLocal);
            return ptInShape;
        }
    }
}