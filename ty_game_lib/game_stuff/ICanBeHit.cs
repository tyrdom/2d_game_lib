using System.Collections.Generic;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public interface ICanBeAndNeedHit : IIdPointShape, ICanBeAndNeedSaw, ICanBeEnemy
    {
        int GetTeam();
        IdPointBox InBox { get; set; }
        size GetSize();
        bool CheckCanBeHit();
        public IdPointBox CovToIdBox();
    }


    public interface ISeeTickMsg
    {
        string? ToString();
    }

    public interface ICharMsg : ISeeTickMsg
    {
        public int GId { get; }
        public HashSet<ICharEvent> CharEvents { get; }
    }

    public interface ISeeActive
    {
        int AId { get; }
        bool IsActive { get; }
    }

    public interface ISeePosChange
    {
        TwoDPoint Pos { get; }
    }

    public static class CanBeHitStandard
    {
    }
}