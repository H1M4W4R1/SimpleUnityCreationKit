using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;

namespace Systems.SimpleUI.Components.Text
{
    /// <summary>
    ///     Drives a <see cref="TextMeshProUGUI"/> from a <see cref="LocalizedString"/> that uses
    ///     named smart-string variables, refreshing automatically on locale change or context change.
    /// </summary>
    /// <typeparam name="TContext">
    ///     The data object whose fields are mapped to smart-string variables.
    ///     Provide it via a <see cref="Systems.SimpleUI.Context.Abstract.ContextProviderBase{T}"/>
    ///     anywhere in the parent hierarchy.
    /// </typeparam>
    /// <remarks>
    ///     Subclass usage:
    ///     <code>
    ///     public class PlayerStatusText : UILocalizedTextWithValuesObject&lt;PlayerData&gt;
    ///     {
    ///         [SerializeField] private StringVariable _name = new();
    ///         [SerializeField] private IntVariable    _level = new();
    ///
    ///         protected override void OnRegisterVariables()
    ///         {
    ///             LocalizedString.Add("playerName", _name);
    ///             LocalizedString.Add("level",      _level);
    ///         }
    ///
    ///         protected override void OnApplyVariables(PlayerData context)
    ///         {
    ///             _name.Value  = context.Name;
    ///             _level.Value = context.Level;
    ///         }
    ///     }
    ///     </code>
    ///     The smart string entry in the Localization table would then look like:
    ///     <c>{playerName} — Level {level}</c>
    /// </remarks>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class UILocalizedTextWithValuesObject<TContext> :
        UIObjectWithContextBase<TContext>,
        IRenderable<TContext>
    {
        /// <summary>
        ///     The localized string entry. Configure the table reference and entry key in the Inspector.
        ///     Named variables registered via <see cref="OnRegisterVariables"/> are added to this instance.
        /// </summary>
        [SerializeField] protected LocalizedString LocalizedString = new();

        [field: SerializeField, HideInInspector]
        private TextMeshProUGUI TextReference { get; set; }

        private TContext _lastContext;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        /// <summary>
        ///     Called once on Awake (before the first OnEnable / RegisterChangeHandler).
        ///     Register named persistent variables with <see cref="LocalizedString"/> here:
        ///     <c>LocalizedString.Add("variableName", myVariable);</c>
        /// </summary>
        protected virtual void OnRegisterVariables()
        {
        }

        /// <summary>
        ///     Called whenever the context reference changes.
        ///     Map context fields onto the registered persistent variable instances.
        ///     Their <c>ValueChanged</c> events will automatically trigger a string re-render.
        /// </summary>
        protected abstract void OnApplyVariables([CanBeNull] TContext context);

        protected override void AssignComponents()
        {
            base.AssignComponents();
            OnRegisterVariables();
        }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            LocalizedString.StringChanged += OnStringChanged;
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();
            LocalizedString.StringChanged -= OnStringChanged;
        }

        // ── Context tracking ─────────────────────────────────────────────────────

        /// <summary>
        ///     Detects when the context reference changes and marks the object dirty,
        ///     which causes the render pipeline to call <see cref="OnRender"/> on the next frame.
        /// </summary>
        public override void ValidateContext()
        {
            TContext current = Context;
            if (EqualityComparer<TContext>.Default.Equals(_lastContext, current)) return;
            _lastContext = current;
            SetDirty();
        }

        // ── IRenderable<TContext> ─────────────────────────────────────────────────

        /// <summary>
        ///     Invoked by the render pipeline when the context is dirty.
        ///     Applies context data to registered variables; the LocalizedString
        ///     re-renders automatically via variable value-changed events.
        /// </summary>
        public void OnRender(TContext context)
        {
            OnApplyVariables(context);
        }

        // ── LocalizedString handler ───────────────────────────────────────────────

        private void OnStringChanged(string value) => TextReference.SetText(value);

        // ── Validation ───────────────────────────────────────────────────────────

        protected override void OnValidate()
        {
            base.OnValidate();
            TextReference = GetComponent<TextMeshProUGUI>();
            Assert.IsNotNull(TextReference,
                "UILocalizedTextWithValuesObject requires a TextMeshProUGUI component.");
        }
    }
}
