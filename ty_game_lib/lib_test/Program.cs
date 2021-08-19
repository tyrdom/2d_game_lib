#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_bot;
using game_config;
using game_stuff;
using rogue_game;

namespace lib_test
{
    internal static class Program
    {
        public static void Main()
        {
            var dictionary = ResNames.Names.ToDictionary(x => x, GetStringByFile);

            RogueLocalConfig.LoadConfig(dictionary);
#if DEBUG
            Console.Out.WriteLine($"~~~~~~{RogueLocalConfig.RogueRebornTick}");
#endif


            var genPlayerByConfig =
                CharacterInitData.GenPlayerByConfig(1, 0, new[] {weapon_id.test_spear, weapon_id.test_cross_bow},
                    size.small,
                    1, new Dictionary<passive_id, uint>
                    {
                        {passive_id.revenge, 2}, {passive_id.absorb_up, 1}, {passive_id.on_break, 1}
                    });

            var characterInitData =
                CharacterInitData.GenPlayerByConfig(2, 1, new[] {weapon_id.test_sword}, size.small, 1,
                    new Dictionary<passive_id, uint>
                    {
                        {passive_id.main_atk, 10}, {passive_id.shield_overload, 3}, {passive_id.energy_armor, 1}
                    });
            var characterInitData2 =
                CharacterInitData.GenPlayerByConfig(3, 1, new[] {weapon_id.test_sword}, size.small, 1);
            var characterInitData3 =
                CharacterInitData.GenPlayerByConfig(4, 1, new[] {weapon_id.test_sword}, size.small, 1);

            var characterInitData4 =
                CharacterInitData.GenPlayerByConfig(5, 1, new[] {weapon_id.test_sword}, size.small, 1);

            // var genCharacterBody = genPlayerByConfig.GenCharacterBody(TwoDPoint.Zero());
            // var characterBody = characterInitData.GenCharacterBody(TwoDPoint.Zero());
            // var body = characterInitData2.GenCharacterBody(TwoDPoint.Zero());
            // var body1 = characterInitData3.GenCharacterBody(TwoDPoint.Zero());
            // var body2 = characterInitData4.GenCharacterBody(TwoDPoint.Zero());
            var genByConfig =
                RogueGame.GenByConfig(new HashSet<CharacterInitData> {genPlayerByConfig, characterInitData},
                    1).genByConfig;

            var playerCharacterStatus = genByConfig.NowGamePlayers[1].Player.CharacterStatus;
            var playerCharacterStatus2 = genByConfig.NowGamePlayers[2].Player.CharacterStatus;
            playerCharacterStatus.PickAProp(Prop.GenById(prop_id.trap));
            playerCharacterStatus.SetPropPoint(100);
            playerCharacterStatus2.PickAProp(Prop.GenById(prop_id.alert));
            playerCharacterStatus2.SetPropPoint(100);
            // genByConfig.ForceSpawnNpc();
#if DEBUG
            var mapApplyDevices = genByConfig.NowPlayMap.PlayGround.GetMapApplyDevices();
            var any = mapApplyDevices.Any(x => x.IsActive);
            // Console.Out.WriteLine($"~~~!!~~~{any}~~!!~~{mapApplyDevices.Count}");
#endif
            // var genById = SimpleBot.GenById(1, characterBody, new Random(), null);
            // var passiveTrait = PassiveTrait.GenById(passive_id.main_atk, 1);
            // genCharacterBody.CharacterStatus.FullAmmo();

            for (var i = 0; i < 300; i++)
            {
                var twoDVector = new TwoDVector(0, 1f);

                var dVector = new TwoDVector(1f, 0f);
                var operate = i < 59
                    ? new Operate(aim: dVector, skillAction: SkillAction.Op2)
                    : new Operate(aim: dVector, skillAction: SkillAction.Op1);
                var dVector1 = new TwoDVector(0, 1f);
                var operate1 = new Operate(aim: dVector1, specialAction: SpecialAction.UseProp);
                var opDic = new Dictionary<int, Operate>
                    {{1, operate}, {2, operate1}};
                var rogueGameGoTickResult = genByConfig.GamePlayGoATick(opDic);

                var (beHit, trapBeHit,
                        playerSee, playerTeleportTo, hitSomething) =
                    rogueGameGoTickResult.PlayGroundGoTickResult;
                var canBeEnemies = playerSee.TryGetValue(2, out var enumerable)
                    ? enumerable.OnChange.OfType<ICanBeEnemy>()
                    : new ICanBeEnemy[] { };
                var immutableHashSet = hitSomething.TryGetValue(2, out var enumerable1)
                    ? enumerable1
                    : ImmutableHashSet<IHitMsg>.Empty;
                // genById.BotSimpleGoATick(canBeEnemies, immutableHashSet, null);
                var mapChange = rogueGameGoTickResult.MapChange;
                if (i % 5 == 0)
                {
                    var gameRequests = new Dictionary<int, IGameRequest>();
#if DEBUG
                    if (i % 30 == 0)
                    {
                        // gameRequests[1] = new ResetGame();
                    }
#endif
                    var gameConsoleGoATick = genByConfig.GameConsoleGoATick(gameRequests);
                    var goNextChapters = gameConsoleGoATick.OfType<PushChapterGoNext>().ToArray();
                    if (goNextChapters.Any())
                    {
                        var pushChapterGoNext = goNextChapters.First();
                        Console.Out.WriteLine($"go next {pushChapterGoNext.YSlotArray.Length}");
                    }
                }

#if DEBUG
                Console.Out.WriteLine("");
#endif
                var characterBodies = playerSee.TryGetValue(1, out var tickSee)
                    ? tickSee.OnChange.OfType<CharacterBody>()
                    : new CharacterBody[] { };
                var firstOrDefault = characterBodies.FirstOrDefault(x => x.GetId() == 1);

                var genTickMsg = (CharTickMsg?) (firstOrDefault?.GenTickMsg() ?? null);

                var twoDPoint = firstOrDefault?.GetAnchor();
#if DEBUG
                Console.Out.WriteLine($"~~~~now on tick {i}");
                if (mapChange)
                {
                    Console.Out.WriteLine($"map change to {genByConfig.NowPlayMap.PlayGround.MgId}");
                }

                if (firstOrDefault != null)
                {
                    Console.Out.WriteLine($"id 5 pos see is {twoDPoint}");
                }

                var theta = genTickMsg?.CharEvents.OfType<StartAct>().FirstOrDefault();
                if (theta != null)
                {
                    Console.Out.WriteLine($"act is {theta.TypeEnum} {theta.IntId}");
                    if (theta.TypeEnum == action_type.skill)
                    {
                        var thetaIntId = (skill_id) theta.IntId;
                        Console.Out.WriteLine($"{thetaIntId}");
                    }
                }
#endif
            }

            var rogueGameSave = genByConfig.Save();


            var rogueGame = rogueGameSave.Load(1);

            for (int i = 0; i < 100; i++)
            {
                var twoDVector = new TwoDVector(0, 1f);

                var dVector = new TwoDVector(1f, 0f);
                var operate = i < 99
                    ? new Operate(aim: dVector, SkillAction.Op1)
                    : new Operate(aim: dVector, move: dVector);
                var dVector1 = new TwoDVector(0, 1f);
                var operate1 = new Operate(aim: dVector1);
                var opDic = new Dictionary<int, Operate>()
                    {{1, operate}, {2, operate1}};
                var rogueGameGoTickResult = rogueGame.GamePlayGoATick(opDic);
            }
        }

        private static string GetStringByFile(string s)
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;
            var currentDirectory = Environment.CurrentDirectory;
            for (var i = 0; i < 4; i++)
            {
                var x = currentDirectory.LastIndexOf("\\", StringComparison.Ordinal);
                currentDirectory = currentDirectory.Substring(0, x);
            }

            //
            //
            // var currentDirectory = osVersionPlatform == PlatformID.MacOSX || osVersionPlatform == PlatformID.Unix
            //     ? $"{directorySeparatorChar}Users{directorySeparatorChar}tianhao{directorySeparatorChar}Documents{directorySeparatorChar}ty_game{directorySeparatorChar}ty_game_lib"
            //     : $"G:{directorySeparatorChar}workspace{directorySeparatorChar}RiderProjects{directorySeparatorChar}2d_game_lib{directorySeparatorChar}ty_game_lib";
            var p =
                $"{currentDirectory}{directorySeparatorChar}game_config{directorySeparatorChar}Resources{directorySeparatorChar}{s}_s.json";

            Console.Out.WriteLine($"{p}");


            using StreamReader sr = new StreamReader(p);
            string ss = sr.ReadToEnd();
            return ss;
        }
    }
}