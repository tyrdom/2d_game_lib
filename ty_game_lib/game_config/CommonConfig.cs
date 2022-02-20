using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace game_config
{
    public static class CommonConfig
    {
        public static ConfigDictionaries Configs { get; private set; }


            = new ConfigDictionaries();


        public static other_config? OtherConfig { get; private set; }

        public static string ConfigBinStringPath()
        {
            var upDir = UpDir(Environment.CurrentDirectory, 4);
            var directorySeparatorChar = Path.DirectorySeparatorChar;
            var s =
                $"{upDir}{directorySeparatorChar}game_config{directorySeparatorChar}Resources{directorySeparatorChar}config.txt";
            return s;
        }

        public static string UpDir(string path, int ll)
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;
            var currentDirectory = path;
            for (var i = 0; i < ll; i++)
            {
                var x = currentDirectory.LastIndexOf($"{directorySeparatorChar}", StringComparison.Ordinal);
                currentDirectory = currentDirectory.Substring(0, x);
            }

            return currentDirectory;
        }

        public static (bool isGetOk, T things) GetWeightThings<T>(this ImmutableArray<(int, T)> weightOverList,
            int randV)
        {
            foreach (var valueTuple in weightOverList.Where(valueTuple => valueTuple.Item1 >= randV))
            {
                return (true, valueTuple.Item2);
            }

            return (false, weightOverList.Last().Item2);
        }

        public static (ImmutableArray<(int, T)> weightOverList, int total) GetWeightOverList<T>(
            this IEnumerable<(int, T)> rawWeight)
        {
            var list = new List<(int, T)>();
            var t = 0;
            foreach (var valueTuple in rawWeight)
            {
                var valueTupleItem1 = valueTuple.Item1;
                var valueTupleItem2 = valueTuple.Item2;
                t += valueTupleItem1;
                var nt = t;
                list.Add((nt, valueTupleItem2));
            }

            return (list.ToImmutableArray(), t);
        }

        public static float ValuePerSecToValuePerTick(this float numPerSec)
        {
            if (OtherConfig != null) return numPerSec * OtherConfig.tick_time;
            throw new Exception("config not loaded");
        }


        public static void ReLoadP(ConfigDictionaries configs)
        {
            Configs = configs;
            OtherConfig = configs.other_configs[1];
        }

#if NETCOREAPP
        public static void LoadConfig()
        {
            var configs = new ConfigDictionaries();
            ReLoadP(configs);
        }
#else
        public static void LoadConfig(Dictionary<string, string> jsons)
        {
            var configs = new ConfigDictionaries(jsons);
            ReLoadP(configs);
        }
#endif
        public static void SaveConfigToFile(string path)
        {
            var serializeObj = GameConfigTools.SerializeObj(Configs);
            File.WriteAllText(path, serializeObj, Encoding.UTF8);
        }

        public static void LoadConfigFormFile(string path)
        {
            var readAllText = File.ReadAllText(path, Encoding.UTF8);
            LoadConfigFromString(readAllText);
        }

        public static void LoadConfigFromString(string readAllText)
        {
            var configDictionaries = GameConfigTools.DesObj(readAllText, Configs);
            Configs = configDictionaries;
            OtherConfig = Configs.other_configs[1];
        }
    }
}