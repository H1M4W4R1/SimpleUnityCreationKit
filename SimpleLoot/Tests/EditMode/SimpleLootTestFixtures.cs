using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleLoot.Abstract.Generator;
using Systems.SimpleLoot.Abstract.Interfaces;
using Systems.SimpleLoot.Abstract.LootTable;
using Systems.SimpleLoot.Abstract.Rarity;
using Systems.SimpleLoot.Data;
using Systems.SimpleLoot.Operations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleLoot.Tests
{
    public abstract class SimpleLootTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            LootGeneratorDatabase.ClearForTests();
        }

        [TearDown]
        public void TearDown()
        {
            LootGeneratorDatabase.ClearForTests();

            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        protected TestLootItem CreateItem(string name, float chance = 0f, bool allowGeneration = true)
        {
            TestLootItem item = Track(ScriptableObject.CreateInstance<TestLootItem>());
            item.name = name;
            item.ChanceValue = chance;
            item.AllowGeneration = allowGeneration;
            return item;
        }

        protected TestRarity CreateRarity(float chance)
        {
            TestRarity rarity = Track(ScriptableObject.CreateInstance<TestRarity>());
            rarity.ChanceValue = chance;
            return rarity;
        }

        protected TestLootTable CreateTable(params (TestLootItem Item, TestRarity RarityOverride)[] entries)
        {
            TestLootTable table = Track(ScriptableObject.CreateInstance<TestLootTable>());
            SerializedObject serializedTable = new SerializedObject(table);
            SerializedProperty entriesProperty = serializedTable.FindProperty("_entries");
            entriesProperty.arraySize = entries.Length;

            for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
            {
                SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(entryIndex);
                entryProperty.FindPropertyRelative("_item").objectReferenceValue = entries[entryIndex].Item;
                entryProperty.FindPropertyRelative("_rarityOverride").objectReferenceValue =
                    entries[entryIndex].RarityOverride;
            }

            serializedTable.ApplyModifiedPropertiesWithoutUndo();
            return table;
        }

        protected TGenerator CreateRegisteredGenerator<TGenerator>()
            where TGenerator : LootDropGeneratorBase
        {
            TGenerator generator = Track(ScriptableObject.CreateInstance<TGenerator>());
            LootGeneratorDatabase.RegisterForTests(generator);
            return generator;
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static List<TLoot> CopyAndRelease<TLoot>(ROListAccess<TLoot> access)
        {
            List<TLoot> copy = new List<TLoot>(access.List.Count);
            for (int itemIndex = 0; itemIndex < access.List.Count; itemIndex++)
            {
                copy.Add(access.List[itemIndex]);
            }

            access.Release();
            return copy;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected + " but received " + actual);
        }
    }

    public sealed class TestLootTable : LootTableBase<TestLootItem>
    {
    }

    public sealed class TestLootItem : ScriptableObject, IWithChance, IWithRarity
    {
        public float ChanceValue { get; set; }
        public bool AllowGeneration { get; set; } = true;
        public RarityBase RarityValue { get; set; }

        public float Chance => ChanceValue;
        public RarityBase Rarity => RarityValue;
    }

    public sealed class TestRarity : RarityBase
    {
        public float ChanceValue { get; set; }
        public override float Chance => ChanceValue;
    }

    public abstract class TestLootGeneratorBase<TGenerator>
        : WeightedLootDropGenerator<TGenerator, TestLootItem>
        where TGenerator : TestLootGeneratorBase<TGenerator>, new()
    {
        public int GeneratedCount { get; private set; }
        public int FailedCount { get; private set; }
        public int LastLootCount { get; private set; }
        public long LastBudget { get; private set; }
        public LootGenerationFlags LastFlags { get; private set; }

        protected override OperationResult CanGenerateItem(
            LootTableEntry<TestLootItem> entry, in LootGenerationContext<TestLootItem> context)
        {
            return entry.Item.AllowGeneration
                ? LootOperations.Permitted()
                : LootOperations.ItemConditionFailed();
        }

        protected override void OnLootGenerated(
            IReadOnlyList<TestLootItem> loot, in LootGenerationContext<TestLootItem> context)
        {
            GeneratedCount++;
            LastLootCount = loot.Count;
            LastBudget = context.budget;
            LastFlags = context.flags;
        }

        protected override void OnLootGenerationFailed(in LootGenerationContext<TestLootItem> context)
        {
            FailedCount++;
            LastBudget = context.budget;
            LastFlags = context.flags;
        }
    }

    public sealed class TestWeightedGenerator : TestLootGeneratorBase<TestWeightedGenerator>
    {
    }

    public sealed class TestEqualGenerator
        : EqualLootDropGenerator<TestEqualGenerator, TestLootItem>
    {
        public int GeneratedCount { get; private set; }
        public int FailedCount { get; private set; }
        public int LastLootCount { get; private set; }
        public long LastBudget { get; private set; }
        public LootGenerationFlags LastFlags { get; private set; }

        protected override OperationResult CanGenerateItem(
            LootTableEntry<TestLootItem> entry, in LootGenerationContext<TestLootItem> context)
        {
            return entry.Item.AllowGeneration
                ? LootOperations.Permitted()
                : LootOperations.ItemConditionFailed();
        }

        protected override void OnLootGenerated(
            IReadOnlyList<TestLootItem> loot, in LootGenerationContext<TestLootItem> context)
        {
            GeneratedCount++;
            LastLootCount = loot.Count;
            LastBudget = context.budget;
            LastFlags = context.flags;
        }

        protected override void OnLootGenerationFailed(in LootGenerationContext<TestLootItem> context)
        {
            FailedCount++;
            LastBudget = context.budget;
            LastFlags = context.flags;
        }
    }
}
