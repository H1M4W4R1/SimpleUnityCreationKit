using NUnit.Framework;
using Systems.SimpleSettings.Saving;

namespace Systems.SimpleSettings.Tests
{
    public sealed class SettingLifecycleTests : SimpleSettingsTestBase
    {
        [Test]
        public void Setting_SetApplyRevertAndReset_UpdateDirtyStateAndCallbacks()
        {
            NamedIntSetting setting = new NamedIntSetting("volume", 10);

            setting.Set(20);
            Assert.IsTrue(setting.IsDirty);
            Assert.AreEqual(20, setting.CurrentValue);
            Assert.AreEqual(10, setting.AppliedValue);
            Assert.AreEqual(1, setting.PreviewCount);
            Assert.AreEqual(20, setting.LastPreviewValue);

            setting.Apply();
            Assert.IsFalse(setting.IsDirty);
            Assert.AreEqual(20, setting.AppliedValue);
            Assert.AreEqual(1, setting.ApplyCount);
            Assert.AreEqual(20, setting.LastAppliedValue);

            setting.Set(30);
            setting.Revert();
            Assert.IsFalse(setting.IsDirty);
            Assert.AreEqual(20, setting.CurrentValue);
            Assert.AreEqual(3, setting.PreviewCount);

            setting.ResetToDefault();
            Assert.IsTrue(setting.IsDirty);
            Assert.AreEqual(10, setting.CurrentValue);
        }

        [Test]
        public void Setting_TryUndo_RestoresPreviousUnappliedValuesInOrder()
        {
            NamedIntSetting setting = new NamedIntSetting("difficulty", 1);

            setting.Set(2);
            setting.Set(3);

            Assert.IsTrue(setting.TryUndo());
            Assert.AreEqual(2, setting.CurrentValue);
            Assert.IsTrue(setting.TryUndo());
            Assert.AreEqual(1, setting.CurrentValue);
            Assert.IsFalse(setting.TryUndo());
        }

        [Test]
        public void Group_TryUndoLastChange_UsesMostRecentlyChangedSetting()
        {
            NamedIntSetting first = new NamedIntSetting("first", 1);
            NamedIntSetting second = new NamedIntSetting("second", 2);
            TestSettingsGroup group = new TestSettingsGroup("gameplay", "gameplay", first, second);

            first.Set(10);
            second.Set(20);

            Assert.IsTrue(group.TryUndoLastChange());
            Assert.AreEqual(10, first.CurrentValue);
            Assert.AreEqual(2, second.CurrentValue);

            Assert.IsTrue(group.TryUndoLastChange());
            Assert.AreEqual(1, first.CurrentValue);
            Assert.IsFalse(group.TryUndoLastChange());
        }

        [Test]
        public void Group_ApplyRevertResetAndDirtyState_AffectEverySetting()
        {
            NamedIntSetting first = new NamedIntSetting("first", 1);
            NamedIntSetting second = new NamedIntSetting("second", 2);
            TestSettingsGroup group = new TestSettingsGroup("gameplay", "gameplay", first, second);

            first.Set(10);
            second.Set(20);
            Assert.IsTrue(group.IsDirty);

            group.Apply();
            Assert.IsFalse(group.IsDirty);
            Assert.AreEqual(1, group.AppliedCount);
            Assert.AreEqual(10, first.AppliedValue);
            Assert.AreEqual(20, second.AppliedValue);

            first.Set(100);
            second.Set(200);
            group.Revert();
            Assert.AreEqual(10, first.CurrentValue);
            Assert.AreEqual(20, second.CurrentValue);

            group.ResetToDefaults();
            Assert.AreEqual(1, first.CurrentValue);
            Assert.AreEqual(2, second.CurrentValue);
        }

        [Test]
        public void Group_SaveAndLoad_RoundTripsMatchingSettingsAndIgnoresUnknownKeys()
        {
            NamedIntSetting first = new NamedIntSetting("first", 1);
            TextSetting text = new TextSetting("name", "default");
            ModeSetting mode = new ModeSetting("mode");
            TestSettingsGroup group = new TestSettingsGroup("gameplay", "gameplay", first, text, mode);

            first.Set(42);
            text.Set("custom");
            mode.Set(TestSettingsMode.High);

            SettingsSaveFile file = group.BuildSaveFile();
            file.Entries.Add(new SettingsSaveFile.SettingEntry { Key = "missing", Value = "999" });

            first.Set(7);
            text.Set("changed");
            mode.Set(TestSettingsMode.Low);

            group.ParseSaveFile(file);

            Assert.AreEqual(42, first.CurrentValue);
            Assert.AreEqual("custom", text.CurrentValue);
            Assert.AreEqual(TestSettingsMode.High, mode.CurrentValue);
            Assert.AreEqual(42, first.LastAppliedValue);
            Assert.AreEqual("custom", text.LastAppliedValue);
            Assert.AreEqual(TestSettingsMode.High, mode.LastAppliedValue);
            Assert.IsFalse(group.IsDirty);
        }

        [Test]
        public void StringSetting_Load_AllowsEmptyStringValues()
        {
            TextSetting text = new TextSetting("name", "default");
            TestSettingsGroup group = new TestSettingsGroup("gameplay", "gameplay", text);
            SettingsSaveFile file = new SettingsSaveFile { GroupId = "gameplay" };
            file.Entries.Add(new SettingsSaveFile.SettingEntry { Key = "name", Value = "" });

            text.Set("not empty");
            group.ParseSaveFile(file);

            Assert.AreEqual(string.Empty, text.CurrentValue);
            Assert.AreEqual(string.Empty, text.AppliedValue);
            Assert.AreEqual(string.Empty, text.LastAppliedValue);
        }
    }
}
