using JetBrains.Annotations;
using XNode;

namespace Systems.SimpleDialogue.Abstract
{
    /// <summary>
    ///     Dialogue node representing a player answer that advances to another dialogue node.
    /// </summary>
    [NodeTint("#5F8F52")]
    public abstract class PlayerDialogueNode : DialogueInteractionNode
    {
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
    }
}
