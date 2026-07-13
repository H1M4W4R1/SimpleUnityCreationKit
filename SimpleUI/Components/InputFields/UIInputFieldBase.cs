using Systems.SimpleUI.Components.Abstract.Interactable;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.InputFields
{
    [RequireComponent(typeof(TMP_InputField))] public abstract class UIInputFieldBase : UIInteractableObjectBase
    {
        [field: SerializeField, HideInInspector] protected TMP_InputField InputFieldReference { get; private set; }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            InputFieldReference.onSelect.AddListener(OnFieldSelected);
            InputFieldReference.onValueChanged.AddListener(OnFieldValueChanged);
            InputFieldReference.onEndEdit.AddListener(OnFieldEndEdited);
            InputFieldReference.onSubmit.AddListener(OnFieldSubmitted);
            InputFieldReference.onDeselect.AddListener(OnFieldDeselected);

            InputFieldReference.onTextSelection.AddListener(OnTextSelected);
            InputFieldReference.onEndTextSelection.AddListener(OnTextDeselected);
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();
            InputFieldReference.onSelect.RemoveListener(OnFieldSelected);
            InputFieldReference.onValueChanged.RemoveListener(OnFieldValueChanged);
            InputFieldReference.onEndEdit.RemoveListener(OnFieldEndEdited);
            InputFieldReference.onSubmit.RemoveListener(OnFieldSubmitted);
            InputFieldReference.onDeselect.RemoveListener(OnFieldDeselected);

            InputFieldReference.onTextSelection.RemoveListener(OnTextSelected);
            InputFieldReference.onEndTextSelection.RemoveListener(OnTextDeselected);
        }

        /// <summary>
        ///     Changes the interactable state of the input field
        /// </summary>
        public sealed override bool IsInteractable => InputFieldReference.interactable;

        /// <summary>
        ///     Makes the input field interactable or not
        /// </summary>
        public override void SetInteractable(bool interactable) => InputFieldReference.interactable = interactable;

        protected override void AssignComponents()
        {
            base.AssignComponents();
            InputFieldReference = GetComponent<TMP_InputField>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            InputFieldReference = GetComponent<TMP_InputField>();
            Assert.IsNotNull(InputFieldReference, "UIInputFieldBase requires a TMP_InputField component");
        }

#region Events

        protected virtual void OnTextSelected(string text, int from, int to)
        {
        }

        protected virtual void OnTextDeselected(string text, int from, int to)
        {
        }

        protected virtual void OnFieldDeselected(string text)
        {
        }

        protected virtual void OnFieldSubmitted(string withText)
        {
        }

        protected virtual void OnFieldEndEdited(string currentText)
        {
        }

        protected virtual void OnFieldValueChanged(string newText)
        {
        }

        protected virtual void OnFieldSelected(string text)
        {
        }

#endregion
    }
}
