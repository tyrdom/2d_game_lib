#nullable enable
using System;
using System.IO;
using System.Linq;
using game_config;

namespace lib_test
{
    internal class Program
    {
        public static void Main()
        {
            var dictionary = ResNames.Names.ToDictionary(x => x, x => GetStringByFile(x));

            rogue_game.LocalConfig.LoadConfig(dictionary);
            Console.Out.WriteLine($"~~~~~~{rogue_game.LocalConfig.RogueRebornTick}");

        }

        private static string GetStringByFile(string s)
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;
            var currentDirectory = $"G:{directorySeparatorChar}workspace{directorySeparatorChar}RiderProjects{directorySeparatorChar}2d_game_lib{directorySeparatorChar}ty_game_lib";
            var p = $"{currentDirectory}{directorySeparatorChar}game_config{directorySeparatorChar}Resources{directorySeparatorChar}{s}_s.json";

            Console.Out.WriteLine($"{p}");
            
            
            using StreamReader sr = new StreamReader(p);
            string ss = sr.ReadToEnd();
            return ss;
        }
    }
}