using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using game_stuff;

namespace game_bot
{
    public class Bot
    {
        public List<(float, SkillAction)> SkillToTrickRange;

        public BotRadio BotRadio;
        public CharacterBody CtrlBody;

        

        public Bot(List<(float, SkillAction)> skillToTrickRange, BotRadio botRadio, CharacterBody ctrlBody)
        {
            SkillToTrickRange = skillToTrickRange;
            BotRadio = botRadio;
            CtrlBody = ctrlBody;
        }

        void BotInit(CharInitMsg charInitMsg)
        {
            
        }

        Operate? BotGoATick(IEnumerable<CharTickMsg> charTickMsgs)
        {
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