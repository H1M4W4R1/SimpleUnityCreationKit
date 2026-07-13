using Systems.SimpleUI.Components.Selectors.Abstract;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Selectors.Implementations.Tabbing
{
    /// <summary>
    ///     Toggle-group selector for UI, used to select single item from a list
    /// </summary>
    /// <typeparam name="TObjectType">Object type in the list</typeparam>
    [RequireComponent(typeof(UISelectorToggleGroup))]
    public abstract class UIToggleGroupSelectorBase<TObjectType> : UISelectorBase<TObjectType>
    {
        [field: SerializeField, HideInInspector] protected UISelectorToggleGroup ToggleGroup { get; private set; }

        /// <summary>
        ///     Selects a toggle
        /// </summary>
        /// <param name="toggleIndex">Index of the toggle to select</param>
        public bool SelectToggle(int toggleIndex) => ToggleGroup.SelectToggle(toggleIndex);

        protected override void AttachEvents()
        {
            base.AttachEvents();
            ToggleGroup.OnSelectionChanged += ToggleGroupSelectionChangedHandler;
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();
            ToggleGroup.OnSelectionChanged -= ToggleGroupSelectionChangedHandler;
        }

        /// <summary>
        ///     Handler for toggle group selection change
        /// </summary>
        private void ToggleGroupSelectionChangedHandler(int newIndex)
        {
            // Notify base implementation
            TrySelectIndex(newIndex);
        }

        protected override void OnSelectedIndexChanged(int from, int to)
        {
            base.OnSelectedIndexChanged(from, to);

            // Select toggle if it is not the first one
            if (to != ToggleGroup.FirstToggleIndex) SelectToggle(to);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ToggleGroup = GetComponent<UISelectorToggleGroup>();
            Assert.IsNotNull(ToggleGroup, "UIToggleGroupSelectorBase requires a UISelectorToggleGroup component");
        }
    }
}