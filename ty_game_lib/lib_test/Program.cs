#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
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
            var dictionary = ResNames.Names.ToDictionary(x => x, x => GetStringByFile(x, 4));

            var configBinStringPath = CommonConfig.ConfigBinStringPath();


#if NET6_0
            CommonConfig.LoadConfig();
#else
            CommonConfig.LoadConfig(dictionary);
#endif

#if DEBUG
            Console.Out.WriteLine($"~~~~~~{RogueLocalConfig.RogueRebornTick}");
#endif


            CommonConfig.SaveConfigToFile(configBinStringPath);

            RogueLocalConfig.ReLoadP();

            var genPlayerByConfig =
                CharacterInitData.GenPlayerByConfig(1, 0, new[] {weapon_id.test_sword, weapon_id.test_dual_swords},
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
                    1, out var gameRespS);

            var playerCharacterStatus = genByConfig.NowGamePlayers[1].Player.CharacterStatus;
            var playerCharacterStatus2 = genByConfig.NowGamePlayers[2].Player.CharacterStatus;
            // playerCharacterStatus.PickAPassive(PassiveTrait.GenById(passive_id.blade_wave,1));
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
                    ? enumerable.OnChangingBodyAndRadarSee.OfType<ICanBeEnemy>()
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
                    ? tickSee.OnChangingBodyAndRadarSee.OfType<CharacterBody>()
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
                        var thetaIntId = (skill_id)theta.IntId;
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

            CopyFiles();
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, bool overwrite)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            var dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (var file in dir.GetFiles())
            {
                var targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (!recursive) return;
            foreach (var subDir in dirs)
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true, overwrite);
            }
        }

        public static string DotNetStandard20BinDir()
        {
            var upDir = CommonConfig.UpDir(Environment.CurrentDirectory, 4);
            var directorySeparatorChar = Path.DirectorySeparatorChar;
            var s =
                $"{upDir}{directorySeparatorChar}rogue_game{directorySeparatorChar}bin{directorySeparatorChar}Release{directorySeparatorChar}netstandard2.0{directorySeparatorChar}";
            return s;
        }

        public static string ReadTargetAssetDir()
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;
            var upDir = CommonConfig.UpDir(Environment.CurrentDirectory, 4);
            var path = upDir + directorySeparatorChar + "lib_test" + directorySeparatorChar + "TargetDir.txt";
            var readAllText = File.ReadAllText(path, Encoding.UTF8);
            var replace = readAllText.Replace('\\', directorySeparatorChar);
            return replace;
        }

        public static void CopyFiles()
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;

            var readTargetAssetDir = ReadTargetAssetDir();

            var binDir = readTargetAssetDir + directorySeparatorChar + "Plugins" + directorySeparatorChar;

            var configTxt = readTargetAssetDir + directorySeparatorChar + "Resources" + directorySeparatorChar +
                            "ConfigJsons" + directorySeparatorChar + "config.txt";

            var configBinStringPath = new FileInfo(CommonConfig.ConfigBinStringPath());
            configBinStringPath.CopyTo(configTxt, true);
            var dotNetStandard20BinDir = DotNetStandard20BinDir();
            CopyDirectory(dotNetStandard20BinDir, binDir, false, true);
            
        }


        private static string GetStringByFile(string s, int ll)
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;

            var upDir = CommonConfig.UpDir(Environment.CurrentDirectory, ll);

            //
            //
            // var currentDirectory = osVersionPlatform == PlatformID.MacOSX || osVersionPlatform == PlatformID.Unix
            //     ? $"{directorySeparatorChar}Users{directorySeparatorChar}tianhao{directorySeparatorChar}Documents{directorySeparatorChar}ty_game{directorySeparatorChar}ty_game_lib"
            //     : $"G:{directorySeparatorChar}workspace{directorySeparatorChar}RiderProjects{directorySeparatorChar}2d_game_lib{directorySeparatorChar}ty_game_lib";
            var p =
                $"{upDir}{directorySeparatorChar}game_config{directorySeparatorChar}Resources{directorySeparatorChar}{s}_s.json";

            Console.Out.WriteLine($"{p}");


            using var sr = new StreamReader(p);
            var ss = sr.ReadToEnd();
            return ss;
        }
    }
}