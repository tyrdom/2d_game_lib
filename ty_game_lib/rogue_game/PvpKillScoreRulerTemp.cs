

// namespace rogue_game
// {
    // public class PvpKillScoreRuler : IPlayRuler
    // {   
    //     private Dictionary<int, RebornUnit> GidToRebornPool { get; }
    //
    //     private ImmutableDictionary<int, TwoDPoint> TeamToRebornPt { get; }
    //
    //     public PvpKillScoreRuler(int scoreReach, ImmutableDictionary<int, TwoDPoint> teamToRebornPt)
    //     {
    //         ScoreReach = scoreReach;
    //         TeamToRebornPt = teamToRebornPt;
    //         GidToRebornPool = new Dictionary<int, RebornUnit>();
    //         TeamScores = new Dictionary<int, int>();
    //     }
    //
    //     private int ScoreReach { get; }
    //     private Dictionary<int, int> TeamScores { get; }
    //
    //     private void AddBody(CharacterBody characterBody)
    //     {
    //         var id = characterBody.GetId();
    //         if (GidToRebornPool.TryGetValue(id, out var rebornUnit))
    //         {
    //             rebornUnit.SetRebornTime();
    //         }
    //         else
    //         {
    //             GidToRebornPool[id] = new RebornUnit(characterBody, RebornUnit.GenRebornTick(characterBody),
    //                 RebornUnit.GetFastRebItems(characterBody));
    //         }
    //     }
    //
    //     public IRuleTickResult RulerGoTick(PlayGround playGround)
    //     {
    //         RebornPoolGoTick();
    //
    //         var playGroundGidToBody = playGround.GidToBody;
    //         var valueTuples = playGroundGidToBody
    //             .Select(x => (x.Value.Team, x.Value.CharacterStatus.CharRuleData.NowKills, x.Key))
    //             .Where(xx => xx.NowKills.Any());
    //         var enumerable = valueTuples.ToList();
    //         var dictionary = enumerable.ToDictionary(k => k.Key,
    //             k => k.NowKills.Select(dd => dd.GetId()).ToList().ToImmutableHashSet());
    //         foreach (var valueTuple in enumerable.GroupBy(x => x.Team))
    //         {
    //             var sum = valueTuple.Sum(x => x.NowKills.Count);
    //             foreach (var tuple in valueTuple)
    //             {
    //                 foreach (var tupleNowKill in tuple.NowKills)
    //                 {
    //                     AddBody(tupleNowKill);
    //                 }
    //             }
    //
    //             var valueTupleKey = valueTuple.Key;
    //             if (TeamScores.TryGetValue(valueTupleKey, out var score))
    //             {
    //                 var teamScore = score + sum;
    //                 if (teamScore >= ScoreReach)
    //                 {
    //                     return new PvPResult(valueTupleKey, true, dictionary);
    //                 }
    //
    //                 TeamScores[valueTupleKey] = teamScore;
    //             }
    //             else
    //             {
    //                 if (sum >= ScoreReach)
    //                 {
    //                     return new PvPResult(valueTupleKey, true, dictionary);
    //                 }
    //
    //                 TeamScores[valueTupleKey] = sum;
    //             }
    //         }
    //
    //         return new PvPResult(null, false, dictionary);
    //     }
    //
    //     private void RebornPoolGoTick()
    //     {
    //         var characterBodies = new List<CharacterBody>();
    //         foreach (var tickRebornUnit in GidToRebornPool.Values)
    //         {
    //             tickRebornUnit.GoATick();
    //             var canReborn = tickRebornUnit.RebornAboutTickFinish();
    //             if (canReborn)
    //             {
    //                 characterBodies.Add(tickRebornUnit.CharacterBody);
    //             }
    //         }
    //
    //         foreach (var grouping in characterBodies.GroupBy(c => c.Team))
    //         {
    //             var twoDPoint = TeamToRebornPt.TryGetValue(grouping.Key, out var pt)
    //                 ? pt
    //                 : throw new DirectoryNotFoundException($"not reborn pt {grouping.Key}");
    //             foreach (var characterBody in grouping)
    //             {
    //                 characterBody.ReBorn(twoDPoint);
    //             }
    //         }
    //     }
    //
    //     private static ImmutableDictionary<int, LevelUpsData> GetLevelUpData()
    //     {
    //         return TempConfig.Configs.standard_level_ups.ToImmutableDictionary(p => p.Key,
    //             pair =>
    //             {
    //                 var standardLevelUp = pair.Value;
    //                 var a = standardLevelUp.next_exp <= 0 ? (int?) null : standardLevelUp.next_exp;
    //                 var tickByTime = TempConfig.GetTickByTime(standardLevelUp.reborn_time);
    //                 var fastReborn = standardLevelUp.fastReborn.Select(GameItem.GenByConfigGain).ToArray();
    //                 return new LevelUpsData(a, (int) tickByTime, fastReborn, standardLevelUp.up_passive_get);
    //             }
    //         );
    //     }
    //
    //     public LevelUps GetLevelUp()
    //     {
    //         return new LevelUps(GetLevelUpData());
    //     }
    // }
// }