using System;

namespace Systems.SimpleShops.Data.Enums
{
    [Flags]
    public enum ShopTransactionFlags
    {
        None = 0,
        IgnoreShopConditions = 1 << 0,
        IgnoreOfferConditions = 1 << 1,
        IgnoreTransactionConditions = 1 << 2
    }
}
