using NUnit.Framework;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Core;
using Systems.SimpleSettings.Utility;

namespace Systems.SimpleSettings.Tests
{
    public sealed class SettingsManagerAndAPITests : SimpleSettingsTestBase
    {
        [Test]
        public void Manager_RegisterAndLookup_ReturnsGroupsByTypeAndId()
        {
            string fileName = CreateSettingsFileName("manager");
            SettingsManager manager = CreateManager(SaveMode.SingleFile, fileName);
            TestSettingsGroup group = new TestSettingsGroup(
                "gameplay", "gameplay", new NamedIntSetting("difficulty", 1));

            manager.RegisterGroup(group);

            Assert.AreSame(group, manager.GetGroup<TestSettingsGroup>());
            Assert.AreSame(group, manager.GetGroup("gameplay"));
            Assert.IsNull(manager.GetGroup("missing"));
        }

        [Test]
        public void SettingsAPI_LookupFindsRegisteredSettings()
        {
            string fileName = CreateSettingsFileName("api_lookup");
            SettingsManager manager = CreateManager(SaveMode.SingleFile, fileName);
            NamedIntSetting difficulty = new NamedIntSetting("difficulty", 1);
            TextSetting text = new TextSetting("name", "default");
            TestSettingsGroup group = new TestSettingsGroup("gameplay", "gameplay", difficulty, text);
            manager.RegisterGroup(group);

            NamedIntSetting foundByType = SettingsAPI.GetSetting<NamedIntSetting>();
            ISetting foundByKey = SettingsAPI.FindSetting("gameplay", "name");

            Assert.AreSame(difficulty, foundByType);
            Assert.AreSame(text, foundByKey);
            Assert.IsNull(SettingsAPI.FindSetting("gameplay", "missing"));
            Assert.IsNull(SettingsAPI.FindSetting("missing", "name"));
        }

        [Test]
        public void SettingsAPI_ApplyRevertResetAndUndo_CanTargetSingleGroup()
        {
            string fileName = CreateSettingsFileName("api_ops");
            SettingsManager manager = CreateManager(SaveMode.SingleFile, fileName);
            NamedIntSetting gameplay = new NamedIntSetting("difficulty", 1);
            NamedIntSetting audio = new NamedIntSetting("volume", 5);
            manager.RegisterGroup(new TestSettingsGroup("gameplay", "gameplay", gameplay));
            manager.RegisterGroup(new TestSettingsGroup("audio", "audio", audio));

            gameplay.Set(10);
            audio.Set(20);
            SettingsAPI.Apply("gameplay");

            Assert.AreEqual(10, gameplay.AppliedValue);
            Assert.AreEqual(5, audio.AppliedValue);

            gameplay.Set(30);
            audio.Set(40);
            Assert.IsTrue(SettingsAPI.TryUndo("audio"));
            Assert.AreEqual(30, gameplay.CurrentValue);
            Assert.AreEqual(20, audio.CurrentValue);

            SettingsAPI.Revert("gameplay");
            Assert.AreEqual(10, gameplay.CurrentValue);

            SettingsAPI.ResetToDefaults("audio");
            Assert.AreEqual(5, audio.CurrentValue);
        }

        [Test]
        public void SettingsAPI_GlobalOperations_ApplyToAllRegisteredGroups()
        {
            string fileName = CreateSettingsFileName("api_global");
            SettingsManager manager = CreateManager(SaveMode.SingleFile, fileName);
            NamedIntSetting first = new NamedIntSetting("first", 1);
            NamedIntSetting second = new NamedIntSetting("second", 2);
            manager.RegisterGroup(new TestSettingsGroup("first", "first", first));
            manager.RegisterGroup(new TestSettingsGroup("second", "second", second));

            first.Set(11);
            second.Set(22);
            SettingsAPI.ApplyAll();

            Assert.AreEqual(11, first.AppliedValue);
            Assert.AreEqual(22, second.AppliedValue);

            first.Set(111);
            second.Set(222);
            Assert.IsTrue(SettingsAPI.TryUndoAll());
            SettingsAPI.RevertAll();

            Assert.AreEqual(11, first.CurrentValue);
            Assert.AreEqual(22, second.CurrentValue);

            SettingsAPI.ResetAll();
            Assert.AreEqual(1, first.CurrentValue);
            Assert.AreEqual(2, second.CurrentValue);
        }

        [Test]
        public void Manager_SingleFileSaveAndLoad_MergesRegisteredGroups()
        {
            string fileName = CreateSettingsFileName("single");
            SettingsManager manager = CreateManager(SaveMode.SingleFile, fileName);
            NamedIntSetting first = new NamedIntSetting("first", 1);
            NamedIntSetting second = new NamedIntSetting("second", 2);
            manager.RegisterGroup(new TestSettingsGroup("first", "first", first));
            manager.RegisterGroup(new TestSettingsGroup("second", "second", second));

            first.Set(10);
            second.Set(20);
            SettingsAPI.SaveAll();

            first.Set(100);
            second.Set(200);
            SettingsAPI.LoadAll();

            Assert.AreEqual(10, first.CurrentValue);
            Assert.AreEqual(20, second.CurrentValue);
        }

        [Test]
        public void Manager_PerGroupSaveAndLoad_UsesEachGroupSaveFileName()
        {
            string sharedFileName = CreateSettingsFileName("unused_shared");
            string groupFileName = CreateSettingsFileName("per_group");
            SettingsManager manager = CreateManager(SaveMode.PerGroup, sharedFileName);
            NamedIntSetting setting = new NamedIntSetting("difficulty", 1);
            manager.RegisterGroup(new TestSettingsGroup("gameplay", groupFileName, setting));

            setting.Set(9);
            SettingsAPI.Save("gameplay");

            setting.Set(99);
            SettingsAPI.Load("gameplay");

            Assert.AreEqual(9, setting.CurrentValue);
        }
    }
}
