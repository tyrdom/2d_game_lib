using game_stuff;

namespace game_bot
{
    public readonly struct BotOpAndThink
    {
        public BotOpAndThink(Operate? operate = null, SkillAction? thinkAct = null)
        {
            Operate = operate;
            ThinkAct = thinkAct;
        }

        public Operate? Operate { get; }
        public SkillAction? ThinkAct { get; }
    }
}