using System;

namespace game_stuff
{
    public class MathTools
    {
        public static float Min(float a, float b)
        {
#if NETCOREAPP3
            return MathF.Min(a, b);
#else
            return (float) Math.Min(a, b);
#endif
        }

        public static float Max(float a, float b)
        {
#if NETCOREAPP3
            return MathF.Max(a, b);
#else
            return (float) Math.Max(a, b);
#endif
        }

        public static float Sqrt(float a)
        {
#if NETCOREAPP3
            return MathF.Sqrt(a, b);
#else
            return (float) Math.Sqrt(a);
#endif
        }
        public static float Acos(float a)
        {
#if NETCOREAPP3
            return MathF.Sqrt(a, b);
#else
            return (float) Math.Acos(a);
#endif
        }
    }
}