using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using game_stuff;

namespace game_bot
{
    public class Bot
    {
        public Dictionary<int, ImmutableArray<(float, SkillAction)>> SkillToTrickRange;


        public List<(float, int)> RangeToWeapon;
        public BotRadio BotRadio;

        public int NowWeapon;

        public Bot(Dictionary<int, ImmutableArray<(float, SkillAction)>> skillToTrickRange, BotRadio botRadio,
            List<(float, int)> rangeToWeapon)
        {
            SkillToTrickRange = skillToTrickRange;
            BotRadio = botRadio;
            RangeToWeapon = rangeToWeapon;
            NowWeapon = 0;
        }

        public Bot(CharInitMsg charInitMsg, BotRadio teamRadio)
        {
            var valueTuples = new List<(float, int)>();
            var dictionary = new Dictionary<int, ImmutableArray<(float, SkillAction)>>();
            foreach (var keyValuePair in charInitMsg.WeaponConfigs)
            {
                var max = keyValuePair.Value.Ranges.Select(x => x.Item1).Max();
                valueTuples.Add((max, keyValuePair.Key));
                dictionary[keyValuePair.Key] = keyValuePair.Value.Ranges;
            }

            valueTuples.Sort((x, y) => -x.Item1.CompareTo(y.Item1));
            SkillToTrickRange = dictionary;
            BotRadio = teamRadio;
            RangeToWeapon = valueTuples;
            NowWeapon = 0;
        }

        Operate? BotSimpleAct(IEnumerable<CharTickMsg> charTickMsgs)
        {
            //

            //

            //
            return null;
        }


        void ListenRadio()
        {
            
        }

        void BroadcastRadio()
        {
        }
    }
}