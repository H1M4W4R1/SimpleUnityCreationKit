using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Utility;
using UnityEngine;

namespace Systems.SimpleSettings.UI
{
    /// <summary>
    ///     Serializable helper that identifies a single <see cref="ISetting"/> by its
    ///     group ID and key.  Attach to any UI setting component and use the custom
    ///     inspector dropdown to select which setting to bind to.
    /// </summary>
    /// <remarks>
    ///     The resolved <see cref="ISetting"/> is cached after the first call to
    ///     <see cref="Resolve"/>, so subsequent calls are O(1).
    ///     Call <see cref="Invalidate"/> to force re-resolution (e.g. after a
    ///     scene reload or group re-registration).
    /// </remarks>
    [System.Serializable]
    public sealed class SettingBinding
    {
        [SerializeField] private string _groupId;
        [SerializeField] private string _settingKey;

        private ISetting _cached;

        /// <summary>ID of the group that owns the setting.</summary>
        [NotNull] public string GroupId
        {
            get => _groupId ?? string.Empty;
            set { _groupId = value; Invalidate(); }
        }

        /// <summary>Key of the setting within the group (usually the concrete class name).</summary>
        [NotNull] public string SettingKey
        {
            get => _settingKey ?? string.Empty;
            set { _settingKey = value; Invalidate(); }
        }

        /// <summary>
        ///     Resolves and returns the <see cref="ISetting"/> identified by
        ///     <see cref="GroupId"/> and <see cref="SettingKey"/>.
        ///     Returns <c>null</c> if the setting is not found or the
        ///     <see cref="SettingsAPI"/> has no active manager.
        /// </summary>
        [CanBeNull] public ISetting Resolve()
        {
            if (_cached != null) return _cached;
            if (string.IsNullOrEmpty(_groupId) || string.IsNullOrEmpty(_settingKey))
                return null;

            _cached = SettingsAPI.FindSetting(_groupId, _settingKey);
            if (_cached == null)
                Debug.LogWarning("[SimpleSettings] SettingBinding could not resolve " +
                                 $"'{_groupId}/{_settingKey}'.");
            return _cached;
        }

        /// <summary>
        ///     Resolves the setting and attempts to cast it to
        ///     <typeparamref name="TSetting"/>.
        /// </summary>
        [CanBeNull] public TSetting Resolve<TSetting>() where TSetting : class, ISetting =>
            Resolve() as TSetting;

        /// <summary>
        ///     Clears the cached reference so the next <see cref="Resolve"/> call
        ///     performs a fresh lookup.
        /// </summary>
        public void Invalidate() => _cached = null;

        /// <summary>Whether this binding has non-empty GroupId and SettingKey.</summary>
        public bool IsConfigured =>
            !string.IsNullOrEmpty(_groupId) && !string.IsNullOrEmpty(_settingKey);
    }
}
