using collision_and_rigid;

namespace game_stuff
{
    public class SaleBoxTickMsg : ISeeTickMsg
    {
        public SaleBoxTickMsg(GameItem cost, (ContainType containType, int id)[] contains, int stackRest, TwoDPoint pos)
        {
            Cost = cost;

            Contains = contains;
            StackRest = stackRest;
            Pos = pos;
        }

        public GameItem Cost { get; }

        public (ContainType containType, int id)[] Contains { get; }

        public int StackRest { get; }
        public TwoDPoint Pos { get; }
    }
}