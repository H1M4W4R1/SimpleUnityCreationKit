# SimpleSettings

A comprehensive, extensible runtime settings system for Unity games. SimpleSettings provides apply/revert/undo/default operations at per-setting, per-group, and global scope, with built-in support for graphics, audio, input bindings, and localization. Integrates with SimpleSaving's save pipeline and SimpleUI's component system.

## Requirements

- **Unity 2022.3+**
- **SimpleCore** assembly
- **SimpleUI** assembly
- **Unity.InputSystem** package
- **Unity.Localization** package
- **TextMeshPro**

### Assembly Definition
- `SimpleSettings.asmdef`

---

## Architecture

```
Setting<TValue>          — generic base; owns CurrentValue / AppliedValue / undo stack
SettingGroupBase         — owns a collection of settings; ISaveData<SettingsSaveFile>
SettingsManager          — MonoBehaviour singleton; registers groups; drives save/load
SettingsAPI              — static facade for all operations
```

Settings expose four operations:

| Operation | Effect |
|---|---|
| `Apply()` | Commits `CurrentValue` → `AppliedValue`; fires engine effect; clears undo stack |
| `Revert()` | Restores `CurrentValue` ← `AppliedValue`; clears undo stack |
| `ResetToDefault()` | Calls `Set(DefaultValue)` |
| `TryUndo()` | Rolls back the last `Set()` call; returns `false` when stack is empty |

All four operations are available per-setting, per-group (`SettingGroupBase`), and globally (`SettingsAPI`).

---

## Quick Start

### Scene Setup

Add `SettingsManager` to a persistent GameObject and configure it in the Inspector:

```
SettingsManager
├── Enable Graphics   ✓
├── Enable Audio      ✓   → assign AudioMixer
├── Enable Controls   ✓   → assign InputActionAsset
├── Enable Localization ✓
├── Save Mode         SingleFile
└── Shared File Name  "settings"
```

Settings are loaded automatically on `Awake`. The save file is written to `Application.persistentDataPath/Settings/`.

### Example Scene

Open `Examples/Scene - Settings.unity` and press Play to try a small runtime settings panel. The scene registers an example gameplay group, then creates UI for a difficulty slider, hints toggle, and Apply/Revert/Undo/Reset actions.

---

## Built-in Settings

### Graphics (`GroupId = "graphics"`)

| Setting | Type | UI | Engine effect |
|---|---|---|---|
| `FieldOfViewSetting` | `float` | Slider 60–120 | `Camera.main.fieldOfView` |
| `ResolutionSetting` | `Resolution` | Dropdown | `Screen.SetResolution` |
| `QualityLevelSetting` | `int` | Dropdown | `QualitySettings.SetQualityLevel` |
| `FullscreenModeSetting` | `FullScreenMode` | Dropdown | `Screen.fullScreenMode` |
| `VSyncSetting` | `bool` | Toggle | `QualitySettings.vSyncCount` |
| `FrameCapSetting` | `int` | Dropdown | `Application.targetFrameRate` |

### Audio (`GroupId = "audio"`)

Requires an `AudioMixer` with float parameters `MasterVolume`, `MusicVolume`, `SfxVolume`, `VoiceVolume`. Values are stored as linear `[0, 1]` and converted to dB on apply.

| Setting | Mixer parameter |
|---|---|
| `MasterVolumeSetting` | `MasterVolume` |
| `MusicVolumeSetting` | `MusicVolume` |
| `SfxVolumeSetting` | `SfxVolume` |
| `VoiceVolumeSetting` | `VoiceVolume` |

### Controls (`GroupId = "controls"`)

One `InputBindingSetting` is created per (action, bindingIndex) pair found in the provided `InputActionAsset`. Composite part bindings are skipped. On apply, `action.ApplyBindingOverride(bindingIndex, path)` is called.

### Localization (`GroupId = "localization"`)

`LanguageSetting` stores the locale identifier string and calls `LocalizationSettings.SelectedLocale` on apply. Options are sourced from `LocalizationSettings.AvailableLocales`.

---

## Custom Settings

### 1 — Create the setting

```csharp
using Systems.SimpleSettings.Abstract;

public sealed class MasterDifficultySetting : Setting<int>, ISliderSetting
{
    public float MinValue => 1;
    public float MaxValue => 5;
    public float Step     => 1;

    public MasterDifficultySetting() : base(defaultValue: 2) { }

    protected override void OnApplyInternal(int value)
    {
        GameRules.DifficultyLevel = value;
    }
}
```

The setting `Key` is automatically `"MasterDifficultySetting"` (the class name). No manual key assignment needed unless multiple instances of the same concrete type exist (see `InputBindingSetting` for that pattern).

### 2 — Create the group

```csharp
using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;

public sealed class GameplaySettingsGroup : SettingGroupBase
{
    public override string GroupId => "gameplay";

    private readonly MasterDifficultySetting _difficulty = new MasterDifficultySetting();
    private readonly ISetting[] _settings;

    public GameplaySettingsGroup()
    {
        _settings = new ISetting[] { _difficulty };
        RegisterSettings(_settings);
    }

    protected override IEnumerable<ISetting> GetSettings()
    {
        return _settings;
    }
}
```

### 3 — Register the group

Register before `SettingsManager.Awake` fires (use `[DefaultExecutionOrder]`):

```csharp
[DefaultExecutionOrder(-200)]
public sealed class MyGameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        SettingsManager.Instance.RegisterGroup(new GameplaySettingsGroup());
    }
}
```

---

## SettingsAPI

```csharp
using Systems.SimpleSettings.Utility;

// Global operations
SettingsAPI.ApplyAll();
SettingsAPI.RevertAll();
SettingsAPI.ResetAll();
SettingsAPI.TryUndoAll();
SettingsAPI.SaveAll();
SettingsAPI.LoadAll();

// Per-group operations
SettingsAPI.Apply("audio");
SettingsAPI.Revert("audio");
SettingsAPI.ResetToDefaults("audio");
SettingsAPI.TryUndo("audio");
SettingsAPI.Save("audio");

// Type-safe setting lookup
FieldOfViewSetting fov = SettingsAPI.GetSetting<FieldOfViewSetting>();
fov.Set(90f);

// Key-based lookup (for dynamic/runtime settings)
ISetting s = SettingsAPI.FindSetting("controls", "Jump/0");
```

---

## UI Components

All UI components extend a single SimpleUI base class and implement `IWithLocalContext<ISetting>`. Bind them to settings via the `SettingBinding` inspector field (GroupId + Key dropdowns).

| Component | Base | Use for |
|---|---|---|
| `UIFloatSliderSetting` | `UISliderBase` | `Setting<float>` + `ISliderSetting` |
| `UIIntSliderSetting` | `UISliderBase` | `Setting<int>` + `ISliderSetting` |
| `UIToggleSetting` | `UIToggleBase` | `Setting<bool>` + `IToggleSetting` |
| `UIInputFieldSetting` | `UIInputFieldBase` | `Setting<string>` |
| `UIDropdownSetting<T>` | `UIDropdownSelectorBase<T>` | Any `ISelectableSetting<T>` |
| `UIKeybindButton` | `UIButtonBase` | `InputBindingSetting` |

### Dropdown

`UIDropdownSetting<T>` is abstract — create a concrete sealed subclass per value type:

```csharp
public sealed class UIQualityDropdown : UIDropdownSetting<int> { }
public sealed class UIResolutionDropdown : UIDropdownSetting<Resolution> { }
```

### Action Buttons

All extend `UIButtonBase`. Set `_groupId` to scope to one group; leave empty for all groups.

| Component | Action |
|---|---|
| `UISettingsApplyButton` | `SettingsAPI.Apply` / `ApplyAll` |
| `UISettingsRevertButton` | `SettingsAPI.Revert` / `RevertAll` |
| `UISettingsResetButton` | `SettingsAPI.ResetToDefaults` / `ResetAll` |
| `UISettingsUndoButton` | `SettingsAPI.TryUndo` / `TryUndoAll` — auto-disables when nothing to undo |

### Settings Window

`UISettingsWindow` extends `UIWindowBase`. Configure `_dirtyClosePolicy` to control what happens when the window is closed with unapplied changes:

| Policy | Behaviour |
|---|---|
| `Revert` *(default)* | Reverts all unapplied changes |
| `Apply` | Applies and saves all changes |
| `Ignore` | Leaves settings as-is |

### Group Panel Base

Extend `UISettingGroupPanelBase` to build per-group settings panels:

```csharp
public sealed class AudioSettingsPanel : UISettingGroupPanelBase
{
    protected override void OnGroupResolved(SettingGroupBase group)
    {
        // group is the AudioSettingsGroup — subscribe to events, populate UI, etc.
    }
}
```

---

## Live Preview

Override `OnCurrentValueChanged` to apply a real-time preview while the user drags a slider, before they hit Apply:

```csharp
protected override void OnCurrentValueChanged(float value)
{
    Camera mainCamera = Camera.main;
    if (mainCamera)
    {
        mainCamera.fieldOfView = value;
    }
}
```

`OnApplyInternal` remains the authoritative persistence point and is only called on `Apply()`.

---

## Save Modes

| Mode | Behaviour |
|---|---|
| `SingleFile` *(default)* | All groups merged into `<sharedFileName>.json` |
| `PerGroup` | Each group saved to its own `<group.SaveFileName>.json` |

Override `SaveFileName` on a group to control the filename in `PerGroup` mode:

```csharp
public override string SaveFileName => "gameplay";
```
