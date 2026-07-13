using System;
using JetBrains.Annotations;

namespace Systems.SimpleSettings.Abstract
{
    /// <summary>
    ///     Non-generic base interface for any configurable setting.
    /// </summary>
    /// <remarks>
    ///     Each concrete setting type is uniquely identified by its runtime type.
    ///     <see cref="Key"/> is automatically set to <c>GetType().Name</c> in
    ///     <see cref="Setting{TValue}"/>, so no manual key management is required
    ///     for standard settings.
    /// </remarks>
    public interface ISetting
    {
        /// <summary>
        ///     Unique key identifying this setting — automatically assigned as
        ///     <c>GetType().Name</c> by <see cref="Setting{TValue}"/>.
        /// </summary>
        [NotNull] string Key { get; }

        /// <summary>
        ///     ID of the <see cref="SettingGroupBase"/> that owns this setting.
        ///     Assigned by the group when the setting is registered.
        /// </summary>
        [NotNull] [UsedImplicitly] string GroupId { get; }

        /// <summary>Runtime type of this setting's value.</summary>
        [NotNull] Type ValueType { get; }

        /// <summary>
        ///     Whether <c>CurrentValue</c> differs from <c>AppliedValue</c>.
        ///     True means there are pending changes not yet committed via <see cref="Apply"/>.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        ///     Commits <c>CurrentValue</c> to <c>AppliedValue</c>, triggers
        ///     the engine effect, and clears the undo stack.
        /// </summary>
        void Apply();

        /// <summary>
        ///     Discards pending changes by restoring <c>CurrentValue</c> to <c>AppliedValue</c>.
        ///     Clears the undo stack.
        /// </summary>
        void Revert();

        /// <summary>
        ///     Restores <c>CurrentValue</c> to the factory <c>DefaultValue</c>.
        ///     Goes through <see cref="Setting{TValue}.Set"/> so undo is supported.
        /// </summary>
        void ResetToDefault();

        /// <summary>
        ///     Undoes the most recent unapplied change.
        /// </summary>
        /// <returns><c>true</c> if a change was undone; <c>false</c> if the undo stack is empty.</returns>
        bool TryUndo();

        /// <summary>Fires whenever <c>CurrentValue</c> changes (on Set, Revert, Undo).</summary>
        event Action OnValueChanged;

        /// <summary>Fires after <see cref="Apply"/> completes.</summary>
        event Action OnApplied;

        // ─── Internal API used by SettingGroupBase (same assembly) ───────────

        /// <summary>Assigns the owning group and wires the change-notification callback.</summary>
        internal void InitializeForGroup([NotNull] string groupId,
                                         [NotNull] Action<ISetting> notifyChanged);

        /// <summary>Serializes <c>CurrentValue</c> to a string for persistence.</summary>
        [CanBeNull] internal string SerializeCurrentValue();

        /// <summary>
        ///     Deserializes <paramref name="serialized"/> and calls <c>LoadValue</c>
        ///     (bypasses undo, applies engine effect).
        /// </summary>
        internal void DeserializeAndLoad([NotNull] string serialized);
    }
}
