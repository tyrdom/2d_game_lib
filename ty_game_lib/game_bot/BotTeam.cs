using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        public Dictionary<int, BotOpAndThink> TempOpThinks { get; private set; }

        public BotTeam()
        {
            SimpleBots = new HashSet<SimpleBot>();
            SizeToNaviMap = new Dictionary<size, PathTop>().ToImmutableDictionary();
            TempOpThinks = new Dictionary<int, BotOpAndThink>();
        }

        public void SetNaviMaps(WalkMap walkMap)
        {
            SizeToNaviMap = walkMap.SizeToEdge.ToImmutableDictionary(p => p.Key,
                p => new PathTop(p.Value));
        }

        public void SetNaviMaps(int mapResId)
        {
            var sizeToNaviMap = BotLocalConfig.NaviMapPerLoad.TryGetValue(mapResId, out var immutableDictionary)
                ? immutableDictionary ?? throw new NullReferenceException()
                : throw new KeyNotFoundException();
            SizeToNaviMap = sizeToNaviMap;
        }


        public void SetBots(IEnumerable<(int id, CharacterBody characterBody)> valueTuples, Random random)
        {
            var simpleBots = valueTuples.Select(x =>
                SimpleBot.GenById(x.Item1, x.Item2, random,
                    SizeToNaviMap.TryGetValue(x.Item2.GetSize(), out var top)
                        ? top
                        : throw new KeyNotFoundException()));


            SimpleBots = simpleBots.IeToHashSet();
        }

        public void AllBotsGoATick(PlayGroundGoTickResult perceivable)
        {
            var botOpAndThinks = SimpleBots.ToDictionary(bot => bot.BotBody.GetId(), bot =>
            {
                var key = bot.BotBody.GetId();
                var canBeEnemies = perceivable.PlayerSee.TryGetValue(key, out var enumerable)
                    ? enumerable.OnChange.OfType<ICanBeEnemy>()
                    : new ICanBeEnemy[] { };
                var immutableHashSet = perceivable.CharacterHitSomeThing.TryGetValue(key, out var enumerable1)
                    ? enumerable1
                    : ImmutableHashSet<IHitMsg>.Empty;
                var pathTop = SizeToNaviMap.TryGetValue(bot.BotBody.GetSize(), out var top)
                    ? top
                    : throw new KeyNotFoundException($"size {bot.BotBody.GetSize()}");
                var botSimpleGoATick = bot.BotSimpleGoATick(canBeEnemies,immutableHashSet,
                    pathTop);

#if DEBUG
                Console.Out.WriteLine($" id{key} op move {botSimpleGoATick.Operate?.Move}");
#endif
                return botSimpleGoATick;
            });

            TempOpThinks = botOpAndThinks;
        }

        public PathTop GetNaviMap(size getSize)
        {
            return SizeToNaviMap.TryGetValue(getSize, out var top)
                ? top
                : throw new KeyNotFoundException($"not such size {getSize}");
        }

        public void SetBots(IEnumerable<SimpleBot> valueTuples)
        {
            SimpleBots = valueTuples.IeToHashSet();
        }

        public void ClearBot()
        {
            SimpleBots.Clear();
        }
    }
}