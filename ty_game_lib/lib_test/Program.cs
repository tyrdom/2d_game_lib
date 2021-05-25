#nullable enable
using System;
using System.Collections.Generic;
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
                CharacterInitData.GenPlayerByConfig(1, 0, new[] {weapon_id.test_sword, weapon_id.test_gun}, size.small,
                    1);
            var characterInitData =
                CharacterInitData.GenPlayerByConfig(2, 1, new[] {weapon_id.test_sword}, size.small, 1);
            var characterInitData2 =
                CharacterInitData.GenPlayerByConfig(3, 1, new[] {weapon_id.test_sword}, size.small, 1);
            var characterInitData3 =
                CharacterInitData.GenPlayerByConfig(4, 1, new[] {weapon_id.test_sword}, size.small, 1);

            var characterInitData4 =
                CharacterInitData.GenPlayerByConfig(5, 1, new[] {weapon_id.test_sword}, size.small, 1);

            var genCharacterBody = genPlayerByConfig.GenCharacterBody(TwoDPoint.Zero());
            var characterBody = characterInitData.GenCharacterBody(TwoDPoint.Zero());
            var body = characterInitData2.GenCharacterBody(TwoDPoint.Zero());
            var body1 = characterInitData3.GenCharacterBody(TwoDPoint.Zero());
            var body2 = characterInitData4.GenCharacterBody(TwoDPoint.Zero());
            var genByConfig =
                RogueGame.GenByConfig(new HashSet<CharacterBody> {genCharacterBody, characterBody, body, body1, body2},
                    genCharacterBody);

            genByConfig.ForceSpawnNpc();
#if DEBUG
            var mapApplyDevices = genByConfig.NowPlayMap.PlayGround.GetMapApplyDevices();
            var any = mapApplyDevices.Any(x => x.IsActive);
            // Console.Out.WriteLine($"~~~!!~~~{any}~~!!~~{mapApplyDevices.Count}");
#endif
            for (var i = 0; i < 30; i++)
            {
                var twoDVector = new TwoDVector(0, 1f);
                var dVector1 = new TwoDVector(0, -0.5f);
                var dVector = new TwoDVector(0, -1f);
                var operate = i < 100
                    ? new Operate(aim: twoDVector, skillAction: SkillAction.Switch)
                    : new Operate(aim: twoDVector, move: dVector);

                var operate1 = new Operate(move: dVector1);
                var opDic = i < 100
                    ? new Dictionary<int, Operate>()
                        {{1, operate}}
                    : new Dictionary<int, Operate>()
                        {{1, operate}, {5, operate1}};
                var rogueGameGoTickResult = genByConfig.GamePlayGoATick(opDic);

                var (playerBeHit, trapBeHit, playerSee, playerTeleportTo) =
                    rogueGameGoTickResult.PlayGroundGoTickResult;

                var mapChange = rogueGameGoTickResult.MapChange;
                if (i % 5 == 0)
                {
                    genByConfig.GameConsoleGoATick(new Dictionary<int, IGameRequest>());
                }

                var characterBodies = playerSee[1].OnChange.OfType<CharacterBody>();
                var firstOrDefault = characterBodies.FirstOrDefault(x => x.GetId() == 5);
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

                if (genTickMsg?.SkillActing != null)
                    Console.Out.WriteLine(
                        $"$ ~~~~~{twoDPoint} {genTickMsg.Value.Gid} act launch :{genTickMsg.Value.SkillActing.Value.Item1} , {(skill_id) genTickMsg.Value.SkillActing.Value.Item2}");
#endif
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