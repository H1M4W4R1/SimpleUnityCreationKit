using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Selectors.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Selectors.Implementations.Dropdown
{
    /// <summary>
    ///     Dropdown selector for UI, used to select single item from a list
    /// </summary>
    /// <typeparam name="TObjectType">Object type in the list</typeparam>
    [RequireComponent(typeof(TMP_Dropdown))]
    public abstract class UIDropdownSelectorBase<TObjectType> : UISelectorBase<TObjectType>
    {
        [field: SerializeField, HideInInspector] private TMP_Dropdown DropdownComponent { get; set; }

        /// <summary>
        ///     Selects a dropdown option by index
        /// </summary>
        /// <param name="index">Index of option</param>
        public bool SelectOption(int index)
        {
            if (!DropdownComponent || Context == null) return false;
            if (!Context.IsValidIndex(index)) return false;

            DropdownComponent.value = index; // this triggers onValueChanged
            return true;
        }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            DropdownComponent.onValueChanged.AddListener(DropdownSelectionChangedHandler);
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();
            DropdownComponent.onValueChanged.RemoveListener(DropdownSelectionChangedHandler);
        }

        /// <summary>
        ///     Handler for dropdown selection change
        /// </summary>
        private void DropdownSelectionChangedHandler(int newIndex)
        {
            // Check if same index
            if (ReferenceEquals(Context, null)) return;

            // Sync with base Context
            TrySelectIndex(newIndex);
        }

        protected override void OnLateSetupComplete()
        {
            base.OnLateSetupComplete();

            if (Context is null) return;

            // Rebuild dropdown options from context
            RefreshDropdownOptions(Context.DataArray);

            // Select default / current index
            SelectOption(Context.SelectedIndex >= 0 ? Context.SelectedIndex : Context.DefaultIndex);
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            if (Context is null) return;

            // Re-sync options if list changed
            RefreshDropdownOptions(Context.DataArray);

            // Update selection if needed — temporarily unsubscribe to prevent
            // re-entrant onValueChanged firing during refresh
            if (DropdownComponent.value != Context.SelectedIndex)
            {
                DropdownComponent.onValueChanged.RemoveListener(DropdownSelectionChangedHandler);
                DropdownComponent.value = Context.SelectedIndex;
                DropdownComponent.onValueChanged.AddListener(DropdownSelectionChangedHandler);
            }
        }

        /// <summary>
        ///     Refresh dropdown options from given data
        /// </summary>
        /// <param name="data">List of data objects</param>
        private void RefreshDropdownOptions([NotNull] IReadOnlyList<TObjectType> data)
        {
            DropdownComponent.options.Clear();
            for (int index = 0; index < data.Count; index++)
            {
                TObjectType obj = data[index];
                DropdownComponent.options.Add(new TMP_Dropdown.OptionData(GetOptionLabel(obj)));
            }

            DropdownComponent.RefreshShownValue();
        }

        /// <summary>
        ///     Converts an object into a dropdown label
        ///     Override to customize how objects are displayed
        /// </summary>
        protected abstract string GetOptionLabel(TObjectType obj);

        protected override void AssignComponents()
        {
            base.AssignComponents();
            DropdownComponent = GetComponent<TMP_Dropdown>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            DropdownComponent = GetComponent<TMP_Dropdown>();
            Assert.IsNotNull(DropdownComponent, "UIDropdownSelectorBase requires a TMP_Dropdown component");
        }
    }
}
