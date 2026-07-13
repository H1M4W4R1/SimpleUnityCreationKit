using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Components;
using Systems.SimpleDialogue.Implementations;

namespace Systems.SimpleDialogue.Data
{
    /// <summary>
    ///     Runtime context passed through dialogue validation and callback methods.
    /// </summary>
    public readonly ref struct DialogueContext
    {
        public readonly Dialogue dialogue;
        public readonly DialogueGraph graph;
        public readonly DialogueInteractionNode currentNode;
        public readonly DialogueInteractionNode targetNode;
        public readonly DialogueOption selectedOption;

        public DialogueContext(
            Dialogue dialogue,
            DialogueGraph graph,
            DialogueInteractionNode currentNode,
            DialogueInteractionNode targetNode,
            in DialogueOption selectedOption)
        {
            this.dialogue = dialogue;
            this.graph = graph;
            this.currentNode = currentNode;
            this.targetNode = targetNode;
            this.selectedOption = selectedOption;
        }
    }
}
