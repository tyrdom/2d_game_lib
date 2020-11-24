using collision_and_rigid;

namespace game_stuff
{
    public readonly struct TrapGoTickResult : IGameUnitTickResult
    {
        public TrapGoTickResult(bool stillAlive = true, IEffectMedia? launchBullet = null,
            IdPointBox? idPointBox = null)
        {
            IdPointBox = idPointBox;
            StillAlive = stillAlive;
            LaunchBullet = launchBullet;
        }

        public bool StillAlive { get; }
        public IEffectMedia? LaunchBullet { get; }

        public IdPointBox? IdPointBox { get; }
    }
}