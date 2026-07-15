using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers;

namespace Systems.SimpleCore.Storage.Databases
{
    /// <summary>Non-generic runtime database operations used by behaviour registration contracts.</summary>
    public interface IRuntimeDatabase
    {
        bool Register(object item);
        void Unregister(object item);
    }

    /// <summary>
    ///     Stores runtime objects by <see cref="Snowflake128"/> for one runtime contract.
    ///     Static storage is isolated by <typeparamref name="TSelf"/>, allowing multiple databases to use the same
    ///     contract without sharing registrations.
    /// </summary>
    /// <typeparam name="TSelf">Concrete database type.</typeparam>
    /// <typeparam name="TContract">Contract exposed when a registered object is resolved.</typeparam>
    public abstract class RuntimeDatabase<TSelf, TContract> : IRuntimeDatabase
        where TSelf : RuntimeDatabase<TSelf, TContract>, new()
        where TContract : class
    {
        /// <summary>Registered objects, sorted by identifier and isolated per closed database type.</summary>
        [NotNull] private static readonly List<RuntimeDatabaseEntry> internalDataStorage =
            new List<RuntimeDatabaseEntry>();

        /// <summary>Registers an object under its created <see cref="Snowflake128"/> identifier.</summary>
        public static bool Register<TItem>([NotNull] TItem item)
            where TItem : class, TContract, IIdentifiable<Snowflake128>
        {
            if (ReferenceEquals(item, null)) return false;
            return Register(item, item);
        }

        /// <summary>Removes an object only when it is still the registration for its identifier.</summary>
        public static void Unregister<TItem>([CanBeNull] TItem item)
            where TItem : class, TContract, IIdentifiable<Snowflake128>
        {
            if (ReferenceEquals(item, null)) return;

            Unregister(item, item);
        }

        bool IRuntimeDatabase.Register(object item)
        {
            if (!(item is TContract contract) || !(item is IIdentifiable<Snowflake128> identifiable)) return false;
            return Register(contract, identifiable);
        }

        void IRuntimeDatabase.Unregister(object item)
        {
            if (!(item is TContract contract) || !(item is IIdentifiable<Snowflake128> identifiable)) return;
            Unregister(contract, identifiable);
        }

        private static bool Register([NotNull] TContract item, IIdentifiable<Snowflake128> identifiable)
        {
            Snowflake128 identifier = identifiable.Identifier;
            if (!identifier.IsCreated) return false;

            int index = FindIndex(identifier);
            RuntimeDatabaseEntry entry = new RuntimeDatabaseEntry(identifier, item);
            if (index >= 0)
                internalDataStorage[index] = entry;
            else
                internalDataStorage.Insert(~index, entry);

            return true;
        }

        private static void Unregister([NotNull] TContract item, IIdentifiable<Snowflake128> identifiable)
        {

            Snowflake128 identifier = identifiable.Identifier;
            if (!identifier.IsCreated) return;
            int index = FindIndex(identifier);
            if (index < 0) return;

            RuntimeDatabaseEntry registeredEntry = internalDataStorage[index];
            if (!ReferenceEquals(registeredEntry.item, item)) return;

            internalDataStorage.RemoveAt(index);
        }

        /// <summary>Attempts to resolve a registered object by its <see cref="Snowflake128"/> identifier.</summary>
        public static bool TryGet(Snowflake128 identifier, [CanBeNull] out TContract item)
        {
            item = null;
            if (!identifier.IsCreated) return false;
            int index = FindIndex(identifier);
            if (index < 0) return false;

            RuntimeDatabaseEntry registeredEntry = internalDataStorage[index];
            item = registeredEntry.item;
            return true;
        }

        /// <summary>Clears all registered objects. Intended for concrete database test helpers.</summary>
        protected static void Clear()
        {
            internalDataStorage.Clear();
        }

        /// <summary>Finds an identifier or returns its bitwise-complement insertion index.</summary>
        private static int FindIndex(Snowflake128 identifier)
        {
            int low = 0;
            int high = internalDataStorage.Count - 1;
            while (low <= high)
            {
                int middle = (low + high) >> 1;
                RuntimeDatabaseEntry entry = internalDataStorage[middle];
                int comparison = entry.identifier.CompareTo(identifier);
                if (comparison == 0) return middle;

                if (comparison < 0)
                    low = middle + 1;
                else
                    high = middle - 1;
            }

            return ~low;
        }

        private readonly struct RuntimeDatabaseEntry
        {
            public readonly Snowflake128 identifier;
            [NotNull] public readonly TContract item;

            public RuntimeDatabaseEntry(Snowflake128 identifier, [NotNull] TContract item)
            {
                this.identifier = identifier;
                this.item = item;
            }
        }
    }
}
