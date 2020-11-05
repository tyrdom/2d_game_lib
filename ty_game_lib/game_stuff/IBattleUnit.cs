using game_config;

namespace game_stuff
{
    public interface IBattleUnit
    {
        float MaxMoveSpeed { get; }
        float MinMoveSpeed { get; }
        float AddMoveSpeed { get; }

        base_attr_id BaseAttrId { get; }
        SurvivalStatus SurvivalStatus { get; }

        AttackStatus AttackStatus { get; }
    }
}