using System;

namespace collision_and_rigid
{
    public static class MathTools
    {
        public static float Abs(float a)
        {
#if NETCOREAPP
            return MathF.Abs(a);
#else
            return (float) Math.Abs(a);
#endif
        }

        public static float Min(float a, float b)
        {
#if NETCOREAPP
            return MathF.Min(a, b);
#else
            return (float) Math.Min(a, b);
#endif
        }

        public static float Cos(float a)
        {
#if NETCOREAPP
            return MathF.Cos(a);
#else
            return (float) Math.Cos(a);
#endif
        }

        public static float Sin(float a)
        {
#if NETCOREAPP
            return MathF.Sin(a);
#else
            return (float) Math.Sin(a);
#endif
        }
        public static int Max(int a, int b)
        {
#if NETCOREAPP
            return Math.Max(a, b);
#else
            return  Math.Max(a, b);
#endif
        }
        public static uint Max(uint a, uint b)
        {
#if NETCOREAPP
            return Math.Max(a, b);
#else
            return  Math.Max(a, b);
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
            var min = Min(1, Max(-1, a));
#if NETCOREAPP
            return MathF.Acos(min);
#else
            return (float) Math.Acos(min);
#endif
        }

        public static float Pi()
        {
#if NETCOREAPP
            return MathF.PI;
#else
            return (float) Math.PI;
#endif
        }
    }
}