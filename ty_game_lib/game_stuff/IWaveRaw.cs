using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface IWaveRaw
    {
        float BaseRange { get; }

        bool GenRawBulletShape(float range, out IRawBulletShape rawBulletShape);
    }

    public class ShootWaveRaw : IWaveRaw
    {
        public ShootWaveRaw(float baseRange, TwoDPoint o, TwoDVector unit)
        {
            BaseRange = baseRange;
            O = o;
            Unit = unit;
        }

        public float BaseRange { get; }

        private TwoDPoint O { get; }

        private TwoDVector Unit { get; }

        public bool GenRawBulletShape(float range, out IRawBulletShape rawBulletShape)
        {
            var r = BaseRange + range;
            if (r > 0)
            {
                var twoDPoint = O.Move(Unit.Multi(r));
                rawBulletShape = new TwoDVectorLine(O, twoDPoint);
                return true;
            }

            rawBulletShape = new Round(TwoDPoint.Zero(), 0f);
            return false;
        }
    }

    public class RectangleWaveRaw : IWaveRaw
    {
        public RectangleWaveRaw(float baseRange, float fixRange, TwoDPoint p1, TwoDPoint p2)
        {
            BaseRange = baseRange;
            FixRange = fixRange;
            P1 = p1;
            P2 = p2;
        }

        public float BaseRange { get; }

        public bool GenRawBulletShape(float range, out IRawBulletShape rawBulletShape)
        {
            var r = BaseRange + range;
            if (r > 0)
            {
                rawBulletShape = new Rectangle(P2, P1, FixRange + r);
                return true;
            }

            rawBulletShape = new Round(TwoDPoint.Zero(), 0);
            return false;
        }

        private float FixRange { get; }

        private TwoDPoint P1 { get; }

        private TwoDPoint P2 { get; }
    }

    public class RoundWaveRaw : IWaveRaw
    {
        public RoundWaveRaw(float baseRange, float r, TwoDPoint pointO)
        {
            BaseRange = baseRange;
            R = r;
            PointO = pointO;
        }

        public float BaseRange { get; }

        public bool GenRawBulletShape(float range, out IRawBulletShape rawBulletShape)
        {
            var r = BaseRange + range;
            if (r > 0)
            {
                rawBulletShape = new Round(PointO, r + R);
                return true;
            }

            rawBulletShape = new Round(TwoDPoint.Zero(), 0);
            return false;
        }

        private float R { get; }

        private TwoDPoint PointO { get; }
    }
}