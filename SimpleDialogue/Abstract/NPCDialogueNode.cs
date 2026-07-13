using JetBrains.Annotations;
using XNode;

namespace Systems.SimpleDialogue.Abstract
{
    /// <summary>
    ///     Dialogue node used for an NPC line and its player answer connections.
    /// </summary>
    [NodeTint("#3E6C92")]
    public abstract class NPCDialogueNode : DialogueInteractionNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict)]
        public DialogueConnection answers;

        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public DialogueConnection next;

        public int AnswerCount
        {
            get
            {
                NodePort port = GetOutputPort(nameof(answers));
                return ReferenceEquals(port, null) ? 0 : port.ConnectionCount;
            }
        }

        [CanBeNull] public PlayerDialogueNode GetAnswerNode(int index)
        {
            NodePort port = GetOutputPort(nameof(answers));
            if (ReferenceEquals(port, null)) return null;
            if (index < 0 || index >= port.ConnectionCount) return null;

            NodePort connectedPort = port.GetConnection(index);
            if (ReferenceEquals(connectedPort, null)) return null;

            return connectedPort.node as PlayerDialogueNode;
        }

        /// <summary>
        ///     Gets the next node for an NPC-only sequence.
        /// </summary>
        [CanBeNull] public DialogueInteractionNode GetNextNode()
        {
            NodePort port = GetOutputPort(nameof(next));
            if (ReferenceEquals(port, null)) return null;

            NodePort connectedPort = port.Connection;
            if (ReferenceEquals(connectedPort, null)) return null;

            return connectedPort.node as DialogueInteractionNode;
        }
    }
}
