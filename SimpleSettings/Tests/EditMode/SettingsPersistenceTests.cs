using System.IO;
using NUnit.Framework;
using Systems.SimpleSettings.Saving;

namespace Systems.SimpleSettings.Tests
{
    public sealed class SettingsPersistenceTests : SimpleSettingsTestBase
    {
        [Test]
        public void CombinedSaveFile_SetGroup_AddsAndReplacesGroupsById()
        {
            CombinedSettingsSaveFile combined = new CombinedSettingsSaveFile();
            SettingsSaveFile original = new SettingsSaveFile { GroupId = "audio" };
            SettingsSaveFile replacement = new SettingsSaveFile { GroupId = "audio" };
            replacement.Entries.Add(new SettingsSaveFile.SettingEntry { Key = "volume", Value = "0.5" });

            combined.SetGroup(original);
            combined.SetGroup(replacement);

            Assert.AreEqual(1, combined.Groups.Count);
            Assert.IsTrue(combined.TryGetGroup("audio", out SettingsSaveFile found));
            Assert.AreSame(replacement, found);
            Assert.IsFalse(combined.TryGetGroup("missing", out SettingsSaveFile missing));
            Assert.IsNull(missing);
        }

        [Test]
        public void SettingsFileIO_WriteAndReadGroup_RoundTripsJson()
        {
            string fileName = CreateSettingsFileName("group");
            SettingsSaveFile file = new SettingsSaveFile { GroupId = "gameplay" };
            file.Entries.Add(new SettingsSaveFile.SettingEntry { Key = "difficulty", Value = "3" });

            SettingsFileIO.WriteGroup(file, fileName);

            Assert.IsTrue(File.Exists(GetSettingsPath(fileName)));
            Assert.IsTrue(SettingsFileIO.TryReadGroup(fileName, out SettingsSaveFile loaded));
            Assert.AreEqual("gameplay", loaded.GroupId);
            Assert.AreEqual(1, loaded.Entries.Count);
            Assert.AreEqual("difficulty", loaded.Entries[0].Key);
            Assert.AreEqual("3", loaded.Entries[0].Value);
        }

        [Test]
        public void SettingsFileIO_WriteAndReadCombined_RoundTripsJson()
        {
            string fileName = CreateSettingsFileName("combined");
            CombinedSettingsSaveFile combined = new CombinedSettingsSaveFile();
            SettingsSaveFile group = new SettingsSaveFile { GroupId = "graphics" };
            group.Entries.Add(new SettingsSaveFile.SettingEntry { Key = "fps", Value = "60" });
            combined.SetGroup(group);

            SettingsFileIO.WriteCombined(combined, fileName);

            Assert.IsTrue(SettingsFileIO.TryReadCombined(fileName, out CombinedSettingsSaveFile loaded));
            Assert.AreEqual(1, loaded.Groups.Count);
            Assert.AreEqual("graphics", loaded.Groups[0].GroupId);
            Assert.AreEqual("fps", loaded.Groups[0].Entries[0].Key);
        }

        [Test]
        public void SettingsFileIO_WhenFileIsMissing_ReturnsFalseAndNullFile()
        {
            string fileName = CreateSettingsFileName("missing");

            Assert.IsFalse(SettingsFileIO.TryReadGroup(fileName, out SettingsSaveFile groupFile));
            Assert.IsNull(groupFile);
            Assert.IsFalse(SettingsFileIO.TryReadCombined(fileName, out CombinedSettingsSaveFile combinedFile));
            Assert.IsNull(combinedFile);
        }
    }
}
