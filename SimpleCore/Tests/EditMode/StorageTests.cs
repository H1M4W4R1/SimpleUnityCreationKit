using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleCore.Storage.Lists;
using UnityEngine;

namespace Systems.SimpleCore.Tests
{
    public sealed class StorageTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int objectIndex = 0; objectIndex < _createdObjects.Count; objectIndex++)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (ReferenceEquals(createdObject, null)) continue;
                Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
            TestRuntimeDatabase.ClearForTests();
        }

        [Test]
        public void RWListAccess_CreateProvidesMutableListAndReleaseInvalidatesAccess()
        {
            RWListAccess<int> access = RWListAccess<int>.Create();

            Assert.IsTrue(access.IsValid);
            access.List.Add(10);
            access.List.Add(20);
            Assert.AreEqual(2, access.List.Count);

            access.Release();

            Assert.IsFalse(access.IsValid);
        }

        [Test]
        public void ROListAccess_CreateProvidesReadOnlyViewAndReleaseReturnsPooledList()
        {
            ROListAccess<string> access = ROListAccess<string>.Create();

            Assert.AreEqual(0, access.List.Count);

            access.Release();
        }

        [Test]
        public void AddressableDatabaseEntry_StoresHashAndObject()
        {
            DatabaseLeafAsset asset = CreateAsset<DatabaseLeafAsset>("entry");
            HashIdentifier hashIdentifier = HashIdentifier.New(typeof(DatabaseLeafAsset));

            AddressableDatabaseEntry<DatabaseBaseAsset> entry =
                new AddressableDatabaseEntry<DatabaseBaseAsset>(hashIdentifier, asset);

            Assert.AreEqual(hashIdentifier, entry.hashIdentifier);
            Assert.AreSame(asset, entry.entryObject);
        }

        [Test]
        public void AddressableDatabase_EmptyLabelLoadsAnEmptyDatabase()
        {
            Assert.AreEqual(0, EmptyAddressableDatabase.Count);
        }

        [Test]
        public void RuntimeDatabase_RegisterAndTryGet_ResolvesRegisteredContract()
        {
            Snowflake128 identifier = new Snowflake128(21L, 9UL);
            RuntimeDatabaseObject runtimeObject = CreateRuntimeDatabaseObject(identifier);

            Assert.IsTrue(TestRuntimeDatabase.Register(runtimeObject));
            Assert.IsTrue(TestRuntimeDatabase.TryGet(identifier, out IRuntimeDatabaseContract resolvedObject));
            Assert.AreSame(runtimeObject, resolvedObject);
        }

        [Test]
        public void RuntimeDatabase_Unregister_DoesNotRemoveReplacementRegistration()
        {
            Snowflake128 identifier = new Snowflake128(22L, 9UL);
            RuntimeDatabaseObject originalObject = CreateRuntimeDatabaseObject(identifier);
            RuntimeDatabaseObject replacementObject = CreateRuntimeDatabaseObject(identifier);

            TestRuntimeDatabase.Register(originalObject);
            TestRuntimeDatabase.Register(replacementObject);
            TestRuntimeDatabase.Unregister(originalObject);

            Assert.IsTrue(TestRuntimeDatabase.TryGet(identifier, out IRuntimeDatabaseContract resolvedObject));
            Assert.AreSame(replacementObject, resolvedObject);
        }

        [Test]
        public void RuntimeDatabase_RegisterOutOfOrder_ResolvesEveryObject()
        {
            Snowflake128 firstIdentifier = new Snowflake128(23L, 9UL);
            Snowflake128 secondIdentifier = new Snowflake128(24L, 9UL);
            Snowflake128 thirdIdentifier = new Snowflake128(25L, 9UL);
            RuntimeDatabaseObject firstObject = CreateRuntimeDatabaseObject(firstIdentifier);
            RuntimeDatabaseObject secondObject = CreateRuntimeDatabaseObject(secondIdentifier);
            RuntimeDatabaseObject thirdObject = CreateRuntimeDatabaseObject(thirdIdentifier);

            TestRuntimeDatabase.Register(thirdObject);
            TestRuntimeDatabase.Register(firstObject);
            TestRuntimeDatabase.Register(secondObject);

            Assert.IsTrue(TestRuntimeDatabase.TryGet(firstIdentifier, out IRuntimeDatabaseContract resolvedFirstObject));
            Assert.IsTrue(TestRuntimeDatabase.TryGet(secondIdentifier, out IRuntimeDatabaseContract resolvedSecondObject));
            Assert.IsTrue(TestRuntimeDatabase.TryGet(thirdIdentifier, out IRuntimeDatabaseContract resolvedThirdObject));
            Assert.AreSame(firstObject, resolvedFirstObject);
            Assert.AreSame(secondObject, resolvedSecondObject);
            Assert.AreSame(thirdObject, resolvedThirdObject);
        }

        [Test]
        public void RuntimeDatabase_RegisterManagedWrapper_ResolvesRegisteredContract()
        {
            Snowflake128 identifier = new Snowflake128(26L, 9UL);
            RuntimeDatabaseWrapper wrapper = new RuntimeDatabaseWrapper(identifier);

            Assert.IsTrue(TestRuntimeDatabase.Register(wrapper));
            Assert.IsTrue(TestRuntimeDatabase.TryGet(identifier, out IRuntimeDatabaseContract resolvedObject));
            Assert.AreSame(wrapper, resolvedObject);
        }

        [Test]
        public void RWListAccess_ToReadOnlyExposesSameCurrentItems()
        {
            RWListAccess<int> access = RWListAccess<int>.Create();
            access.List.Add(4);
            access.List.Add(8);

            ROListAccess<int> readOnlyAccess = access.ToReadOnly();
            try
            {
                Assert.AreEqual(2, readOnlyAccess.List.Count);
                Assert.AreEqual(4, readOnlyAccess.List[0]);
                Assert.AreEqual(8, readOnlyAccess.List[1]);
            }
            finally
            {
                readOnlyAccess.Release();
            }
        }

        [Test]
        public void RWListAccess_CreateClearsListReturnedFromPool()
        {
            RWListAccess<int> firstAccess = RWListAccess<int>.Create();
            firstAccess.List.Add(12);
            firstAccess.Release();

            RWListAccess<int> secondAccess = RWListAccess<int>.Create();
            try
            {
                Assert.AreEqual(0, secondAccess.List.Count);
            }
            finally
            {
                secondAccess.Release();
            }
        }

        private TAsset CreateAsset<TAsset>(string assetName)
            where TAsset : DatabaseBaseAsset
        {
            TAsset asset = ScriptableObject.CreateInstance<TAsset>();
            asset.name = assetName;
            _createdObjects.Add(asset);
            return asset;
        }

        private RuntimeDatabaseObject CreateRuntimeDatabaseObject(Snowflake128 identifier)
        {
            RuntimeDatabaseObject runtimeObject = ScriptableObject.CreateInstance<RuntimeDatabaseObject>();
            runtimeObject.Initialize(identifier);
            _createdObjects.Add(runtimeObject);
            return runtimeObject;
        }

        public class DatabaseBaseAsset : ScriptableObject
        {
        }

        public class DatabaseIntermediateAsset : DatabaseBaseAsset
        {
        }

        public sealed class DatabaseLeafAsset : DatabaseIntermediateAsset
        {
        }

        public sealed class DatabaseOtherLeafAsset : DatabaseBaseAsset
        {
        }

        public sealed class EmptyAddressableDatabase : AddressableDatabase<EmptyAddressableDatabase, DatabaseBaseAsset>
        {
            protected override string AddressableLabel => "SimpleCore.Tests.EmptyAddressableDatabase";
        }

        private interface IRuntimeDatabaseContract
        {
        }

        private sealed class RuntimeDatabaseObject : ScriptableObject, IRuntimeDatabaseContract, IIdentifiable<Snowflake128>
        {
            public Snowflake128 Identifier { get; set; }

            public void Initialize(Snowflake128 identifier)
            {
                Identifier = identifier;
            }
        }

        private sealed class RuntimeDatabaseWrapper : IRuntimeDatabaseContract, IIdentifiable<Snowflake128>
        {
            public Snowflake128 Identifier { get; set; }

            public RuntimeDatabaseWrapper(Snowflake128 identifier)
            {
                Identifier = identifier;
            }
        }

        private sealed class TestRuntimeDatabase :
            RuntimeDatabase<TestRuntimeDatabase, IRuntimeDatabaseContract>
        {
            public static void ClearForTests()
            {
                Clear();
            }
        }
    }
}
