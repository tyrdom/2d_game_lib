namespace rogue_game
{
    public interface IGameRequest
    {
    }

    public readonly struct GoNextChapter : IGameRequest
    {
    }
    public  readonly struct Pause: IGameRequest
    {
    }
    public readonly struct ResetGame : IGameRequest
    {
    }

    public readonly struct Leave : IGameRequest
    {
    }
#if DEBUG
    public readonly struct SkipChapter : IGameRequest
    {
    }

    public readonly struct ForcePassChapter : IGameRequest
    {
    }
#endif
    public readonly struct KickPlayer : IGameRequest
    {
        public KickPlayer(int seat)
        {
            Seat = seat;
        }

        public int Seat { get; }
    }

    public readonly struct RebornPlayer : IGameRequest
    {
        public RebornPlayer(int seat)
        {
            Seat = seat;
        }

        public int Seat { get; }
    }
}