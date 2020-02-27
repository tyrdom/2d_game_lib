using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public static class GameTools
    {
        public static void DoOp(IIdPointShape idPointShape, Dictionary<int, Operate> gidToOp)
        {
            switch (idPointShape)
            {
                case CharacterBody characterBody:
                    characterBody.DoOpFromDic(gidToOp);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(idPointShape));
            }
        }

        public static float GetMaxUp(float? height)
        {
            if (height == null)
            {
                return TempConfig.MaxUp;
            }

            var maxHeight = TempConfig.MaxHeight - height.Value;
            var f = 2f * TempConfig.G * maxHeight;
            return MathF.Sqrt(f);
        }

        static BulletBox GenRawBulletBox(IRawBulletShape shape, float r)
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