using Systems.SimpleDialogue.Components;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Systems.SimpleDialogue.Editor
{
    /// <summary>
    ///     Inspector helper for opening the assigned xNode dialogue graph.
    /// </summary>
    [CustomEditor(typeof(Dialogue))]
    public sealed class DialogueEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Dialogue dialogue = (Dialogue) target;
            if (ReferenceEquals(dialogue.Graph, null)) return;

            if (!GUILayout.Button("Open Dialogue Graph")) return;
            NodeEditorWindow.Open(dialogue.Graph);
        }
    }
}
