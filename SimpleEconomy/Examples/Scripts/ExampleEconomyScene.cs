using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Data;
using Systems.SimpleCore.Examples;
using Systems.SimpleEconomy.Data.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleEconomy.Examples.Scripts
{
    [DisallowMultipleComponent]
    public sealed class ExampleEconomyScene : MonoBehaviour
    {
        [SerializeField] private long _grantAmount = 100L;
        [SerializeField] private long _spendAmount = 35L;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private bool _runExampleOnStart;

        private ExampleGoldWallet _wallet;
        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Awake()
        {
            if (!TryGetComponent(out _wallet))
                _wallet = gameObject.AddComponent<ExampleGoldWallet>();
        }

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
                RefreshStatus("Ready. Grant, spend, or test limits.");
            }
        }

        [ContextMenu("Run Economy Example")]
        public void RunExample()
        {
            ExampleGoldCurrency currency = CurrencyDatabase.GetExact<ExampleGoldCurrency>();
            if (ReferenceEquals(currency, null))
            {
                Debug.LogWarning("[SimpleEconomy] ExampleGoldCurrency was not found in the currency database. Let the auto-create/addressables setup generate it before running wallet operations.");
                return;
            }

            OperationResult grantResult = _wallet.TryAdd(_grantAmount);
            OperationResult spendResult = _wallet.TryTake(_spendAmount);
            _lastResult = ExampleRuntimePanel.FormatResult(spendResult);
            Debug.Log("[SimpleEconomy] Grant result: " + grantResult + ", spend result: " + spendResult + ", final balance: " + _wallet.Balance);
            RefreshStatus("Ran grant then spend flow.");
        }

        private void GrantCurrency()
        {
            if (!ValidateCurrency()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(_wallet.TryAdd(_grantAmount));
            RefreshStatus("Granted " + _grantAmount + " gold.");
        }

        private void SpendCurrency()
        {
            if (!ValidateCurrency()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(_wallet.TryTake(_spendAmount));
            RefreshStatus("Spent " + _spendAmount + " gold.");
        }

        private void TryOverspend()
        {
            if (!ValidateCurrency()) return;
            long requestedAmount = _wallet.Balance + _spendAmount;
            _lastResult = ExampleRuntimePanel.FormatResult(_wallet.TryTake(requestedAmount));
            RefreshStatus("Tried to spend " + requestedAmount + " gold with balance limits.");
        }

        private void AllowDebtSpend()
        {
            if (!ValidateCurrency()) return;
            long requestedAmount = _wallet.Balance + _spendAmount;
            _lastResult = ExampleRuntimePanel.FormatResult(_wallet.TryTake(requestedAmount, ModifyWalletCurrencyFlags.IgnoreBalanceLimits));
            RefreshStatus("Spent " + requestedAmount + " gold while ignoring balance limits.");
        }

        private void ResetWallet()
        {
            _wallet.ResetBalance();
            RefreshStatus("Wallet reset.");
        }

        private bool ValidateCurrency()
        {
            ExampleGoldCurrency currency = CurrencyDatabase.GetExact<ExampleGoldCurrency>();
            if (!ReferenceEquals(currency, null))
            {
                return true;
            }

            Debug.LogWarning("[SimpleEconomy] ExampleGoldCurrency was not found in the currency database. Let the auto-create/addressables setup generate it before running wallet operations.");
            RefreshStatus("ExampleGoldCurrency was not found in the currency database.");
            return false;
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleEconomy Example",
                "Explore wallet adds, spends, insufficient funds, and debt-style balance overrides.");

            _panel.AddSection("Wallet");
            Button grantButton = _panel.AddButton("Grant Gold");
            grantButton.onClick.AddListener(GrantCurrency);

            Button spendButton = _panel.AddButton("Spend Gold");
            spendButton.onClick.AddListener(SpendCurrency);

            Button overspendButton = _panel.AddButton("Try Overspend");
            overspendButton.onClick.AddListener(TryOverspend);

            Button debtButton = _panel.AddButton("Allow Debt Spend");
            debtButton.onClick.AddListener(AllowDebtSpend);

            Button resetButton = _panel.AddButton("Reset Wallet");
            resetButton.onClick.AddListener(ResetWallet);

            Button runAllButton = _panel.AddButton("Run Full Example");
            runAllButton.onClick.AddListener(RunExample);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            _panel.SetStatus(
                message +
                "\nBalance: " + _wallet.Balance +
                "\nLast result: " + _lastResult);
        }
    }
}
