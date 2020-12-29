namespace game_stuff
{
    public class SaleBoxTickMsg : ISeeTickMsg
    {
        public SaleBoxTickMsg(GameItem cost, (ContainType containType, int id)[] contains, int stackRest)
        {
            Cost = cost;

            Contains = contains;
            StackRest = stackRest;
        }

        public GameItem Cost { get; }

        public (ContainType containType, int id)[] Contains { get; }

        public int StackRest { get; }
    }
}