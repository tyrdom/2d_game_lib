using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class GameTools
    {
        public static (ITwoDTwoP?, IHitStuff?) BodyGoATick(IIdPointShape idPointShape, Dictionary<int, Operate> gidToOp)
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

        public static Dictionary<BodySize, BulletBox> GenBulletShapes(float[] bulletShapeParams, int bulletLocalRotate,
            Point localPoint, raw_shape rawShape)
        {
            var twoDPoint = GenVectorByConfig(localPoint);

            var cos = MathTools.Cos(bulletLocalRotate * 180 / MathTools.Pi());
            var sin = MathTools.Sin(bulletLocalRotate * 180 / MathTools.Pi());
            var rotate = new TwoDVector(cos, sin);
            IRawBulletShape rawBulletShape = rawShape switch
            {
                raw_shape.rectangle => new Rectangle2(bulletShapeParams[0], bulletShapeParams[1], twoDPoint,
                    rotate),
                raw_shape.sector => new Sector(bulletShapeParams[0], bulletShapeParams[1], twoDPoint, rotate),
                _ => throw new ArgumentOutOfRangeException()
            };
            return
                GenDicBulletBox(rawBulletShape);
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

        private static Dictionary<BodySize, BulletBox> GenDicBulletBox(IRawBulletShape rawBulletShape)
        {
            return
                TempConfig.SizeToR.ToDictionary(pair => pair.Key, pair =>
                    new BulletBox(rawBulletShape.GenBulletZone(pair.Value),
                        rawBulletShape.GenBulletShape(pair.Value)));
        }

        public static TwoDVector GenVectorByConfig(Point pt)
        {
            return new TwoDVector(pt.x, pt.y);
        }

        public static IAntiActBuffConfig GenBuffByConfig(push_buff pushBuff)
        {
            var pushType = pushBuff.PushType switch
            {
                game_config.PushType.Vector => PushType.Vector,
                game_config.PushType.Center => PushType.Center,
                _ => throw new ArgumentOutOfRangeException()
            };
            var pushAboutVector = pushBuff.FixVector.Any() ? GenVectorByConfig(pushBuff.FixVector.First()) : null;

            if (pushBuff.UpForce > 0)
            {
                return new PushAirAntiActBuffConfig(pushBuff.PushForce, pushType, pushBuff.UpForce, pushAboutVector,
                    pushBuff.LastTick);
            }

            return new PushEarthAntiActBuffConfig(pushBuff.PushForce, pushType, pushAboutVector, pushBuff.LastTick);
        }


        public static IAntiActBuffConfig GenBuffByConfig(caught_buff caughtBuff)
        {
            var twoDVectors = caughtBuff.CatchPoints.Select(GenVectorByConfig);

            return new CatchAntiActBuffConfig(twoDVectors.ToArray(), caughtBuff.LastTick,
                Skill.GenSkillById(caughtBuff.TrickSkill));
        }
    }
}