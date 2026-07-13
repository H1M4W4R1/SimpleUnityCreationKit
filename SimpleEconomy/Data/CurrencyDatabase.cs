using System.Runtime.CompilerServices;
using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleEconomy.Currencies;

[assembly: InternalsVisibleTo("SimpleEconomy.Tests")]

namespace Systems.SimpleEconomy.Data
{
    /// <summary>
    ///     Database of currencies
    /// </summary>
    public sealed class CurrencyDatabase : AddressableDatabase<CurrencyDatabase, CurrencyBase>
    {
        public const string LABEL = "SimpleEconomy.Currencies";

        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        private static bool _useTestData;

        public new static int Count => _useTestData
            ? internalDataStorage.Count
            : AddressableDatabase<CurrencyDatabase, CurrencyBase>.Count;

        [CanBeNull] public new static TCurrencyType GetExact<TCurrencyType>()
            where TCurrencyType : CurrencyBase, new()
        {
            if (!_useTestData) return AddressableDatabase<CurrencyDatabase, CurrencyBase>.GetExact<TCurrencyType>();

            HashIdentifier hashIdentifier = HashIdentifier.New(typeof(TCurrencyType));
            for (int entryIndex = 0; entryIndex < internalDataStorage.Count; entryIndex++)
            {
                AddressableDatabaseEntry<CurrencyBase> entry = internalDataStorage[entryIndex];
                if (entry.hashIdentifier.CompareTo(hashIdentifier) != 0) continue;
                if (entry.entryObject is TCurrencyType currency) return currency;
            }

            return null;
        }

        internal static void RegisterForTests<TCurrencyType>([NotNull] TCurrencyType currency)
            where TCurrencyType : CurrencyBase
        {
            _useTestData = true;
            internalDataStorage.Add(
                new AddressableDatabaseEntry<CurrencyBase>(HashIdentifier.New(typeof(TCurrencyType)), currency));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            _useTestData = true;
            internalDataStorage.Clear();
        }
#endif
    }
}
