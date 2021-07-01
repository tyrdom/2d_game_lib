using System;
using System.Dynamic;

namespace game_stuff
{
    public interface ISaleStuff
    {
        int GetId();

        int GetNum();
    }

    public static class SaleStuffStandard
    {
        public static ContainType GetTitle(this ISaleStuff saleStuff)
        {
            return saleStuff switch
            {
                PassiveTrait _ => ContainType.PassiveC,
                Prop _ => ContainType.PropC,
                Vehicle _ => ContainType.VehicleC,
                Weapon _ => ContainType.WeaponC,
                GameItem _ => ContainType.GameItemC,
                _ => throw new ArgumentOutOfRangeException(nameof(saleStuff))
            };
        }
        
        

        public static (ContainType type, int intId) GetSaleId(this ISaleStuff saleStuff)
        {
            return (saleStuff.GetTitle(), saleStuff.GetId());
        }
    }
}