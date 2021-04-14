#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;
using game_stuff;
using rogue_game;

namespace lib_test
{
    internal class Program
    {
        public static void Main()
        {
            var dictionary = ResNames.Names.ToDictionary(x => x, GetStringByFile);

            RogueLocalConfig.LoadConfig(dictionary);
#if DEBUG
            Console.Out.WriteLine($"~~~~~~{RogueLocalConfig.RogueRebornTick}");
#endif


            var genPlayerByConfig = CharacterInitData.GenPlayerByConfig(1, 0, new[] {2}, size.small, 1);
            var characterInitData = CharacterInitData.GenPlayerByConfig(2, 1, new[] {1}, size.small, 1);
            var characterInitData2 = CharacterInitData.GenPlayerByConfig(3, 1, new[] {1}, size.small, 1);
            var genCharacterBody = genPlayerByConfig.GenCharacterBody(TwoDPoint.Zero());
            var characterBody = characterInitData.GenCharacterBody(TwoDPoint.Zero());
            var body = characterInitData2.GenCharacterBody(TwoDPoint.Zero());
            var genByConfig =
                RogueGame.GenByConfig(new HashSet<CharacterBody> {genCharacterBody, characterBody, body},
                    genCharacterBody);

#if DEBUG
            var mapApplyDevices = genByConfig.NowPlayMap.PlayGround.GetMapApplyDevices();
            var any = mapApplyDevices.Any(x => x.IsActive);
            // Console.Out.WriteLine($"~~~!!~~~{any}~~!!~~{mapApplyDevices.Count}");
#endif
            for (var i = 0; i < 300; i++)
            {
                var twoDVector = new TwoDVector(1f, 0);
                var operate = i < 100
                    ? new Operate(aim: twoDVector, snipeAction: SnipeAction.SnipeOn1, skillAction: SkillAction.Op1)
                    : new Operate(aim: twoDVector, snipeAction: SnipeAction.SnipeOff);
                var rogueGameGoTickResult = genByConfig.GamePlayGoATick(new Dictionary<int, Operate>() {{1, operate}});
                var (playerBeHit, trapBeHit, playerSee, playerTeleportTo) =
                    rogueGameGoTickResult.PlayGroundGoTickResult;
                var mapChange = rogueGameGoTickResult.MapChange;
                if (i % 5 == 0)
                {
                    genByConfig.GameConsoleGoATick(new Dictionary<int, IGameRequest>());
                }

                var characterBodies = playerSee[1].OfType<CharacterBody>();
                var firstOrDefault = characterBodies.FirstOrDefault();
                var genTickMsg = (CharTickMsg) firstOrDefault.GenTickMsg();
                var twoDPoint = firstOrDefault?.GetAnchor();
#if DEBUG
                Console.Out.WriteLine($"~~~~now on tick {i}");
                if (mapChange)
                {
                    Console.Out.WriteLine($"map change to {genByConfig.NowPlayMap.PlayGround.MgId}");
                }

                Console.Out.WriteLine($"$ ~~~~~{twoDPoint} {genTickMsg.Gid} skill launch :{genTickMsg.SkillLaunch}");
#endif
            }
        }

        private static string GetStringByFile(string s)
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;
            var osVersionPlatform = Environment.OSVersion.Platform;

            var currentDirectory = osVersionPlatform == PlatformID.MacOSX || osVersionPlatform == PlatformID.Unix
                ? $"{directorySeparatorChar}Users{directorySeparatorChar}tianhao{directorySeparatorChar}Documents{directorySeparatorChar}ty_game{directorySeparatorChar}ty_game_lib"
                : $"G:{directorySeparatorChar}workspace{directorySeparatorChar}RiderProjects{directorySeparatorChar}2d_game_lib{directorySeparatorChar}ty_game_lib";
            var p =
                $"{currentDirectory}{directorySeparatorChar}game_config{directorySeparatorChar}Resources{directorySeparatorChar}{s}_s.json";

            Console.Out.WriteLine($"{p}");


            using StreamReader sr = new StreamReader(p);
            string ss = sr.ReadToEnd();
            return ss;
        }
    }
}