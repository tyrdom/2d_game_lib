using game_stuff;

namespace game_bot
{
    public class BotRadio
    {
        public delegate void EnemyAlertHandler(CharTickMsg charTickMsg);

        public event EnemyAlertHandler EnemyEvent = null!;

        public BotRadio()
        {
            
        }

        public void OnEnemyEvent(CharTickMsg charTickMsg)
        {
            EnemyEvent?.Invoke(charTickMsg);
        }
    }
}