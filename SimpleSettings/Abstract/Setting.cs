using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleSettings.Abstract
{
    /// <summary>
    ///     Abstract base for a single configurable value of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of value this setting holds.</typeparam>
    /// <remarks>
    ///     <para>
    ///         <b>Key</b> is automatically assigned to <c>GetType().Name</c>, so each
    ///         concrete subclass is uniquely identified as long as only one instance
    ///         of each concrete type exists per group.
    ///         For settings with multiple instances (e.g. <c>InputBindingSetting</c>),
    ///         override <see cref="Key"/> in the constructor.
    ///     </para>
    ///     <para>
    ///         <b>Undo</b> tracks unapplied changes. Calling <see cref="Apply"/> or
    ///         <see cref="Revert"/> clears the undo stack for this setting.
    ///     </para>
    ///     <para>
    ///         <b>Live preview</b> is opt-in: override <see cref="OnCurrentValueChanged"/>
    ///         to apply an immediate effect (e.g. move camera while dragging FoV slider).
    ///         <see cref="OnApplyInternal"/> remains the canonical persistence point.
    ///     </para>
    ///     <para>
    ///         <b>Serialization</b> uses <see cref="SerializeValue"/> /
    ///         <see cref="TryDeserializeValue"/> which handle all primitive types, enums,
    ///         and strings by default. Override both for custom struct types (e.g. <c>Resolution</c>).
    ///     </para>
    /// </remarks>
    public abstract class Setting<TValue> : ISetting
    {
        private readonly Stack<TValue> _undoStack = new();
        private Action<ISetting> _notifyChangedCallback;

        // ──────────────────────────── ISetting ────────────────────────────

        /// <inheritdoc/>
        public string Key { get; protected set; }

        /// <inheritdoc/>
        public string GroupId { get; private set; }

        /// <inheritdoc/>
        public Type ValueType => typeof(TValue);

        /// <inheritdoc/>
        public bool IsDirty => !ValuesEqual(CurrentValue, AppliedValue);

        /// <inheritdoc/>
        public event Action OnValueChanged;

        /// <inheritdoc/>
        public event Action OnApplied;

        // ─────────────────────────── Properties ───────────────────────────

        /// <summary>The live (possibly unapplied) current value.</summary>
        public TValue CurrentValue { get; private set; }

        /// <summary>The last successfully applied value (revert target).</summary>
        public TValue AppliedValue { get; private set; }

        /// <summary>Factory default returned by <see cref="ResetToDefault"/>.</summary>
        public TValue DefaultValue { get; }

        // ──────────────────────────── Constructor ─────────────────────────

        /// <summary>
        ///     Creates a new setting. <see cref="Key"/> is automatically assigned to
        ///     <c>GetType().Name</c> — override it in the derived constructor for
        ///     settings that need multiple instances with distinct keys.
        /// </summary>
        /// <param name="defaultValue">The initial and factory-default value.</param>
        protected Setting(TValue defaultValue)
        {
            Key          = GetType().Name;
            DefaultValue = defaultValue;
            CurrentValue = defaultValue;
            AppliedValue = defaultValue;
            GroupId = string.Empty;
        }

        // ────────────────────────── Public API ────────────────────────────

        /// <summary>
        ///     Updates <see cref="CurrentValue"/>, records the previous value on the
        ///     undo stack, and fires <see cref="OnValueChanged"/>.
        /// </summary>
        public void Set(TValue value)
        {
            _undoStack.Push(CurrentValue);
            CurrentValue = value;
            OnCurrentValueChanged(value);
            OnValueChanged?.Invoke();
            _notifyChangedCallback?.Invoke(this);
        }

        /// <inheritdoc/>
        public void Apply()
        {
            AppliedValue = CurrentValue;
            _undoStack.Clear();
            OnApplyInternal(CurrentValue);
            OnApplied?.Invoke();
        }

        /// <inheritdoc/>
        public void Revert()
        {
            CurrentValue = AppliedValue;
            _undoStack.Clear();
            OnCurrentValueChanged(AppliedValue);
            OnValueChanged?.Invoke();
        }

        /// <inheritdoc/>
        public void ResetToDefault() => Set(DefaultValue);

        /// <inheritdoc/>
        public bool TryUndo()
        {
            if (!_undoStack.TryPop(out TValue previous)) return false;
            CurrentValue = previous;
            OnCurrentValueChanged(previous);
            OnValueChanged?.Invoke();
            return true;
        }

        // ─────────────────────────── Abstracts ───────────────────────────

        /// <summary>
        ///     Called when <see cref="Apply"/> is invoked.
        ///     Apply the engine effect here (e.g. <c>QualitySettings.SetQualityLevel(value)</c>).
        /// </summary>
        protected abstract void OnApplyInternal(TValue value);

        // ─────────────────────────── Virtuals ────────────────────────────

        /// <summary>
        ///     Called whenever <see cref="CurrentValue"/> changes (Set, Revert, Undo).
        ///     Override to apply a live-preview effect before the user hits Apply.
        ///     Default implementation does nothing.
        /// </summary>
        protected virtual void OnCurrentValueChanged(TValue value) { }

        /// <summary>
        ///     Serializes <see cref="CurrentValue"/> to a string for persistence.
        ///     Default handles: <c>string</c>, primitives, and enums.
        ///     Override for custom struct types (e.g. <c>Resolution</c>).
        /// </summary>
        [CanBeNull]
        protected virtual string SerializeValue()
        {
            TValue val = CurrentValue;
            if (val == null) return null;
            if (typeof(TValue).IsEnum)
                return ((int)(object)val).ToString();
            return val.ToString();
        }

        /// <summary>
        ///     Deserializes a string produced by <see cref="SerializeValue"/> back into
        ///     a <typeparamref name="TValue"/>. Default handles primitives, enums, and strings.
        ///     Override for custom struct types.
        /// </summary>
        protected virtual bool TryDeserializeValue([NotNull] string serialized, out TValue value)
        {
            value = default;

            try
            {
                Type t = typeof(TValue);

                if (t == typeof(string))
                {
                    value = (TValue)(object)serialized;
                    return true;
                }

                if (string.IsNullOrEmpty(serialized)) return false;

                if (t.IsEnum)
                {
                    value = (TValue)Enum.Parse(t, serialized);
                    return true;
                }

                value = (TValue)Convert.ChangeType(serialized,
                    Nullable.GetUnderlyingType(t) ?? t);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SimpleSettings] Failed to deserialize '{serialized}' " +
                                 $"for setting '{Key}' ({typeof(TValue).Name}): {e.Message}");
                return false;
            }
        }

        /// <summary>
        ///     Equality comparison for <typeparamref name="TValue"/>.
        ///     Override for custom equality semantics (e.g. floating-point tolerance).
        /// </summary>
        protected virtual bool ValuesEqual(TValue a, TValue b) =>
            EqualityComparer<TValue>.Default.Equals(a, b);

        // ─────────────────── Internal (used by SettingGroupBase) ──────────

        /// <summary>
        ///     Sets both <see cref="CurrentValue"/> and <see cref="AppliedValue"/> directly,
        ///     bypassing the undo stack, then calls <see cref="OnApplyInternal"/> to apply
        ///     the engine effect. Used exclusively by <see cref="SettingGroupBase"/> during loading.
        /// </summary>
        internal void LoadValue(TValue value)
        {
            CurrentValue = value;
            AppliedValue = value;
            _undoStack.Clear();
            OnApplyInternal(value);
        }

        // ─────────────── Explicit ISetting internal implementation ─────────

        void ISetting.InitializeForGroup(string groupId, Action<ISetting> notifyChanged)
        {
            GroupId                = groupId;
            _notifyChangedCallback = notifyChanged;
        }

        string ISetting.SerializeCurrentValue() => SerializeValue();

        void ISetting.DeserializeAndLoad(string serialized)
        {
            if (TryDeserializeValue(serialized, out TValue value))
                LoadValue(value);
        }
    }
}
