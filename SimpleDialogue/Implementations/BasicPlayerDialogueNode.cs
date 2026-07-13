using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Data;
using UnityEngine;
using UnityEngine.Localization;
using XNode;

namespace Systems.SimpleDialogue.Implementations
{
    /// <summary>
    ///     Built-in player answer node with inspector-authored answer text.
    /// </summary>
    [Node.CreateNodeMenu("Basic Player Dialogue")]
    public sealed class BasicPlayerDialogueNode : PlayerDialogueNode, IDialogueWithText
    {
        [SerializeField] private LocalizedString _localizedText = new(DialogueLocalization.TABLE_COLLECTION_NAME, string.Empty);

        public LocalizedString GetText(in DialogueContext context) => _localizedText;

    }
}
