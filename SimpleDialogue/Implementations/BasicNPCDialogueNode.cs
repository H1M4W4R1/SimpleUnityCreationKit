using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Data;
using UnityEngine;
using UnityEngine.Localization;
using XNode;

namespace Systems.SimpleDialogue.Implementations
{
    /// <summary>
    ///     Built-in NPC dialogue node with inspector-authored speaker and text.
    /// </summary>
    [Node.CreateNodeMenu("Basic NPC Dialogue")]
    public sealed class BasicNPCDialogueNode : NPCDialogueNode, IDialogueWithSpeakerName, IDialogueWithText
    {
        [SerializeField] private LocalizedString _localizedSpeakerName = new(DialogueLocalization.TABLE_COLLECTION_NAME, string.Empty);

        [SerializeField] private LocalizedString _localizedText = new(DialogueLocalization.TABLE_COLLECTION_NAME, string.Empty);

        public LocalizedString GetSpeakerName(in DialogueContext context) => _localizedSpeakerName;

        public LocalizedString GetText(in DialogueContext context) => _localizedText;

    }
}
