using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class GameTools
    {
        public static CharGoTickResult BodyGoATick(IIdPointShape idPointShape, Dictionary<int, Operate> gidToOp)
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

        public static Zone GenRdBox(Dictionary<BodySize, BulletBox> bulletBoxes)
        {
            var enumerable = bulletBoxes.Select(x => x.Value.Zone);
            var aggregate = enumerable.Aggregate(new Zone(0, 0, 0, 0), (s, x) => s.Join(x));
            var bulletRdBox = aggregate.GetBulletRdBox();
            return bulletRdBox;
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
                    TempConfig.GetTickByTime(pushBuff.LastTime));
            }

            return new PushEarthAntiActBuffConfig(pushBuff.PushForce, pushType, pushAboutVector,
                TempConfig.GetTickByTime(pushBuff.LastTime));
        }

        public static IAntiActBuffConfig GenBuffByConfig(caught_buff caughtBuff)
        {
            var twoDVectors = caughtBuff.CatchKeyPoints
                .ToDictionary(
                    x => TempConfig.GetTickByTime(x.key_time),
                    x => GenVectorByConfig(x.key_point))
                .Select(pair => (pair.Key, pair.Value))
                .ToList();
            twoDVectors.Sort((x, y) => x.Key.CompareTo(y.Key));

            var vectors = new List<TwoDVector>();

            var (key, value) = twoDVectors.FirstOrDefault();
            if (key != 0 || value == null) throw new Exception($"no good key vectors at caught_buff {caughtBuff.id}");
            vectors.Add(value);
            var nk = key;
            var nowVector = value;
            if (twoDVectors.Count <= 1)
                return new CatchAntiActBuffConfig(vectors.ToArray(), TempConfig.GetTickByTime(caughtBuff.LastTime),
                    Skill.GenSkillById(caughtBuff.TrickSkill));
            for (var index = 1; index < twoDVectors.Count; index++)
            {
                var (k1, v1) = twoDVectors[index];
                if (v1 != null)
                {
                    var genLinearListToAnother = nowVector.GenLinearListToAnother(v1, (int) (k1 - nk));
                    vectors.AddRange(genLinearListToAnother);
                }
                else
                {
                    throw new Exception($"no good config at caught_buff {caughtBuff.id} ");
                }
            }

            return new CatchAntiActBuffConfig(vectors.ToArray(), TempConfig.GetTickByTime(caughtBuff.LastTime),
                Skill.GenSkillById(caughtBuff.TrickSkill));
        }
    }
}