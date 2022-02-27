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
        // private HashSet<SimpleBot> SimpleBots { get; set; }
        //
        // public Dictionary<int, BotOpAndThink> TempOpThinks { get; private set; }

        public HashSet<NormalBehaviorBot> NormalBehaviorBots { get; set; }

        public Dictionary<int, Operate?> TempOperate { get; private set; }

        public BotTeam()
        {
            NormalBehaviorBots = new HashSet<NormalBehaviorBot>();
            SizeToNaviMap = new Dictionary<size, PathTop>().ToImmutableDictionary();
            TempOperate = new Dictionary<int, Operate?>();
        }

      

        public void SetNaviMaps(int mapResId)
        {
            var sizeToNaviMap = BotLocalConfig.NaviMapPerLoad.TryGetValue(mapResId, out var immutableDictionary)
                ? immutableDictionary ?? throw new NullReferenceException()
                : throw new KeyNotFoundException($"key {mapResId}");
            SizeToNaviMap = sizeToNaviMap;
        }


        public void SetBots(IEnumerable<(int id, CharacterBody characterBody)> valueTuples, Random random)
        {
            var simpleBots = valueTuples.Select(x =>
                NormalBehaviorBot.GenById(x.Item1, x.Item2,
                    SizeToNaviMap.TryGetValue(x.Item2.GetSize(), out var top)
                        ? top
                        : throw new KeyNotFoundException()));


            NormalBehaviorBots = simpleBots.IeToHashSet();
        }

        public void AllBotsGoATick(PlayGroundGoTickResult perceivable)
        {
            var botOpAndThinks = NormalBehaviorBots.ToDictionary(bot => bot.LocalBehaviorTreeBotAgent.BotBody.GetId(),
                bot =>
                {
                    var key = bot.LocalBehaviorTreeBotAgent.BotBody.GetId();
                    var canBeEnemies = perceivable.PlayerSee.TryGetValue(key, out var enumerable)
                        ? enumerable
                        : PlayerTickSense.Empty;

                    var immutableHashSet = perceivable.CharacterHitSomeThing.TryGetValue(key, out var enumerable1)
                        ? enumerable1
                        : ImmutableHashSet<IHitMsg>.Empty;
                    var bulletHits =
                        perceivable.CharacterGidBeHit.TryGetValue(key, out var beHit)
                            ? beHit.OfType<BulletHit>().ToImmutableHashSet()
                            : ImmutableHashSet<BulletHit>.Empty;

                    var pathTop =
                        SizeToNaviMap.TryGetValue(bot.LocalBehaviorTreeBotAgent.BotBody.GetSize(), out var top)
                            ? top
                            : throw new KeyNotFoundException($"size {bot.LocalBehaviorTreeBotAgent.BotBody.GetSize()}");
                    var botSimpleGoATick = bot.GoATick(canBeEnemies, immutableHashSet,
                        pathTop);

#if DEBUG
                    Console.Out.WriteLine($" id{key} op move {botSimpleGoATick}");
#endif
                    return botSimpleGoATick;
                });

            TempOperate = botOpAndThinks;
        }

        public PathTop GetNaviMap(size getSize)
        {
            return SizeToNaviMap.TryGetValue(getSize, out var top)
                ? top
                : throw new KeyNotFoundException($"not such size {getSize}");
        }

        public void SetBots(IEnumerable<NormalBehaviorBot> valueTuples)
        {
            NormalBehaviorBots = valueTuples.IeToHashSet();
        }

        public void ClearBot()
        {
            NormalBehaviorBots.Clear();
        }
    }
}