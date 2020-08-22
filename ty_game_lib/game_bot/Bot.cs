using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using collision_and_rigid;
using game_stuff;

namespace game_bot
{
    class Bot
    {
        public Dictionary<SkillAction, float> SkillToTrickRange;

        public BotRadio BotRadio;
        public CharacterBody CtrlBody;

        public TwoDPoint TargetPos;

        public Bot(Dictionary<SkillAction, float> skillToTrickRange, BotRadio botRadio, CharacterBody ctrlBody,
            TwoDPoint targetPos)
        {
            SkillToTrickRange = skillToTrickRange;
            BotRadio = botRadio;
            CtrlBody = ctrlBody;
            TargetPos = targetPos;
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