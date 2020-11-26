using collision_and_rigid;

namespace game_stuff
{
    public readonly struct TrapGoTickResult : IGameUnitTickResult
    {
        public TrapGoTickResult(bool stillAlive = true, IEffectMedia? launchBullet = null,
            Trap? self = null)
        {
            Self = self;
            StillAlive = stillAlive;
            LaunchBullet = launchBullet;
        }

        public bool StillAlive { get; }
        public IEffectMedia? LaunchBullet { get; }

        public Trap? Self { get; }
    }
}