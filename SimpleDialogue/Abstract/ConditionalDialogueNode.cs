using JetBrains.Annotations;
using Systems.SimpleDialogue.Data;
using XNode;

namespace Systems.SimpleDialogue.Abstract
{
    /// <summary>
    ///     Non-rendered flow node that enters either output according to an application-defined condition.
    /// </summary>
    [NodeTint("#A47F43")]
    public abstract class ConditionalDialogueNode : DialogueInteractionNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public DialogueConnection whenTrue;

        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public DialogueConnection whenFalse;

        /// <summary>
        ///     Evaluates the condition that selects the next flow output.
        /// </summary>
        protected internal abstract bool EvaluateCondition(in DialogueContext context);

        [CanBeNull] internal DialogueInteractionNode GetNextNode(in DialogueContext context)
        {
            string portName = EvaluateCondition(in context) ? nameof(whenTrue) : nameof(whenFalse);
            NodePort port = GetOutputPort(portName);
            if (ReferenceEquals(port, null)) return null;

            NodePort connectedPort = port.Connection;
            if (ReferenceEquals(connectedPort, null)) return null;

            return connectedPort.node as DialogueInteractionNode;
        }

        protected internal sealed override string GetSpeakerName(in DialogueContext context) => string.Empty;

        protected internal sealed override string GetText(in DialogueContext context) => string.Empty;
    }
}
