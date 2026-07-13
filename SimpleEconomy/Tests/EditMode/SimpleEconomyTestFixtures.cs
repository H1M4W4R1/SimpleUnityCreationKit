using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Currencies;
using Systems.SimpleEconomy.Data;
using Systems.SimpleEconomy.Data.Context;
using Systems.SimpleEconomy.Operations;
using Systems.SimpleEconomy.Wallets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleEconomy.Tests
{
    public abstract class SimpleEconomyTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            CurrencyDatabase.ClearForTests();
        }

        [TearDown]
        public void TearDown()
        {
            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (!createdObject) continue;
                Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
            CurrencyDatabase.ClearForTests();
        }

        protected TCurrencyType CreateRegisteredCurrency<TCurrencyType>()
            where TCurrencyType : CurrencyBase
        {
            TCurrencyType currency = Track(ScriptableObject.CreateInstance<TCurrencyType>());
            currency.name = typeof(TCurrencyType).Name;
            CurrencyDatabase.RegisterForTests(currency);
            return currency;
        }

        protected TWalletType CreateWallet<TWalletType>()
            where TWalletType : CurrencyWalletBase
        {
            GameObject gameObject = Track(new GameObject(typeof(TWalletType).Name));
            gameObject.SetActive(false);
            return gameObject.AddComponent<TWalletType>();
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected + " but received " + actual);
        }
    }

    public sealed class TestCurrency : CurrencyBase
    {
        public bool RejectAdd { get; set; }
        public bool RejectTake { get; set; }
        public int AddCheckCount { get; private set; }
        public int TakeCheckCount { get; private set; }
        public int AddedCount { get; private set; }
        public int AddFailedCount { get; private set; }
        public int TakenCount { get; private set; }
        public int TakeFailedCount { get; private set; }
        public CurrencyWalletBase LastWallet { get; private set; }
        public long LastAddAmount { get; private set; }
        public long LastTakeAmount { get; private set; }
        public long LastAmountLeft { get; private set; }
        public ushort LastSystemCode { get; private set; }
        public ushort LastResultCode { get; private set; }

        protected internal override OperationResult CanBeAdded(in CurrencyAddContext context)
        {
            AddCheckCount++;
            CaptureAddContext(context);
            if (RejectAdd) return EconomyOperations.NotEnoughCurrency();
            return base.CanBeAdded(context);
        }

        protected internal override OperationResult CanBeTaken(in CurrencyTakeContext context)
        {
            TakeCheckCount++;
            CaptureTakeContext(context);
            if (RejectTake) return EconomyOperations.NotEnoughCurrency();
            return base.CanBeTaken(context);
        }

        protected internal override void OnCurrencyAdded(
            in CurrencyAddContext context,
            in OperationResult result,
            long amountLeft)
        {
            AddedCount++;
            LastAmountLeft = amountLeft;
            CaptureAddContext(context);
            CaptureResult(result);
        }

        protected internal override void OnCurrencyAddFailed(
            in CurrencyAddContext context,
            in OperationResult result)
        {
            AddFailedCount++;
            CaptureAddContext(context);
            CaptureResult(result);
        }

        protected internal override void OnCurrencyTaken(
            in CurrencyTakeContext context,
            in OperationResult result,
            long amountLeft)
        {
            TakenCount++;
            LastAmountLeft = amountLeft;
            CaptureTakeContext(context);
            CaptureResult(result);
        }

        protected internal override void OnCurrencyTakeFailed(
            in CurrencyTakeContext context,
            in OperationResult result)
        {
            TakeFailedCount++;
            CaptureTakeContext(context);
            CaptureResult(result);
        }

        private void CaptureAddContext(in CurrencyAddContext context)
        {
            LastWallet = context.wallet;
            LastAddAmount = context.amount;
        }

        private void CaptureTakeContext(in CurrencyTakeContext context)
        {
            LastWallet = context.wallet;
            LastTakeAmount = context.amountExpected;
        }

        private void CaptureResult(in OperationResult result)
        {
            LastSystemCode = result.systemCode;
            LastResultCode = result.resultCode;
        }
    }

    public sealed class OtherTestCurrency : CurrencyBase
    {
    }

    public sealed class TestWallet : CurrencyWalletBase<TestCurrency>
    {
        public bool RejectAdd { get; set; }
        public bool RejectTake { get; set; }
        public int AddCheckCount { get; private set; }
        public int TakeCheckCount { get; private set; }
        public int AddedCount { get; private set; }
        public int AddFailedCount { get; private set; }
        public int TakenCount { get; private set; }
        public int TakeFailedCount { get; private set; }
        public long LastAddAmount { get; private set; }
        public long LastTakeAmount { get; private set; }
        public long LastAmountLeft { get; private set; }
        public ushort LastSystemCode { get; private set; }
        public ushort LastResultCode { get; private set; }

        public void SetBalanceForTests(long balance)
        {
            Balance = balance;
        }

        protected override OperationResult CanAddCurrency(in CurrencyAddContext context)
        {
            AddCheckCount++;
            LastAddAmount = context.amount;
            if (RejectAdd) return EconomyOperations.NotEnoughCurrency();
            return base.CanAddCurrency(context);
        }

        protected override OperationResult CanTakeCurrency(in CurrencyTakeContext context)
        {
            TakeCheckCount++;
            LastTakeAmount = context.amountExpected;
            if (RejectTake) return EconomyOperations.NotEnoughCurrency();
            return base.CanTakeCurrency(context);
        }

        protected override void OnCurrencyAdded(
            in CurrencyAddContext context,
            in OperationResult result,
            long amountLeft)
        {
            AddedCount++;
            LastAddAmount = context.amount;
            LastAmountLeft = amountLeft;
            CaptureResult(result);
            base.OnCurrencyAdded(context, result, amountLeft);
        }

        protected override void OnCurrencyAddFailed(
            in CurrencyAddContext context,
            in OperationResult result)
        {
            AddFailedCount++;
            LastAddAmount = context.amount;
            CaptureResult(result);
            base.OnCurrencyAddFailed(context, result);
        }

        protected override void OnCurrencyTaken(
            in CurrencyTakeContext context,
            in OperationResult result,
            long amountLeft)
        {
            TakenCount++;
            LastTakeAmount = context.amountExpected;
            LastAmountLeft = amountLeft;
            CaptureResult(result);
            base.OnCurrencyTaken(context, result, amountLeft);
        }

        protected override void OnCurrencyTakeFailed(
            in CurrencyTakeContext context,
            in OperationResult result)
        {
            TakeFailedCount++;
            LastTakeAmount = context.amountExpected;
            CaptureResult(result);
            base.OnCurrencyTakeFailed(context, result);
        }

        private void CaptureResult(in OperationResult result)
        {
            LastSystemCode = result.systemCode;
            LastResultCode = result.resultCode;
        }
    }

    public sealed class TestGlobalWallet : GlobalCurrencyWalletBase<TestGlobalWallet, TestCurrency>
    {
    }
}
