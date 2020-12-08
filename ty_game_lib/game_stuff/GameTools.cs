using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class GameTools
    {
        public static base_attribute GenBaseAttrById(base_attr_id baseAttrId)
        {
            if (!TempConfig.Configs.base_attributes.TryGetValue(baseAttrId, out var baseAttribute))
                throw new ArgumentException($"not such attr{baseAttrId}");
            return baseAttribute;
        }

        public static (int MaxAmmo, float MoveMaxSpeed, float MoveMinSpeed, float MoveAddSpeed, int StandardPropMaxStack
            ,
            float RecycleMulti) GenOtherBaseStatusByAttr(base_attribute baseAttribute)
        {
            return (baseAttribute.MaxAmmo, baseAttribute.MoveMaxSpeed, baseAttribute.MoveMinSpeed,
                baseAttribute.MoveAddSpeed,
                TempConfig.StandardPropMaxStack, baseAttribute.RecycleMulti);
        }

        public static ( float TrapAtkMulti, float TrapSurvivalMulti) GenTrapAttr(base_attribute baseAttribute)
        {
            return (baseAttribute.TrapAtkMulti, baseAttribute.TrapSurvivalMulti);
        }

        public static (SurvivalStatus baseSurvivalStatus, AttackStatus baseAtkStatus) GenStatusByAttr(
            base_attribute baseAttribute, float f = 1f)
        {
            return (SurvivalStatus.GenByConfig(baseAttribute, f), AttackStatus.GenByConfig(baseAttribute));
        }

        public static CharGoTickResult BodyGoATick(IIdPointShape idPointShape, Dictionary<int, Operate> gidToOp)
        {
            return idPointShape switch
            {
                CharacterBody characterBody => characterBody.GoATick(gidToOp),
                _ => throw new ArgumentOutOfRangeException(nameof(idPointShape))
            };
        }

        public static TrapGoTickResult TrapGoATick(IIdPointShape idPointShape)
        {
            return idPointShape switch
            {
                Trap trap => trap.GoATick(),
                _ => throw new ArgumentOutOfRangeException(nameof(idPointShape))
            };
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

        public static bool IsHit(IHitMedia hitMedia, ICanBeHit characterBody)
        {
            var checkAlive = characterBody.CheckCanBeHit();
            var characterBodyBodySize = characterBody.GetSize();
            return checkAlive && hitMedia.SizeToBulletCollision.TryGetValue(characterBodyBodySize, out var bulletBox) &&
                   bulletBox.IsHit(characterBody.GetAnchor(), hitMedia.Pos, hitMedia.Aim);
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

            Zone? z = null;
            var aggregate = enumerable.Aggregate(z, (s, x) => Zone.Join(s, x));
            if (aggregate == null) throw new ArgumentException("bullet have no hit box!!");
            var bulletRdBox = aggregate.Value.GetBulletRdBox();
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


        public static IStunBuffConfig GenBuffByConfig(push_buff pushBuff)
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
                return new PushAirStunBuffConfig(pushBuff.PushForce, pushType, pushBuff.UpForce, pushAboutVector,
                    TempConfig.GetTickByTime(pushBuff.LastTime));
            }

            return new PushEarthStunBuffConfig(pushBuff.PushForce, pushType, pushAboutVector,
                TempConfig.GetTickByTime(pushBuff.LastTime));
        }

        public static List<IMapInteractable> DropWeapon(Dictionary<int, Weapon> weapons, BodySize bodySize,
            TwoDPoint pos)
        {
            var mapInteractable = new List<IMapInteractable>();
            foreach (var keyValuePair in weapons)
            {
                if (!keyValuePair.Value.SkillGroups.TryGetKey(bodySize, out _)) continue;
                var genIMapInteractable = keyValuePair.Value.DropAsIMapInteractable(pos);
                mapInteractable.Add(genIMapInteractable);
                weapons.Remove(keyValuePair.Key);
            }

            return mapInteractable;
        }

        public static IStunBuffConfig GenBuffByConfig(caught_buff caughtBuff)
        {
            var twoDVectors = caughtBuff.CatchKeyPoints
                .ToDictionary(
                    x =>
                    {
                        Console.Out.WriteLine($"time is {TempConfig.GetTickByTime(x.key_time)}");
                        return TempConfig.GetTickByTime(x.key_time);
                    },
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
                return new CatchStunBuffConfig(vectors.ToArray(), TempConfig.GetTickByTime(caughtBuff.LastTime),
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

            return new CatchStunBuffConfig(vectors.ToArray(), TempConfig.GetTickByTime(caughtBuff.LastTime),
                Skill.GenSkillById(caughtBuff.TrickSkill));
        }
    }
}