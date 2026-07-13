using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Data;
using XNode;

namespace Systems.SimpleDialogue.Implementations
{
    /// <summary>
    ///     Terminal node for a dialogue branch.
    /// </summary>
    [Node.CreateNodeMenu("Exit")]
    [NodeTint("#94605B")]
    public sealed class DialogueExitNode : DialogueInteractionNode
    {
        protected internal override string GetSpeakerName(in DialogueContext context) => string.Empty;

        protected internal override string GetText(in DialogueContext context) => string.Empty;
    }
}
