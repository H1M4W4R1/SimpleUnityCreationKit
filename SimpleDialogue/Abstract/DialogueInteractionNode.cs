using Systems.SimpleCore.Operations;
using Systems.SimpleDialogue.Data;
using Systems.SimpleDialogue.Operations;
using XNode;

namespace Systems.SimpleDialogue.Abstract
{
    /// <summary>
    ///     Base node used by SimpleDialogue xNode graphs.
    /// </summary>
    public abstract class DialogueInteractionNode : Node
    {
        [Input(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict)]
        public DialogueConnection input;

        protected internal virtual OperationResult CanEnter(in DialogueContext context) =>
            DialogueOperations.Permitted();

        protected internal virtual bool IsVisible(in DialogueContext context) => true;

        protected internal virtual bool IsAvailable(in DialogueContext context) => true;

        protected internal abstract string GetSpeakerName(in DialogueContext context);

        protected internal abstract string GetText(in DialogueContext context);

        protected internal virtual void OnNodeEntered(in DialogueContext context, in OperationResult result)
        {
        }

        protected internal virtual void OnNodeEnterFailed(in DialogueContext context, in OperationResult result)
        {
        }

        protected internal virtual void OnNodeExited(in DialogueContext context, in OperationResult result)
        {
        }

        internal OperationResult CanEnterInternal(in DialogueContext context)
        {
            OperationResult result = CanEnter(in context);
            if (!result) return result;

            if (!IsVisible(in context)) return DialogueOperations.NodeUnavailable();
            if (!IsAvailable(in context)) return DialogueOperations.NodeUnavailable();

            return result;
        }

        public override object GetValue(NodePort port) => new DialogueConnection();
    }
}
