using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimplePermissions.Abstract;

namespace Systems.SimplePermissions.Data
{
    /// <summary>
    ///     Addressable database for every configured permission asset.
    /// </summary>
    public sealed class PermissionDatabase : AddressableDatabase<PermissionDatabase, PermissionBase>
    {
        /// <summary>Addressable label assigned to every permission asset.</summary>
        public const string LABEL = "SimplePermissions.Permissions";

        /// <inheritdoc />
        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        private static bool _useTestData;

        [CanBeNull] public new static TPermission GetExact<TPermission>()
            where TPermission : PermissionBase, new()
        {
            if (!_useTestData) return AddressableDatabase<PermissionDatabase, PermissionBase>.GetExact<TPermission>();

            HashIdentifier hashIdentifier = HashIdentifier.New(typeof(TPermission));
            int low = 0;
            int high = internalDataStorage.Count - 1;
            while (low <= high)
            {
                int mid = low + ((high - low) >> 1);
                AddressableDatabaseEntry<PermissionBase> entry = internalDataStorage[mid];
                int comparison = entry.hashIdentifier.CompareTo(hashIdentifier);
                if (comparison == 0) return entry.entryObject as TPermission;

                if (comparison < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return null;
        }

        internal static void RegisterForTests([NotNull] PermissionBase permission)
        {
            _useTestData = true;
            internalDataStorage.Add(
                new AddressableDatabaseEntry<PermissionBase>(HashIdentifier.New(permission.GetType()), permission));
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
