using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Core;
using Systems.SimpleUI.Components.Panels;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Abstract
{
    /// <summary>
    ///     Abstract panel base for UI panels that represent a single settings group.
    ///     Resolves the group from <see cref="SettingsManager"/> using <see cref="_groupId"/>.
    /// </summary>
    /// <remarks>
    ///     Subclasses override <see cref="OnGroupResolved"/> to perform initial setup
    ///     once the group becomes available, and may call <see cref="Group"/> at any
    ///     time thereafter to read group state.
    /// </remarks>
    public abstract class UISettingGroupPanelBase : UIPanelBase
    {
        [SerializeField, Tooltip("Group ID this panel targets. Must match a registered group's GroupId.")]
        private string _groupId;

        private SettingGroupBase _group;

        // ─────────────────────── Protected API ────────────────────────────

        /// <summary>
        ///     The settings group this panel targets, or <c>null</c> if the group has
        ///     not yet been resolved (e.g. <see cref="SettingsManager"/> not yet awake).
        /// </summary>
        [CanBeNull] protected SettingGroupBase Group
        {
            get
            {
                if (_group != null) return _group;
                if (!SettingsManager.Instance) return null;

                _group = SettingsManager.Instance.GetGroup(_groupId);
                if (_group != null) OnGroupResolved(_group);
                return _group;
            }
        }

        /// <summary>
        ///     The group ID this panel is configured to target.
        /// </summary>
        protected string GroupId => _groupId;

        // ─────────────────────── Lifecycle ────────────────────────────────

        /// <inheritdoc/>
        protected override void OnRefresh()
        {
            base.OnRefresh();

            // Eagerly resolve the group on the first refresh so subclasses
            // do not need to call Group themselves to trigger OnGroupResolved.
            _ = Group;
        }

        // ─────────────────────── Extension points ─────────────────────────

        /// <summary>
        ///     Called once, the first time the group is successfully resolved from
        ///     <see cref="SettingsManager"/>. Override to subscribe to group events
        ///     or populate child UI elements.
        /// </summary>
        /// <param name="group">The resolved group (never null).</param>
        protected virtual void OnGroupResolved(SettingGroupBase group) { }
    }
}
