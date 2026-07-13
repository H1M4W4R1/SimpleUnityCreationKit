using System;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Implementations;
using XNodeEditor;

namespace Systems.SimpleDialogue.Editor
{
    /// <summary>
    ///     Restricts xNode's creation menu to concrete nodes supported by <see cref="DialogueGraph"/>.
    /// </summary>
    [CustomNodeGraphEditor(typeof(DialogueGraph), "SimpleDialogue.GraphEditor")]
    public sealed class DialogueGraphEditor : NodeGraphEditor
    {
        public override string GetNodeMenuName(Type type)
        {
            if (type.IsAbstract || !typeof(DialogueInteractionNode).IsAssignableFrom(type)) return null;
            return base.GetNodeMenuName(type);
        }
    }
}
