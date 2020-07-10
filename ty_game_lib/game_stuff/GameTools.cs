using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public static class GameTools
    {
        public static (ITwoDTwoP?, Bullet?) BodyGoATick(IIdPointShape idPointShape, Dictionary<int, Operate> gidToOp)
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

        public static bool IsHit(IHitStuff hitStuff, CharacterBody characterBody)
        {
            var characterBodyBodySize = characterBody.BodySize;
            return hitStuff.SizeToBulletCollision.TryGetValue(characterBodyBodySize, out var bulletBox) &&
                   bulletBox.IsHit(characterBody.NowPos, hitStuff.Pos, hitStuff.Aim);
        }

        public static float GetMaxUpSpeed(float? height)
        {
            if (height == null)
            {
                return TempConfig.MaxUpSpeed;
            }

            var maxHeight = TempConfig.MaxHeight - height.Value;
            var f = 2f * TempConfig.G * maxHeight;
            return MathTools.Sqrt(f);
        }

        private static BulletBox GenRawBulletBox(IRawBulletShape shape, float r)
        {
            var genBulletZone = shape.GenBulletZone(r);
            var genBulletShape = shape.GenBulletShape(r);
            var bulletBox = new BulletBox(genBulletZone, genBulletShape);
            return bulletBox;
        }

        static Dictionary<BodySize, BulletBox> GenDicBulletBox(IRawBulletShape shape)
        {
            var bulletBoxes = new Dictionary<BodySize, BulletBox>();
            foreach (var k in TempConfig.SizeToR)
            {
                var genRawBulletBox = GenRawBulletBox(shape, k.Value);

                bulletBoxes[k.Key] = genRawBulletBox;
            }

            return bulletBoxes;
        }
    }
}