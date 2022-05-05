using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public static class GameTools
    {
        public static bool TryStringToEnum<T>(this string id, out T eId) where T : struct
        {
            eId = default;
            return Enum.IsDefined(typeof(T), id) && Enum.TryParse(id, out eId);
        }


        public static base_attribute GenBaseAttrById(int baseAttrId)
        {
            if (!CommonConfig.Configs.base_attributes.TryGetValue(baseAttrId, out var baseAttribute))
                throw new ArgumentException($"not such attr{baseAttrId}");
            return baseAttribute;
        }

        public static (int MaxAmmo, float MoveMaxSpeed, float MoveMinSpeed, float MoveAddSpeed, int StandardPropMaxStack
            ,
            float RecycleMulti) GenOtherBaseStatusByAttr(base_attribute baseAttribute)
        {
            return (baseAttribute.MaxAmmo, baseAttribute.MoveMaxSpeed,
                baseAttribute.MoveMinSpeed,
                baseAttribute.MoveAddSpeed,
                CommonConfig.OtherConfig.standard_max_prop_stack, baseAttribute.RecycleMulti);
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


        public static IWaveRaw GenBulletBladeWaveRaw(float[] bulletShapeParams, int bulletLocalRotate,
            Point localPoint, raw_shape rawShape, float baseBladeWaveRange)
        {
            var rotate = DegreeToTwoDVector(bulletLocalRotate);
            var genVectorByConfig = localPoint.GenVectorByConfig();
            var shapeParam = bulletShapeParams[0];
            switch (rawShape)
            {
                case raw_shape.line:

                    var genBulletBladeWavePoint =
                        new TwoDPoint(shapeParam, bulletShapeParams[1]).ClockTurnAboutZero(rotate)
                            .Move(genVectorByConfig);
                    var vector = genBulletBladeWavePoint.Vector().GetUnit2() ?? new TwoDVector(1f, 0f);
                    return new ShootWaveRaw(baseBladeWaveRange, genBulletBladeWavePoint, vector);
                case raw_shape.rectangle:
                    var bladeWavePoint = new TwoDPoint( shapeParam / 2f, 0);
                    var twoDVector = new TwoDVector(0, bulletShapeParams[1] / 2f);
                    var dVector = new TwoDVector(0, -bulletShapeParams[1] / 2f);
                    var twoDPoint = bladeWavePoint.Move(twoDVector).ClockTurnAboutZero(rotate).Move(genVectorByConfig);
                    var dPoint = bladeWavePoint.Move(dVector).ClockTurnAboutZero(rotate).Move(genVectorByConfig);
                    return new RectangleWaveRaw(baseBladeWaveRange, 0f,
                        twoDPoint,
                        dPoint);


                case raw_shape.sector:
                    var y = bulletShapeParams[1];
                    var r = MathTools.Sqrt(shapeParam * shapeParam + y * y);
                    var height = r - shapeParam;
                    var p1 = new TwoDPoint(shapeParam, y).ClockTurnAboutZero(rotate).Move(genVectorByConfig);
                    var p2 = new TwoDPoint(shapeParam, -y).ClockTurnAboutZero(rotate).Move(genVectorByConfig);
                    return new RectangleWaveRaw(baseBladeWaveRange, height, p1, p2);
                case raw_shape.round:
                    var bulletBladeWavePoint = localPoint.GenPointByConfig();
                    return new RoundWaveRaw(baseBladeWaveRange, shapeParam, bulletBladeWavePoint);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rawShape), rawShape, null);
            }
        }

        public static Dictionary<size, BulletBox> GenBulletShapes(float[] bulletShapeParams, int bulletLocalRotate,
            Point localPoint, raw_shape rawShape)
        {
            var twoDPoint = localPoint.GenVectorByConfig();

            return GenBulletShapes(bulletShapeParams, bulletLocalRotate, twoDPoint, rawShape);
        }

        private static Dictionary<size, BulletBox> GenBulletShapes(float[] bulletShapeParams, int bulletLocalRotate,
            TwoDVector twoDPoint, raw_shape rawShape)
        {
            var rotate = DegreeToTwoDVector(bulletLocalRotate);
            IRawBulletShape rawBulletShape = rawShape switch
            {
                raw_shape.rectangle => new Rectangle2(bulletShapeParams[0], bulletShapeParams[1], twoDPoint,
                    rotate),
                raw_shape.sector => new Sector(bulletShapeParams[0], bulletShapeParams[1], twoDPoint, rotate),
                raw_shape.round => new Round(twoDPoint.ToPt(), bulletShapeParams[0]),
                raw_shape.line => new TwoDVectorLine(new TwoDPoint(0, 0).Move(twoDPoint),
                    new TwoDPoint(bulletShapeParams[0], bulletShapeParams[1]).ClockTurnAboutZero(rotate)
                        .Move(twoDPoint)),
                _ => throw new ArgumentOutOfRangeException()
            };
            return
                GenDicBulletBox(rawBulletShape);
        }

        private static TwoDVector DegreeToTwoDVector(int bulletLocalRotate)
        {
            var cos = MathTools.Cos(bulletLocalRotate * 180 / MathTools.Pi());
            var sin = MathTools.Sin(bulletLocalRotate * 180 / MathTools.Pi());
            var rotate = new TwoDVector(cos, sin);
            return rotate;
        }

        public static bool IsHit(this IHitMedia hitMedia, ICanBeAndNeedHit canBeAndNeedHit)
        {
            var checkAlive = canBeAndNeedHit.CheckCanBeHit();
            var characterBodyBodySize = canBeAndNeedHit.GetSize();
            return checkAlive && hitMedia.SizeToBulletCollision.TryGetValue(characterBodyBodySize, out var bulletBox) &&
                   bulletBox.IsHit(canBeAndNeedHit.GetAnchor(), hitMedia.Pos, hitMedia.Aim);
        }


        public static float GetMaxUpSpeed(float? height)
        {
            if (height == null)
            {
                return StuffLocalConfig.MaxUpSpeed;
            }

            var maxHeight = CommonConfig.OtherConfig.max_hegiht - height.Value;
            var f = 2f * CommonConfig.OtherConfig.g_acc * maxHeight;
            return MathTools.Sqrt(f);
        }

        public static Zone GenRdBox(Dictionary<size, BulletBox> bulletBoxes)
        {
            var enumerable = bulletBoxes.Select(x => x.Value.Zone);

            Zone? z = null;
            var aggregate = enumerable.Aggregate(z, (s, x) => Zone.Join(s, x));
            if (aggregate == null) throw new ArgumentException("bullet have no hit box!!");
            var bulletRdBox = aggregate.Value.GetBulletRdBox();
            return bulletRdBox;
        }

        public static Dictionary<size, BulletBox> GenDicBulletBox(this IRawBulletShape rawBulletShape)
        {
            return
                CommonConfig.Configs.bodys.ToDictionary(pair => pair.Key, pair =>
                    new BulletBox(rawBulletShape.GenBulletZone(pair.Value.rad),
                        rawBulletShape.GenBulletShape(pair.Value.rad)));
        }

        public static TwoDVector GenVectorByConfig(this Point pt)
        {
            return new TwoDVector(pt.x, pt.y);
        }

        private static TwoDPoint GenPointByConfig(this Point pt)
        {
            return new TwoDPoint(pt.x, pt.y);
        }

        public static List<IMapInteractable> DropWeapon(Dictionary<int, Weapon> weapons, size bodySize,
            TwoDPoint pos)
        {
            var mapInteractable = new List<IMapInteractable>();
            foreach (var keyValuePair in weapons)
            {
                if (!keyValuePair.Value.SkillGroups.ContainsKey(bodySize)) continue;
                var genIMapInteractable = keyValuePair.Value.DropAsIMapInteractable(pos);
                mapInteractable.Add(genIMapInteractable);
                weapons.Remove(keyValuePair.Key);
            }

            return mapInteractable;
        }
    }
}