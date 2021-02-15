using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Schema;
using collision_and_rigid;
using cov_path_navi;
using game_config;
using game_stuff;

namespace game_bot
{
    public class BotTeam
    {
        private ImmutableDictionary<size, PathTop> SizeToNaviMap { get; set; }
        private HashSet<SimpleBot> SimpleBots { get; set; }

        public Dictionary<int, BotOpAndThink> TempOpThinks { get; set; }

        public BotTeam()
        {
            SimpleBots = new HashSet<SimpleBot>();
            SizeToNaviMap = new Dictionary<size, PathTop>().ToImmutableDictionary();
        }

        public void SetNaviMaps(WalkMap walkMap)
        {
            SizeToNaviMap = walkMap.SizeToEdge.ToImmutableDictionary(p => p.Key,
                p => new PathTop(p.Value));
        }

        public void SetNaviMaps(int mapResId)
        {
            var sizeToNaviMap = LocalConfig.NaviMapPerLoad.TryGetValue(mapResId, out var immutableDictionary)
                ? immutableDictionary ?? throw new NullReferenceException()
                : throw new KeyNotFoundException();
            SizeToNaviMap = sizeToNaviMap;
        }


        public void SetBots(IEnumerable<(int, CharacterBody)> valueTuples, Random random)
        {
            var simpleBots = valueTuples.Select(x =>
                SimpleBot.GenById(x.Item1, x.Item2, random,
                    SizeToNaviMap.TryGetValue(x.Item2.GetSize(), out var top)
                        ? top
                        : throw new KeyNotFoundException()));
            SimpleBots = simpleBots.IeToHashSet();
        }

        public void AllBotsGoATick(ImmutableDictionary<int, ImmutableHashSet<IPerceivable>> perceivable)
        {
            var botOpAndThinks = SimpleBots.ToDictionary(x => x.BotBody.GetId(), x => x.BotSimpleGoATick(
                perceivable.TryGetValue(x.BotBody.GetId(), out var enumerable)
                    ? enumerable.OfType<ICanBeEnemy>()
                    : throw new KeyNotFoundException(),
                SizeToNaviMap.TryGetValue(x.BotBody.GetSize(), out var top) ? top : throw new KeyNotFoundException()
            ));

            TempOpThinks = botOpAndThinks;
        }
    }
}