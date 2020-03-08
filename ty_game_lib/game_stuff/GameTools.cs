using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public static class GameTools
    {
        public static (ITwoDTwoP?,Bullet?) BodyGoATick(IIdPointShape idPointShape, Dictionary<int, Operate> gidToOp)
        {
            switch (idPointShape)
            {
                case CharacterBody characterBody:
                    var doOpFromDic = characterBody.BodyGoATick(gidToOp);
                    return doOpFromDic;
                default:
                    throw new ArgumentOutOfRangeException(nameof(idPointShape));
            }
        }

        public static float GetMaxUpSpeed(float? height)
        {
            if (height == null)
            {
                return TempConfig.MaxUpSpeed;
            }

            var maxHeight = TempConfig.MaxHeight - height.Value;
            var f = 2f * TempConfig.G * maxHeight;
            return MathF.Sqrt(f);
        }

        private static BulletBox GenRawBulletBox(IRawBulletShape shape, float r)
        {
            var genBulletZone = shape.GenBulletZone(r);
            var genBulletShape = shape.GenBulletShape(r);
            var bulletBox = new BulletBox(genBulletZone,genBulletShape);
            return bulletBox;
        }

        static Dictionary<BodySize, BulletBox> GenDicBulletBox(IRawBulletShape shape)
        {
            var bulletBoxes = new Dictionary<BodySize,BulletBox>();
            foreach (var (key, value) in TempConfig.SizeToR)
            {
                var genRawBulletBox = GenRawBulletBox(shape,value);

                bulletBoxes[key] = genRawBulletBox;
            }

            return bulletBoxes;
        }
    }
}