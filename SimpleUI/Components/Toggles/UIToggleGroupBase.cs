using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract.Interactable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Toggles
{
    [RequireComponent(typeof(ToggleGroup))] [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIToggleGroupBase : UIInteractableObjectBase
    {
        [field: SerializeField, HideInInspector] protected ToggleGroup ToggleGroupReference { get; private set; }

        public sealed override bool IsInteractable => CanvasGroupReference!.interactable;

        /// <summary>
        ///     List of all toggles in this toggle group
        /// </summary>
        public List<UIToggleBase> Toggles { get; } = new();

        /// <summary>
        ///     Gets the index of the first toggle that is toggled
        ///     Returns -1 if no toggle is toggled
        /// </summary>
        public int FirstToggleIndex
        {
            get
            {
                for (int i = 0; i < Toggles.Count; i++)
                    if (Toggles[i].IsToggled)
                        return i;

                return -1;
            }
        }

        /// <summary>
        ///     Check if at least one toggle must be active
        /// </summary>
        public bool RequireAtLeastOneActive
        {
            get => !ToggleGroupReference.allowSwitchOff;
            protected set => ToggleGroupReference.allowSwitchOff = !value;
        }

        protected override void OnSetupComplete()
        {
            // We need to do this here to ensure all toggles have been created by first render
            // methodology (e.g. via UIList)
            base.OnLateSetupComplete();
            RefreshToggleArray();
        }

        /// <summary>
        ///     Sets the interactable state of the toggle group
        /// </summary>
        public override void SetInteractable(bool interactable) =>
            CanvasGroupReference!.interactable = interactable;

        protected override void AssignComponents()
        {
            base.AssignComponents();
            ToggleGroupReference = GetComponent<ToggleGroup>();
        }

        /// <summary>
        ///     Selects a toggle
        /// </summary>
        public bool SelectToggle(int toggleIndex)
        {
            //  Ensure toggle index is valid
            if (toggleIndex < 0 || toggleIndex >= Toggles.Count) return false;

            // Update all toggles except the one we want to select
            for (int i = 0; i < Toggles.Count; i++)
            {
                if (i == toggleIndex) continue;
                if (Toggles[i].IsToggled) Toggles[i].IsToggled = false;
            }

            // Select the toggle
            Toggles[toggleIndex].IsToggled = true;
            return true;
        }

        /// <summary>
        ///     Method to update the toggle array and register all toggles in this toggle group
        /// </summary>
        internal void RefreshToggleArray()
        {
            // Clear toggle array
            Toggles.Clear();

            // Add all toggles in this toggle group to the toggle array
            Toggles.AddRange(GetComponentsInChildren<UIToggleBase>());

            // Register toggle to toggle group
            for (int i = 0; i < Toggles.Count; i++)
            {
                Toggle toggle = Toggles[i].ToggleReference;
                if (toggle.group != ToggleGroupReference) toggle.group = ToggleGroupReference;
            }
        }

        /// <summary>
        ///     Checks if a toggle is toggled
        /// </summary>
        /// <param name="index">Index of the toggle</param>
        /// <returns>True if the toggle is toggled, false otherwise (or when the index is out of range)</returns>
        public bool IsToggled(int index)
        {
            if (index < 0 || index >= Toggles.Count) return false;
            return Toggles[index].IsToggled;
        }

        /// <summary>
        ///     Changes the state of a toggle
        /// </summary>
        /// <param name="index">Index of the toggle</param>
        /// <param name="value">Value to set </param>
        public void SetToggled(int index, bool value)
        {
            Assert.IsFalse(index < 0 || index >= Toggles.Count, "Toggle index out of range");
            if (index < 0 || index >= Toggles.Count) return;
            Toggles[index].IsToggled = value;
        }

        /// <summary>
        ///     Event that is called when a toggle value changes
        /// </summary>
        /// <param name="toggleIndex">Index of the toggle that changed</param>
        /// <param name="newValue">New value of the toggle</param>
        protected abstract void OnToggleValueChanged(int toggleIndex, bool newValue);

        /// <summary>
        ///     Handles the toggle changed event
        /// </summary>
        internal void OnToggleChanged([NotNull] UIToggleBase uiToggleBase, bool newValue)
        {
            Assert.IsTrue(Toggles.Contains(uiToggleBase), "Toggle is not registered in this toggle group");
            OnToggleValueChanged(Toggles.IndexOf(uiToggleBase), newValue);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ToggleGroupReference = GetComponent<ToggleGroup>();
            Assert.IsNotNull(ToggleGroupReference, "UIToggleGroupBase requires a ToggleGroup component");
        }
    }
}
