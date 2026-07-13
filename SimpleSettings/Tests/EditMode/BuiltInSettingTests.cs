using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Groups;
using Systems.SimpleSettings.Settings.Controls;
using Systems.SimpleSettings.Settings.Graphics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems.SimpleSettings.Tests
{
    public sealed class BuiltInSettingTests : SimpleSettingsTestBase
    {
        [Test]
        public void FrameCapSetting_ExposesExpectedOptionsAndAppliesTargetFrameRate()
        {
            int previousFrameRate = Application.targetFrameRate;
            FrameCapSetting setting = new FrameCapSetting();

            try
            {
                IReadOnlyList<int> typedOptions = setting.GetTypedOptions();
                IReadOnlyList<object> boxedOptions = setting.GetOptions();

                Assert.AreEqual(6, typedOptions.Count);
                Assert.AreEqual(6, boxedOptions.Count);
                Assert.AreEqual(60, setting.CurrentValue);
                Assert.AreEqual("Unlimited", setting.GetTypedOptionLabel(-1));

                setting.Set(144);
                setting.Apply();

                Assert.AreEqual(144, Application.targetFrameRate);
                Assert.AreEqual(3, setting.SelectedIndex);
            }
            finally
            {
                Application.targetFrameRate = previousFrameRate;
            }
        }

        [Test]
        public void VSyncSetting_AppliesQualitySettingsVSyncCount()
        {
            int previousVSyncCount = QualitySettings.vSyncCount;
            VSyncSetting setting = new VSyncSetting();

            try
            {
                setting.Set(false);
                setting.Apply();
                Assert.AreEqual(0, QualitySettings.vSyncCount);

                setting.Set(true);
                setting.Apply();
                Assert.AreEqual(1, QualitySettings.vSyncCount);
            }
            finally
            {
                QualitySettings.vSyncCount = previousVSyncCount;
            }
        }

        [Test]
        public void InputBindingSetting_AppliesAndRemovesBindingOverride()
        {
            InputAction action = new InputAction("Jump", binding: "<Keyboard>/space");

            try
            {
                InputBindingSetting setting = new InputBindingSetting(action, 0);

                setting.Set("<Keyboard>/enter");
                setting.Apply();
                Assert.AreEqual("<Keyboard>/enter", action.bindings[0].overridePath);

                setting.Set(string.Empty);
                setting.Apply();
                Assert.IsTrue(string.IsNullOrEmpty(action.bindings[0].overridePath));
            }
            finally
            {
                action.Dispose();
            }
        }

        [Test]
        public void ControlsSettingsGroup_CreatesBindingsAndSkipsCompositeParts()
        {
            InputActionAsset asset = Track(ScriptableObject.CreateInstance<InputActionAsset>());
            InputActionMap map = new InputActionMap("Gameplay");
            InputAction move = map.AddAction("Move");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s");
            map.AddAction("Jump", binding: "<Keyboard>/space");
            asset.AddActionMap(map);

            ControlsSettingsGroup group = new ControlsSettingsGroup(asset);

            Assert.AreEqual(2, CountSettings(group));
            Assert.IsNotNull(group.GetBinding("Move", 0));
            Assert.IsNull(group.GetBinding("Move", 1));
            Assert.IsNotNull(group.GetBinding("Jump", 0));
        }

        [Test]
        public void BuiltInGroups_RegisterExpectedSettings()
        {
            GraphicsSettingsGroup graphics = new GraphicsSettingsGroup();
            AudioSettingsGroup audio = new AudioSettingsGroup(null);
            LocalizationSettingsGroup localization = new LocalizationSettingsGroup();

            Assert.AreEqual("graphics", graphics.GroupId);
            Assert.AreEqual(6, CountSettings(graphics));
            Assert.AreEqual("audio", audio.GroupId);
            Assert.AreEqual(4, CountSettings(audio));
            Assert.AreEqual("localization", localization.GroupId);
            Assert.AreEqual(1, CountSettings(localization));
            Assert.IsInstanceOf<ISliderSetting>(graphics.FieldOfView);
            Assert.IsInstanceOf<IToggleSetting>(graphics.VSync);
        }
    }
}
