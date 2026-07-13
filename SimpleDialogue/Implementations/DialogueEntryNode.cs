using JetBrains.Annotations;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Data;
using UnityEngine;
using XNode;

namespace Systems.SimpleDialogue.Implementations
{
    /// <summary>
    ///     Entry point for a dialogue graph.
    /// </summary>
    [Node.CreateNodeMenu("Entry")]
    [NodeTint("#7C6AA6")]
    public sealed class DialogueEntryNode : DialogueInteractionNode
    {
        [field: SerializeField] public string EntryId { get; private set; } = "default";

        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public DialogueConnection next;

        [CanBeNull] public DialogueInteractionNode GetNextNode()
        {
            NodePort port = GetOutputPort(nameof(next));
            if (ReferenceEquals(port, null)) return null;

            NodePort connectedPort = port.Connection;
            if (ReferenceEquals(connectedPort, null)) return null;

            return connectedPort.node as DialogueInteractionNode;
        }

        protected internal override string GetSpeakerName(in DialogueContext context) => string.Empty;

        protected internal override string GetText(in DialogueContext context) => string.Empty;
    }
}
