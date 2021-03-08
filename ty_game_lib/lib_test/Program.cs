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

            rogue_game.LocalConfig.LoadConfig(dictionary);
#if DEBUG
            Console.Out.WriteLine($"~~~~~~{rogue_game.LocalConfig.RogueRebornTick}");
#endif


            var genPlayerByConfig = game_stuff.CharacterInitData.GenPlayerByConfig(1, 1, new[] {1}, size.small, 1);
            var genCharacterBody = genPlayerByConfig.GenCharacterBody(TwoDPoint.Zero());
            var genByConfig =
                rogue_game.RogueGame.GenByConfig(new HashSet<CharacterBody>() {genCharacterBody}, genCharacterBody);

#if DEBUG
            var mapApplyDevices = genByConfig.NowPlayMap.PlayGround.GetMapApplyDevices();
            var any = mapApplyDevices.Any(x => x.IsActive);
            Console.Out.WriteLine($"~~~!!~~~{any}~~!!~~{mapApplyDevices.Count}");
#endif
            for (var i = 0; i < 200; i++)
            {
                var operate = new Operate(mapInteractive: MapInteract.BuyOrApplyCall);
                var (playerBeHit, trapBeHit, playerSee, playerTeleportTo) =
                    genByConfig.GamePlayGoATick(new Dictionary<int, Operate>() {{1, operate}});
                if (i % 5 == 0)
                {
                    genByConfig.GameConsoleGoATick(new Dictionary<int, IGameRequest>());
                }

                var twoDPoint = playerSee[1].OfType<CharacterBody>().FirstOrDefault()?.GetAnchor();
#if DEBUG
                Console.Out.WriteLine($"$ ~~~~~{twoDPoint}");
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