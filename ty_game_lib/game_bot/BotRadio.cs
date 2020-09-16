using collision_and_rigid;
using game_stuff;

namespace game_bot
{
    public interface IEnemyMsg
    {
    }

    public class EnemyFound : IEnemyMsg
    {
        public TwoDPoint Pos;
        public int? NowPolyId { get; }

        public EnemyFound(int? nowPolyId, TwoDPoint pos)
        {
            NowPolyId = nowPolyId;
            Pos = pos;
        }
    }

    public class BotRadio
    {
        public delegate void EnemyAlertHandler(IEnemyMsg charTickMsg);

        public event EnemyAlertHandler EnemyEvent = null!;

        public BotRadio()
        {
        }

        public void OnEnemyEvent(IEnemyMsg charTickMsg)
        {
            EnemyEvent?.Invoke(charTickMsg);
        }
    }
}