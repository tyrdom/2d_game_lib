using System;
using System.Collections.Generic;
using game_config;

namespace rogue_chapter_maker
{
    public static class MakerTools
    {
        public static Random Random { get; } = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            list.Shuffle(Random);
        }
    }
}