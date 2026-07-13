using Systems.SimpleCore.Operations;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Components;
using UnityEngine.Localization;

namespace Systems.SimpleDialogue.Data
{
    /// <summary>
    ///     Renderable player answer option for the current dialogue line.
    /// </summary>
    public readonly struct DialogueOption
    {
        public readonly Dialogue dialogue;
        public readonly PlayerDialogueNode node;
        public readonly int index;
        public readonly LocalizedString text;
        public readonly bool isAvailable;

        public bool IsValid => !ReferenceEquals(dialogue, null) && !ReferenceEquals(node, null);

        public DialogueOption(
            Dialogue dialogue,
            PlayerDialogueNode node,
            int index,
            LocalizedString text,
            bool isAvailable)
        {
            this.dialogue = dialogue;
            this.node = node;
            this.index = index;
            this.text = text;
            this.isAvailable = isAvailable;
        }

        public OperationResult Select()
        {
            if (ReferenceEquals(dialogue, null)) return Systems.SimpleDialogue.Operations.DialogueOperations.DialogueNotRunning();
            return dialogue.SelectOption(this);
        }
    }
}
