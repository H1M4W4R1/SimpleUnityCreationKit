using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Components;
using Systems.SimpleDialogue.Implementations;
using UnityEngine.Localization;

namespace Systems.SimpleDialogue.Data
{
    /// <summary>
    ///     Reusable render state exposed to dialogue renderers.
    /// </summary>
    public sealed class DialogueViewContext
    {
        [NotNull] private readonly IReadOnlyList<DialogueOption> _options;

        [CanBeNull] public Dialogue Dialogue { get; private set; }
        [CanBeNull] public DialogueGraph Graph { get; private set; }
        [CanBeNull] public DialogueInteractionNode CurrentNode { get; private set; }
        [CanBeNull] public LocalizedString SpeakerName { get; private set; }
        [CanBeNull] public LocalizedString Text { get; private set; }
        public bool IsRunning { get; private set; }
        /// <summary>
        ///     Whether the renderer can offer an interaction that advances to the current NPC node's next node.
        /// </summary>
        public bool CanAdvance { get; private set; }
        [NotNull] public DialogueOptionListContext OptionsContext { get; }
        [NotNull] public IReadOnlyList<DialogueOption> Options => _options;

        public DialogueViewContext([NotNull] IReadOnlyList<DialogueOption> options)
        {
            _options = options;
            OptionsContext = new DialogueOptionListContext(options);
        }

        internal void Set(
            [CanBeNull] Dialogue dialogue,
            [CanBeNull] DialogueGraph graph,
            [CanBeNull] DialogueInteractionNode currentNode,
            [CanBeNull] LocalizedString speakerName,
            [CanBeNull] LocalizedString text,
            bool isRunning,
            bool canAdvance)
        {
            Dialogue = dialogue;
            Graph = graph;
            CurrentNode = currentNode;
            SpeakerName = speakerName;
            Text = text;
            IsRunning = isRunning;
            CanAdvance = canAdvance;
        }
    }
}
