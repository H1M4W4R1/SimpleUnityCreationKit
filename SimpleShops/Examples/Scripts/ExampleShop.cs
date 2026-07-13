using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Components;
using Systems.SimpleShops.Data.Context;
using Systems.SimpleShops.Data.Enums;
using Systems.SimpleShops.Operations;
using UnityEngine;

namespace Systems.SimpleShops.Examples.Scripts
{
    public sealed class ExampleShop : ShopBase
    {
        [SerializeField] private int _customerCoins = 100;
        [SerializeField] private int _customerItems = 1;
        [SerializeField] private int _shopStock = 3;
        [SerializeField] private int _purchasePrice = 25;
        [SerializeField] private int _sellPrice = 10;

        private int _initialCustomerCoins;
        private int _initialCustomerItems;
        private int _initialShopStock;

        public int CustomerCoins => _customerCoins;
        public int CustomerItems => _customerItems;
        public int ShopStock => _shopStock;

        private void Awake()
        {
            _initialCustomerCoins = _customerCoins;
            _initialCustomerItems = _customerItems;
            _initialShopStock = _shopStock;
        }

        public void ResetExampleState()
        {
            _customerCoins = _initialCustomerCoins;
            _customerItems = _initialCustomerItems;
            _shopStock = _initialShopStock;
        }

        protected internal override OperationResult CanPayTransactionCosts(in ShopTransactionContext context)
        {
            if (context.transactionKind == ShopTransactionKind.Purchase && _customerCoins < _purchasePrice)
                return ShopOperations.TransactionCostUnavailable();

            if (context.transactionKind == ShopTransactionKind.Sell && _customerItems <= 0)
                return ShopOperations.TransactionCostUnavailable();

            return ShopOperations.Permitted();
        }

        protected internal override OperationResult CanGrantTransactionReturns(in ShopTransactionContext context)
        {
            if (context.transactionKind == ShopTransactionKind.Purchase && _shopStock <= 0)
                return ShopOperations.TransactionReturnUnavailable();

            return ShopOperations.Permitted();
        }

        protected internal override OperationResult PayTransactionCosts(in ShopTransactionContext context)
        {
            if (context.transactionKind == ShopTransactionKind.Purchase)
            {
                _customerCoins -= _purchasePrice;
                return ShopOperations.Permitted();
            }

            _customerItems--;
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult RefundTransactionCosts(in ShopTransactionContext context)
        {
            if (context.transactionKind == ShopTransactionKind.Purchase)
            {
                _customerCoins += _purchasePrice;
                return ShopOperations.Permitted();
            }

            _customerItems++;
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult GrantTransactionReturns(in ShopTransactionContext context)
        {
            if (context.transactionKind == ShopTransactionKind.Purchase)
            {
                _shopStock--;
                _customerItems++;
                return ShopOperations.Permitted();
            }

            _shopStock++;
            _customerCoins += _sellPrice;
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult RollbackTransactionReturns(in ShopTransactionContext context)
        {
            if (context.transactionKind == ShopTransactionKind.Purchase)
            {
                _shopStock++;
                _customerItems--;
                return ShopOperations.Permitted();
            }

            _shopStock--;
            _customerCoins -= _sellPrice;
            return ShopOperations.Permitted();
        }
    }
}
