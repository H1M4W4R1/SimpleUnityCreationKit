using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Examples;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleShops.Examples.Scripts
{
    [DisallowMultipleComponent]
    public sealed class ExampleShopScene : MonoBehaviour
    {
        [SerializeField] private ExampleShop _shop;
        [SerializeField] private ExampleShopCustomer _customer;
        [SerializeField] private ExamplePotionPurchaseOffer _purchaseOffer;
        [SerializeField] private ExamplePotionSellOffer _sellOffer;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private bool _runExampleOnStart;

        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            if (_runExampleOnStart)
            {
                RunExample();
            }
            else
            {
                RefreshStatus("Ready. Purchase, sell, or check offers.");
            }
        }

        [ContextMenu("Run Shop Example")]
        public void RunExample()
        {
            if (!ValidateShop()) return;
            OperationResult purchaseResult = _shop.TryPurchase(_purchaseOffer, _customer);
            OperationResult sellResult = _shop.TrySell(_sellOffer, _customer);
            _lastResult = ExampleRuntimePanel.FormatResult(sellResult);
            Debug.Log("[SimpleShops] Purchase result: " + purchaseResult + ", sell result: " + sellResult + ", coins: " + _shop.CustomerCoins + ", items: " + _shop.CustomerItems + ", stock: " + _shop.ShopStock);
            RefreshStatus("Ran purchase then sell flow.");
        }

        private void CheckPurchase()
        {
            if (!ValidateShop()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(_shop.CanPurchase(_purchaseOffer, _customer));
            RefreshStatus("Purchase check completed.");
        }

        private void PurchasePotion()
        {
            if (!ValidateShop()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(_shop.TryPurchase(_purchaseOffer, _customer));
            RefreshStatus("Purchase attempted.");
        }

        private void CheckSell()
        {
            if (!ValidateShop()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(_shop.CanSell(_sellOffer, _customer));
            RefreshStatus("Sell check completed.");
        }

        private void SellPotion()
        {
            if (!ValidateShop()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(_shop.TrySell(_sellOffer, _customer));
            RefreshStatus("Sell attempted.");
        }

        private void ResetShop()
        {
            if (!_shop)
            {
                RefreshStatus("Example shop is not assigned.");
                return;
            }

            _shop.ResetExampleState();
            RefreshStatus("Shop state reset.");
        }

        private bool ValidateShop()
        {
            if (!_shop)
            {
                Debug.LogWarning("[SimpleShops] Example shop is not assigned.");
                RefreshStatus("Example shop is not assigned.");
                return false;
            }

            if (!_customer)
            {
                Debug.LogWarning("[SimpleShops] Example customer is not assigned.");
                RefreshStatus("Example customer is not assigned.");
                return false;
            }

            if (ReferenceEquals(_purchaseOffer, null) || !_purchaseOffer)
            {
                Debug.LogWarning("[SimpleShops] Example purchase offer is not assigned.");
                RefreshStatus("Example purchase offer is not assigned.");
                return false;
            }

            if (ReferenceEquals(_sellOffer, null) || !_sellOffer)
            {
                Debug.LogWarning("[SimpleShops] Example sell offer is not assigned.");
                RefreshStatus("Example sell offer is not assigned.");
                return false;
            }

            return true;
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleShops Example",
                "Navigate purchase checks, purchases, sell checks, sell transactions, and stock limits.");

            _panel.AddSection("Transactions");
            Button checkPurchaseButton = _panel.AddButton("Check Purchase");
            checkPurchaseButton.onClick.AddListener(CheckPurchase);

            Button purchaseButton = _panel.AddButton("Purchase Potion");
            purchaseButton.onClick.AddListener(PurchasePotion);

            Button checkSellButton = _panel.AddButton("Check Sell");
            checkSellButton.onClick.AddListener(CheckSell);

            Button sellButton = _panel.AddButton("Sell Potion");
            sellButton.onClick.AddListener(SellPotion);

            Button resetButton = _panel.AddButton("Reset Shop State");
            resetButton.onClick.AddListener(ResetShop);

            Button runAllButton = _panel.AddButton("Run Full Example");
            runAllButton.onClick.AddListener(RunExample);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            string state = _shop
                ? "Coins: " + _shop.CustomerCoins + " | Items: " + _shop.CustomerItems + " | Stock: " + _shop.ShopStock
                : "Shop missing";

            _panel.SetStatus(
                message +
                "\n" + state +
                "\nLast result: " + _lastResult);
        }
    }
}
