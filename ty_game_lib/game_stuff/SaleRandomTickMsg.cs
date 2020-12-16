namespace game_stuff
{
    public class SaleRandomTickMsg : ISeeTickMsg
    {
        public SaleRandomTickMsg(GameItem cost, ContainType containType, int stackRest)
        {
            Cost = cost;
            ContainType = containType;
            StackRest = stackRest;
        }

        public GameItem Cost { get; }

        public ContainType ContainType { get; }

        public int StackRest { get; }
    }
}