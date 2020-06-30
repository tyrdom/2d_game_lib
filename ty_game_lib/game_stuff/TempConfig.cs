using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public static class TempConfig
    {
        public static readonly Dictionary<BodySize, float> SizeToR = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 1.5f,
            [BodySize.Medium] = 3f,
            [BodySize.Big] = 4.5f
        };

        public static readonly Dictionary<BodySize, float> SizeToMass = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 2.25f,
            [BodySize.Medium] = 9f,
            [BodySize.Big] = 20.25f
        };


        public const float MaxHeight = 2f;
        public static readonly float MaxUpSpeed = MathTools.Sqrt(2f * G * MaxHeight);
        public const float G = 0.1f;
        public const float Friction = 1f;

        public const int ToughGrowPerTick = 1000;
        public const int MidTough = 10000;
        public const int WeaponNum = 2;
        public const float TwoSToSeePerTick = 10f;
        public static readonly PushOnAir OutCought = new PushOnAir(new TwoDVector(0, 0), 0.1f, 0, 6);

        public const int QSpaceBodyMaxPerLevel = 5;

        public const int HitWallTickParam = 2;
        public const int HitWallCatchTickParam = 10;
        public const int HitWallDmgParam = 2;
        public const float HitWallCatchDmgParam = 5f;

        public const float StandardSightR = 45f;
        public static TwoDVector StandardVector = new TwoDVector(1f, 1.2f);
        public const int StartHp = 1000;
        public static int TestAtk = 10;

        public static int TrickProtect = 100;
        public static int ProtectTick = 10;
    }
}