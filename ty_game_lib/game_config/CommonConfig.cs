using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace game_config
{
    public static class CommonConfig
    {
        public static ConfigDictionaries Configs { get; private set; }

#if NETCOREAPP
            = new ConfigDictionaries();
#else
            = new ConfigDictionaries("");
#endif

        public static other_config OtherConfig { get; private set; }

        public static uint GetTickByTime(float time)
        {
            return (uint) Math.Round(time / OtherConfig.tick_time, 0);
        }

        public static int GetIntTickByTime(float time)
        {
            return (int) Math.Round(time / OtherConfig.tick_time, 0);
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
            return numPerSec * OtherConfig.tick_time;
        }

        public static uint ValuePerSecToValuePerTick(this uint numPerSec)
        {
            return (uint) Math.Round(numPerSec * OtherConfig.tick_time, 0);
        }

        public static int ValuePerSecToValuePerTick(this int numPerSec)
        {
            return (int) Math.Round(numPerSec * OtherConfig.tick_time);
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
    }
}