#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace game_config
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var configDictionaries = new ConfigDictionaries(ResModel.Dll);
            //并发foreach
            Parallel.ForEach(configDictionaries.all_Immutable_dictionary, dictionary =>

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
            });
        }
    }
}