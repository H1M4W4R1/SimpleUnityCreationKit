using NUnit.Framework;
using Systems.SimpleFactions.Editor.Automation;
using Systems.SimpleFactions.Examples;
using UnityEditor;

namespace Systems.SimpleFactions.Tests
{
    public sealed class FactionEditorAutomationTests : SimpleFactionsTestBase
    {
        [Test]
        public void FactionLevelAssigner_WhenGeneratedLevelAlreadyAssigned_DoesNotReportDirty()
        {
            ExampleFaction faction =
                AssetDatabase.LoadAssetAtPath<ExampleFaction>("Assets/Generated/Factions/ExampleFaction.asset");
            ExampleReputationLevel level =
                AssetDatabase.LoadAssetAtPath<ExampleReputationLevel>(
                    "Assets/Generated/ReputationLevels/ExampleReputationLevel.asset");

            Assert.IsFalse(ReferenceEquals(faction, null));
            Assert.IsFalse(ReferenceEquals(level, null));
            Assert.AreSame(level, faction.Levels[0]);

            bool changed = FactionLevelAssigner.AssignAllForTests();

            Assert.IsFalse(changed);
        }
    }
}
