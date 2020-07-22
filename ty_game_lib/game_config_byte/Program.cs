using System;
using System.IO;
using System.Linq;
using game_config;

namespace game_config_byte
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var dictionary in Content.all_Immutable_dictionary)
            {
                var type1 = (from object? key in dictionary.Keys select key?.GetType()).FirstOrDefault();
                var type2 = (from object? key in dictionary.Values select key?.GetType()).FirstOrDefault();
                Console.Out.WriteLine($"{type1}");
                Console.Out.WriteLine($"{type2}");
                //save dictionary
                var methodInfo = typeof(GameConfigTools).GetMethod("SaveDict");
                var makeGenericMethod = methodInfo?.MakeGenericMethod(type1, type2);
                makeGenericMethod?.Invoke(null, new object[] {dictionary});
                Console.Out.WriteLine($"{dictionary.GetType()} saved");
            }
        }
    }
}