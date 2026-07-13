using JetBrains.Annotations;
using Systems.SimpleShops.Abstract;
using Systems.SimpleShops.Components;
using Systems.SimpleShops.Data.Enums;

namespace Systems.SimpleShops.Data.Context
{
    public readonly ref struct ShopTransactionContext
    {
        [CanBeNull] public readonly ShopBase shop;
        [CanBeNull] public readonly ShopOfferBase offer;
        [CanBeNull] public readonly IShopCustomer customer;
        public readonly ShopTransactionKind transactionKind;
        public readonly ShopTransactionFlags flags;

        public ShopTransactionContext(
            [CanBeNull] ShopBase shop,
            [CanBeNull] ShopOfferBase offer,
            [CanBeNull] IShopCustomer customer,
            ShopTransactionKind transactionKind,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            this.shop = shop;
            this.offer = offer;
            this.customer = customer;
            this.transactionKind = transactionKind;
            this.flags = flags;
        }
    }
}
