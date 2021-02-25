﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace game_config
{
    public static class RandTools
    {
        public static T ChooseRandOne<T>(this IList<T> array, Random random)
        {
            return (array.Any()
                ? array.Count > 1
                    ? array[random.Next(array.Count)]
                    : array[0]
                : default)!;
        }

        public static IEnumerable<T> ChooseRandCanSame<T>(this T[] array, int num, Random random)
        {
            var enumerable = Enumerable.Range(0, num);
            return array.Any()
                ? array.Length > 1
                    ? enumerable.Select(x => array[random.Next(array.Length)])
                    : enumerable.Select(x => array[0])
                : throw new IndexOutOfRangeException();
        }

        public static IEnumerable<T> ChooseRandDif<T>(this T[] array, int num, Random random)
        {
            array.Shuffle(random);
            return array.Take(num);
        }

        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}