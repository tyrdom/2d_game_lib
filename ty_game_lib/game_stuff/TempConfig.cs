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


        public static float MaxHeight = 2f;
        public static float MaxUp = MathF.Sqrt(2f * G * MaxHeight);
        public static float G = 10f;
        public static readonly float Friction = 1f;

        public static int ToughGrowPerTick = 1000;
        public static int MidTough = 10000;

        public static PushOnAir OutCought = new PushOnAir(new TwoDVector(0, 0), 0.1f, 0, 8);
    }
}