using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleFactions.Data.SaveFiles;
using Systems.SimpleFactions.Utility;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using Systems.SimpleSaving.Abstract;
using UnityEngine;

namespace Systems.SimpleFactions.Tests
{
    public sealed class FactionRelationPersistenceTests : SimpleFactionsTestBase
    {
        [Test]
        public void SaveToMemoryAndLoad_RestoresFactionToFactionRelation()
        {
            TestFaction sourceFaction = CreateRegisteredFaction<TestFaction>();
            OtherTestFaction targetFaction = CreateRegisteredFaction<OtherTestFaction>();
            TestFactionRelation relationType = Track(ScriptableObject.CreateInstance<TestFactionRelation>());
            RelationTypeDatabase.RegisterForTests(relationType);

            FactionAPI.SetRelation<TestFactionRelation>(sourceFaction, targetFaction, 7);
            SaveFileBase memorySave = FactionAPI.SaveToMemory();
            FactionRelationSaveFile saveFile = memorySave as FactionRelationSaveFile;
            FactionAPI.SetRelation<TestFactionRelation>(sourceFaction, targetFaction, 99);

            FactionAPI.Load(memorySave);

            Assert.IsNotNull(saveFile);
            Assert.AreEqual(1, saveFile.Entries.Length);
            Assert.AreEqual(FactionRelationTargetKind.Faction, saveFile.Entries[0].TargetKind);
            Assert.AreEqual(HashIdentifier.New(targetFaction.GetType()).Value, saveFile.Entries[0].TargetFactionTypeHash);
            Assert.AreEqual(7, FactionAPI.GetRelationValue<TestFactionRelation>(sourceFaction, targetFaction));
        }

        [Test]
        public void SaveToMemoryAndLoad_RestoresRelationToRegisteredIdentifiedRuntimeObject()
        {
            TestFaction sourceFaction = CreateRegisteredFaction<TestFaction>();
            TestFactionRelation relationType = Track(ScriptableObject.CreateInstance<TestFactionRelation>());
            RelationTypeDatabase.RegisterForTests(relationType);
            GameObject targetObject = Track(new GameObject("Player"));
            Snowflake128 identifier = new Snowflake128(10L, 4UL);
            IdentifiedRelatable target = targetObject.AddComponent<IdentifiedRelatable>();
            target.Initialize(identifier);
            Assert.IsTrue(RelatableObjectDatabase.Register(target));

            FactionAPI.SetRelation<TestFactionRelation>(sourceFaction, target, 50);
            SaveFileBase memorySave = FactionAPI.SaveToMemory();
            FactionRelationSaveFile saveFile = memorySave as FactionRelationSaveFile;
            FactionAPI.SetRelation<TestFactionRelation>(sourceFaction, target, 5);

            FactionAPI.Load(memorySave);

            Assert.IsNotNull(saveFile);
            Assert.AreEqual(1, saveFile.Entries.Length);
            Assert.AreEqual(FactionRelationTargetKind.RuntimeObject, saveFile.Entries[0].TargetKind);
            Assert.AreEqual(identifier, saveFile.Entries[0].TargetRuntimeIdentifier);
            Assert.AreEqual(50, FactionAPI.GetRelationValue<TestFactionRelation>(sourceFaction, target));
        }

        [Test]
        public void SaveToMemory_SkipsRuntimeTargetWithoutSnowflakeIdentifier()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestFactionRelation relationType = Track(ScriptableObject.CreateInstance<TestFactionRelation>());
            RelationTypeDatabase.RegisterForTests(relationType);
            GameObject targetObject = Track(new GameObject("Unidentified Player"));
            UnidentifiedRelatable target = targetObject.AddComponent<UnidentifiedRelatable>();
            FactionAPI.SetRelation<TestFactionRelation>(faction, target, 50);

            FactionRelationSaveFile saveFile = FactionAPI.SaveToMemory() as FactionRelationSaveFile;

            Assert.IsNotNull(saveFile);
            Assert.AreEqual(0, saveFile.Entries.Length);
        }

        private sealed class TestFactionRelation : RelationTypeBase { }

        private sealed class UnidentifiedRelatable : MonoBehaviour, IRelatable
        {
            [SerializeField] private List<RelationEntry> _relationEntries = new List<RelationEntry>();

            List<RelationEntry> IRelatable.RelationEntries => _relationEntries;
        }

        private sealed class IdentifiedRelatable : MonoBehaviour, IRelatable, IIdentifiable<Snowflake128>
        {
            [SerializeField] private List<RelationEntry> _relationEntries = new List<RelationEntry>();

            public Snowflake128 Identifier { get; set; }

            List<RelationEntry> IRelatable.RelationEntries => _relationEntries;

            public void Initialize(Snowflake128 identifier)
            {
                Identifier = identifier;
            }
        }
    }
}
