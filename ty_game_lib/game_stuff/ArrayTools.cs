using System.Collections.Generic;
using System.Linq;

namespace game_stuff
{
    public static class ArrayTools
    {
        public static float[] Multiply(uint level, IEnumerable<float> getVector)
        {
            var floats = getVector.Select(x => x * level).ToArray();

            return floats;
        }

        public static float[] Plus(this float[] a, float[] b)
        {
            if (!a.Any()) return b;
            var enumerable = a.Zip(b, (x, y) => x + y).ToArray();
            return enumerable;
        }
    }
}