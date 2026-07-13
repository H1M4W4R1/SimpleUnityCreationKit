using JetBrains.Annotations;
using Systems.SimpleDialogue.Abstract;
using UnityEngine;
using XNode;

namespace Systems.SimpleDialogue.Implementations
{
    /// <summary>
    ///     xNode graph asset containing SimpleDialogue nodes.
    /// </summary>
    [CreateAssetMenu(menuName = "Simple Dialogue/Dialogue Graph", fileName = "Dialogue Graph")]
    public sealed class DialogueGraph : NodeGraph
    {
        [CanBeNull] public DialogueEntryNode GetEntryNode(string entryId)
        {
            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
            {
                if (nodes[nodeIndex] is not DialogueEntryNode entryNode) continue;
                if (entryNode.EntryId == entryId) return entryNode;
            }

            return null;
        }

        [CanBeNull] public DialogueInteractionNode GetStartNode(string entryId)
        {
            DialogueEntryNode entryNode = GetEntryNode(entryId);
            return ReferenceEquals(entryNode, null) ? null : entryNode.GetNextNode();
        }
    }
}
