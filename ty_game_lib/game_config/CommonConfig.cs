using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace game_config
{
    public static class CommonConfig
    {
        
        
        public static int BattleExpId { get; private set; } = 1;
        public static ConfigDictionaries Configs { get; private set; }
#if NETCOREAPP
            = new ConfigDictionaries();
#else
            = new ConfigDictionaries("");
#endif
        public static uint GetTickByTime(float time)
        {
            return (uint) (time * TickPerSec);
        }

        public static int GetIntTickByTime(float time)
        {
            return (int) (time * TickPerSec);
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

        public static float NumPerSecToTickPerSec(float numPerSec)
        {
            return numPerSec / TickPerSec;
        }

        public static uint NumPerSecToTickPerSec(uint numPerSec)
        {
            return (uint) (numPerSec / TickPerSec);
        }

        public static int NumPerSecToTickPerSec(int numPerSec)
        {
            return numPerSec / TickPerSec;
        }


        private static int TickPerSec { get; set; } = 10;
        public static void ReLoadP(ConfigDictionaries configs)
        {
            Configs = configs;
            var configsOtherConfig = configs.other_configs[1];
            TickPerSec = configsOtherConfig.tick_per_sec;
            BattleExpId = configs.items.Values.First(x => x.ItemType == ItemType.battle_exp).id;
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