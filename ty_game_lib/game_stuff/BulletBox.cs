using System;
using collision_and_rigid;

namespace game_stuff
{
    public class BulletBox

    {
        public Zone Zone;
        public IBulletShape BulletShape { get; }

        public BulletBox(Zone zone, IBulletShape bulletShape)
        {
            Zone = zone;
            BulletShape = bulletShape;
        }

        public bool IsHit(TwoDPoint objPos, TwoDPoint bPos, TwoDVector bAim)
        {
            var posInLocal = objPos.GenPosInLocal(bPos, bAim);
#if DEBUG
            Console.Out.WriteLine($"gen pos in local {genPosInLocal}");
#endif
            if (!Zone.IncludePt(posInLocal)) return false;


            var ptInShape = BulletShape.PtRealInShape(posInLocal);
            return ptInShape;
        }
    }
}