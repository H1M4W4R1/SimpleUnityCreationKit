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
    }
}
