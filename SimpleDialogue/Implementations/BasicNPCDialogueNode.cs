using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Data;
using UnityEngine;
using XNode;

namespace Systems.SimpleDialogue.Implementations
{
    /// <summary>
    ///     Built-in NPC dialogue node with inspector-authored speaker and text.
    /// </summary>
    [Node.CreateNodeMenu("Basic NPC Dialogue")]
    public sealed class BasicNPCDialogueNode : NPCDialogueNode
    {
        [SerializeField] private string _speakerName = string.Empty;

        [SerializeField, TextArea(2, 8)] private string _text = string.Empty;

        protected internal override string GetSpeakerName(in DialogueContext context) => _speakerName;

        protected internal override string GetText(in DialogueContext context) => _text;
    }
}
