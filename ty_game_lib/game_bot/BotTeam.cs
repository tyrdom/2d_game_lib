using System;
using System.Collections.Generic;
using System.Linq;
using cov_path_navi;
using game_stuff;

namespace game_bot
{
    public class BotTeam
    {
        private Dictionary<int, Bot> Bots { get; }

        public BotTeam(Dictionary<int, Bot> bots)
        {
            Bots = bots;
        }

        public BotTeam(Dictionary<int, HashSet<CharInitMsg>> teamToCharInitMsgs, PathTop pathTop)
        {
            var botRadio = new BotRadio();
            var random = new Random(Seed: 3324);
            var valueTuples = teamToCharInitMsgs.ToDictionary(pair => pair.Key, pair =>
            {
                var myTeamIds = pair.Value.Select(x => x.GId);

                var otherTeamIds = teamToCharInitMsgs.Where(x => x.Key != pair.Key)
                    .SelectMany(pair2 => pair2.Value.Select(x => x.GId));
                return (myTeamIds, otherTeamIds);
            });
            var dictionary = teamToCharInitMsgs.SelectMany(aPair =>
            {
                var (myTeamIds, otherTeamIds) = valueTuples[aPair.Key];
                return aPair.Value.Select(x =>
                    new Bot(x, botRadio, pathTop, myTeamIds.ToArray(), otherTeamIds.ToArray(), x.GId, random));
            }).ToDictionary(x => x.MyGid, x => x);

            Bots = dictionary;
        }

        public Dictionary<int, Operate> ReceiveTickMsgGenOperates(
            Dictionary<int, IEnumerable<CharTickMsg>> gidToCharTickMsg) //fast tick
        {
            var dictionary = new Dictionary<int, Operate>();
            foreach (var keyValuePair in gidToCharTickMsg)
            {
                if (!Bots.TryGetValue(keyValuePair.Key, out var bot)) continue;
                var botSimpleTick = bot.BotSimpleTick(keyValuePair.Value);
                if (botSimpleTick != null)
                {
                    dictionary[bot.MyGid] = botSimpleTick;
                }
            }

            return dictionary;
        }


        public void AllBroadCast() //slow tick
        {
            foreach (var keyValuePair in Bots)
            {
                keyValuePair.Value.BroadcastRadio();
            }
        }
    }
}