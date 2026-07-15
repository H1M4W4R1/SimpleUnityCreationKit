using System.Collections.Generic;
#if UNITY_INCLUDE_TESTS
using System.Runtime.CompilerServices;
#endif
using JetBrains.Annotations;
using Systems.SimpleSaving.Utility;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Groups;
using Systems.SimpleSettings.Saving;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("SimpleSettings.Tests")]
#endif

namespace Systems.SimpleSettings.Core
{
    /// <summary>
    ///     Central MonoBehaviour singleton that owns all setting groups, coordinates
    ///     apply / revert / undo / reset operations, and handles disk persistence.
    /// </summary>
    /// <remarks>
    ///     Built-in groups (Graphics, Audio, Controls, Localization) are created
    ///     automatically on <c>Awake</c> based on the serialized enable-flags.
    ///     Custom groups should be registered via <see cref="RegisterGroup"/> before
    ///     <c>Awake</c> runs (use <c>[DefaultExecutionOrder(-100)]</c> on the registering
    ///     component, or register in another component's <c>Awake</c> that executes first).
    /// </remarks>
    public sealed class SettingsManager : MonoBehaviour
    {
        // ─────────────── Built-in group enable toggles ────────────────────
        [SerializeField] private bool _enableGraphics     = true;
        [SerializeField] private bool _enableAudio        = true;
        [SerializeField] private bool _enableControls     = true;
        [SerializeField] private bool _enableLocalization = true;

        // ─────────── Unity-object refs for built-in groups ────────────────
        [SerializeField, Tooltip("Required when Audio group is enabled.")]
        private AudioMixer _audioMixer;

        [SerializeField, Tooltip("Required when Controls group is enabled.")]
        private InputActionAsset _inputActions;

        // ──────────────────── Save configuration ──────────────────────────
        [SerializeField]
        private SaveMode _saveMode = SaveMode.SingleFile;

        [SerializeField, Tooltip("File name (no extension) used in SingleFile mode.")]
        private string _sharedFileName = "settings";

        // ─────────────────────── State ────────────────────────────────────
        private readonly List<SettingGroupBase> _groups = new();

        // Combined save file used in SingleFile mode, kept in memory to merge groups.
        private CombinedSettingsSaveFile _combinedFile;

        /// <summary>The active singleton instance.</summary>
        [CanBeNull] public static SettingsManager Instance { get; private set; }

        /// <summary>All currently registered setting groups.</summary>
        [NotNull] public IReadOnlyList<SettingGroupBase> Groups => _groups;

        // ─────────────────────── Lifecycle ────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Auto-create built-in groups based on enable flags.
            if (_enableGraphics)     RegisterGroup(new GraphicsSettingsGroup());
            if (_enableAudio)        RegisterGroup(new AudioSettingsGroup(_audioMixer));
            if (_enableControls)     RegisterGroup(new ControlsSettingsGroup(_inputActions));
            if (_enableLocalization) RegisterGroup(new LocalizationSettingsGroup());

            LoadAll();
        }

#if UNITY_INCLUDE_TESTS
        internal void AwakeForTests()
        {
            Awake();
        }
#endif

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ───────────────────── Group registration ─────────────────────────

        /// <summary>
        ///     Registers a group with this manager.
        ///     Custom groups should be registered before <c>Awake</c> finishes.
        /// </summary>
        public void RegisterGroup([NotNull] SettingGroupBase group)
        {
            _groups.Add(group);
        }

        /// <summary>
        ///     Returns the first registered group of type <typeparamref name="TGroup"/>,
        ///     or <c>null</c> if none is registered.
        /// </summary>
        [CanBeNull] public TGroup GetGroup<TGroup>() where TGroup : SettingGroupBase
        {
            for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
            {
                SettingGroupBase group = _groups[groupIndex];
                if (group is TGroup typed) return typed;
            }

            return null;
        }

        /// <summary>
        ///     Returns the first registered group with the given <paramref name="groupId"/>,
        ///     or <c>null</c> if not found.
        /// </summary>
        [CanBeNull] public SettingGroupBase GetGroup([NotNull] string groupId)
        {
            for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
            {
                SettingGroupBase group = _groups[groupIndex];
                if (group.GroupId == groupId) return group;
            }

            return null;
        }

        // ─────────────────────── Apply ────────────────────────────────────

        /// <summary>Applies all setting groups.</summary>
        public void ApplyAll()
        {
            for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
            {
                _groups[groupIndex].Apply();
            }
        }

        /// <summary>Applies the group with the given <paramref name="groupId"/>.</summary>
        public void ApplyGroup([NotNull] string groupId) => GetGroup(groupId)?.Apply();

        // ─────────────────────── Revert ───────────────────────────────────

        /// <summary>Reverts all setting groups to their last applied values.</summary>
        public void RevertAll()
        {
            for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
            {
                _groups[groupIndex].Revert();
            }
        }

        /// <summary>Reverts the group with the given <paramref name="groupId"/>.</summary>
        public void RevertGroup([NotNull] string groupId) => GetGroup(groupId)?.Revert();

        // ─────────────────────── Reset ────────────────────────────────────

        /// <summary>Resets all setting groups to their factory defaults.</summary>
        public void ResetAll()
        {
            for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
            {
                _groups[groupIndex].ResetToDefaults();
            }
        }

        /// <summary>Resets the group with the given <paramref name="groupId"/> to factory defaults.</summary>
        public void ResetGroup([NotNull] string groupId) => GetGroup(groupId)?.ResetToDefaults();

        // ─────────────────────── Undo ─────────────────────────────────────

        /// <summary>
        ///     Undoes the most recent unapplied change across <i>all</i> groups
        ///     (tries each group until one succeeds).
        /// </summary>
        /// <returns><c>true</c> if any group had a change to undo.</returns>
        public bool TryUndoAll()
        {
            for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
            {
                SettingGroupBase group = _groups[groupIndex];
                if (group.TryUndoLastChange()) return true;
            }

            return false;
        }

        /// <summary>
        ///     Undoes the most recent unapplied change in the group with the given
        ///     <paramref name="groupId"/>.
        /// </summary>
        /// <returns><c>true</c> if a change was undone.</returns>
        public bool TryUndoGroup([NotNull] string groupId) =>
            GetGroup(groupId)?.TryUndoLastChange() ?? false;

        // ─────────────────────── Save ─────────────────────────────────────

        /// <summary>Saves all registered groups to disk.</summary>
        public void SaveAll()
        {
            if (_saveMode == SaveMode.SingleFile)
                SaveAllSingleFile();
            else
            {
                for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
                {
                    SaveGroupPerFile(_groups[groupIndex]);
                }
            }
        }

        /// <summary>Saves the group with the given <paramref name="groupId"/> to disk.</summary>
        public void SaveGroup([NotNull] string groupId)
        {
            SettingGroupBase group = GetGroup(groupId);
            if (group == null) return;

            if (_saveMode == SaveMode.SingleFile)
                SaveGroupIntoSingleFile(group);
            else
                SaveGroupPerFile(group);
        }

        // ─────────────────────── Load ─────────────────────────────────────

        /// <summary>Loads all registered groups from disk.</summary>
        public void LoadAll()
        {
            if (_saveMode == SaveMode.SingleFile)
                LoadAllSingleFile();
            else
            {
                for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
                {
                    LoadGroupPerFile(_groups[groupIndex]);
                }
            }
        }

        /// <summary>Loads the group with the given <paramref name="groupId"/> from disk.</summary>
        public void LoadGroup([NotNull] string groupId)
        {
            SettingGroupBase group = GetGroup(groupId);
            if (group == null) return;

            if (_saveMode == SaveMode.SingleFile)
                LoadGroupFromSingleFile(group);
            else
                LoadGroupPerFile(group);
        }

        // ─────────────────────── Private helpers ──────────────────────────

        private void SaveAllSingleFile()
        {
            _combinedFile ??= new CombinedSettingsSaveFile();
            for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
            {
                SettingGroupBase group = _groups[groupIndex];
                SettingsSaveFile groupFile = (SettingsSaveFile)SaveAPI.Save(group);
                if (groupFile != null) _combinedFile.SetGroup(groupFile);
            }

            SettingsFileIO.WriteCombined(_combinedFile, _sharedFileName);
        }

        private void SaveGroupIntoSingleFile([NotNull] SettingGroupBase group)
        {
            // Load existing combined file if not yet in memory.
            if (_combinedFile == null)
                SettingsFileIO.TryReadCombined(_sharedFileName, out _combinedFile);
            _combinedFile ??= new CombinedSettingsSaveFile();

            SettingsSaveFile groupFile = (SettingsSaveFile)SaveAPI.Save(group);
            if (groupFile != null) _combinedFile.SetGroup(groupFile);

            SettingsFileIO.WriteCombined(_combinedFile, _sharedFileName);
        }

        private void SaveGroupPerFile([NotNull] SettingGroupBase group)
        {
            SettingsSaveFile groupFile = (SettingsSaveFile)SaveAPI.Save(group);
            if (groupFile != null) SettingsFileIO.WriteGroup(groupFile, group.SaveFileName);
        }

        private void LoadAllSingleFile()
        {
            if (!SettingsFileIO.TryReadCombined(_sharedFileName, out _combinedFile)) return;

            for (int groupIndex = 0; groupIndex < _groups.Count; groupIndex++)
            {
                SettingGroupBase group = _groups[groupIndex];
                if (!_combinedFile!.TryGetGroup(group.GroupId, out SettingsSaveFile file)) continue;
                SaveAPI.Load(group, file!);
            }
        }

        private void LoadGroupFromSingleFile([NotNull] SettingGroupBase group)
        {
            if (_combinedFile == null)
                SettingsFileIO.TryReadCombined(_sharedFileName, out _combinedFile);
            if (_combinedFile == null) return;

            if (_combinedFile.TryGetGroup(group.GroupId, out SettingsSaveFile file))
                SaveAPI.Load(group, file!);
        }

        private void LoadGroupPerFile([NotNull] SettingGroupBase group)
        {
            if (SettingsFileIO.TryReadGroup(group.SaveFileName, out SettingsSaveFile file))
                SaveAPI.Load(group, file!);
        }
    }
}
