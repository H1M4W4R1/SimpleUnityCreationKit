using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimplePermissions.Abstract;

namespace Systems.SimplePermissions.Data
{
    /// <summary>
    ///     Addressable database for every configured requirement asset.
    /// </summary>
    public sealed class RequirementDatabase : AddressableDatabase<RequirementDatabase, RequirementBase>
    {
        /// <summary>Addressable label assigned to every requirement asset.</summary>
        public const string LABEL = "SimplePermissions.Requirements";

        /// <inheritdoc />
        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        private static bool _useTestData;

        [CanBeNull] public new static TRequirement GetExact<TRequirement>()
            where TRequirement : RequirementBase, new()
        {
            if (!_useTestData) return AddressableDatabase<RequirementDatabase, RequirementBase>.GetExact<TRequirement>();

            HashIdentifier hashIdentifier = HashIdentifier.New(typeof(TRequirement));
            int low = 0;
            int high = internalDataStorage.Count - 1;
            while (low <= high)
            {
                int mid = low + ((high - low) >> 1);
                AddressableDatabaseEntry<RequirementBase> entry = internalDataStorage[mid];
                int comparison = entry.hashIdentifier.CompareTo(hashIdentifier);
                if (comparison == 0) return entry.entryObject as TRequirement;

                if (comparison < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return null;
        }

        internal static void RegisterForTests([NotNull] RequirementBase requirement)
        {
            _useTestData = true;
            internalDataStorage.Add(
                new AddressableDatabaseEntry<RequirementBase>(HashIdentifier.New(requirement.GetType()), requirement));
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
