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


        public bool IsHit(TwoDPoint objPos, TwoDPoint bPos, TwoDVector bAim)
        {
            var genPosInLocal = objPos.GenPosInLocal(bPos, bAim);
            if (!Zone.IncludePt(genPosInLocal)) return false;
            var ptInShape = BulletShape.PtRealInShape(genPosInLocal);
            return ptInShape;

        }
    }
}