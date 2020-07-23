using System;

namespace collision_and_rigid
{
    public class MathTools
    {
        public static float Min(float a, float b)
        {
#if NETCOREAPP
            return MathF.Min(a, b);
#else
            return (float) Math.Min(a, b);
#endif
        }

        public static float Max(float a, float b)
        {
#if NETCOREAPP
            return MathF.Max(a, b);
#else
            return (float) Math.Max(a, b);
#endif
        }

        public static float Sqrt(float a)
        {
#if NETCOREAPP
            return MathF.Sqrt(a);
#else
            return (float) Math.Sqrt(a);
#endif
        }
        public static float Acos(float a)
        {
#if NETCOREAPP
            return MathF.Acos(a);
#else
            return (float) Math.Acos(a);
#endif
        }
    }
}