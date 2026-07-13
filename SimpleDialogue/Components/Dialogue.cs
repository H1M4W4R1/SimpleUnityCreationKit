using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Data;
using Systems.SimpleDialogue.Implementations;
using Systems.SimpleDialogue.Operations;
using UnityEngine;

namespace Systems.SimpleDialogue.Components
{
    /// <summary>
    ///     Runs a dialogue graph owned by this GameObject.
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Dialogue : MonoBehaviour
    {
        public const string DEFAULT_ENTRY_ID = "default";

        [field: SerializeField] public DialogueGraph Graph { get; private set; }

        [field: SerializeField] public string DefaultEntryId { get; private set; } = DEFAULT_ENTRY_ID;

        [CanBeNull] private IDialogueRenderer _renderer;
        [CanBeNull] private DialogueInteractionNode _currentNode;
        [CanBeNull] private DialogueGraph _currentGraph;
        [CanBeNull] private static Dialogue _activeDialogue;
        [NotNull] private readonly List<DialogueOption> _options = new();
        [NotNull] private DialogueViewContext _viewContext = null!; // Not null, as assigned in Awake

        [CanBeNull] public DialogueInteractionNode CurrentNode => _currentNode;

        [CanBeNull] public DialogueGraph CurrentGraph => _currentGraph;

        public DialogueViewContext ViewContext => _viewContext;

        public IReadOnlyList<DialogueOption> Options => _options;

        public bool IsRunning { get; private set; }

        public OperationResult BeginDialogue() => BeginDialogue(DefaultEntryId, ActionSource.External);

        public OperationResult BeginDialogue(
            string entryId,
            ActionSource actionSource = ActionSource.External)
        {
            if (!Graph) return FailStart(DialogueOperations.GraphIsNull(), actionSource);

            DialogueInteractionNode startNode = Graph.GetStartNode(entryId);
            if (ReferenceEquals(startNode, null))
                return FailStart(DialogueOperations.EntryNotFound(), actionSource);
            if (IsAnotherDialogueRunning())
                return FailStart(DialogueOperations.AnotherDialogueRunning(), actionSource);

            _currentGraph = Graph;
            IsRunning = true;
            _activeDialogue = this;

            OperationResult enterResult = EnterNode(startNode, actionSource);
            if (!enterResult)
            {
                IsRunning = false;
                _currentGraph = null;
                ClearActiveDialogue();
                return enterResult;
            }

            OperationResult result = DialogueOperations.Started();
            if (actionSource == ActionSource.External) OnDialogueStarted(in result);
            return result;
        }

        public OperationResult RecoverDialogue(ActionSource actionSource = ActionSource.External)
        {
            if (!IsRunning) return BeginDialogue(DefaultEntryId, actionSource);
            if (ReferenceEquals(_currentNode, null)) return BeginDialogue(DefaultEntryId, actionSource);

            RefreshView(actionSource);
            return DialogueOperations.NodeEntered();
        }

        public OperationResult InterruptDialogue(ActionSource actionSource = ActionSource.External)
        {
            if (!IsRunning) return DialogueOperations.DialogueNotRunning();

            IsRunning = false;
            ClearActiveDialogue();
            OperationResult result = DialogueOperations.Interrupted();
            ClearRuntimeState();

            if (actionSource == ActionSource.External) OnDialogueInterrupted(in result);
            RenderCurrentState();
            return result;
        }

        public OperationResult SelectOption(
            in DialogueOption option,
            ActionSource actionSource = ActionSource.External)
        {
            if (!IsRunning) return DialogueOperations.DialogueNotRunning();
            if (!option.IsValid) return DialogueOperations.OptionNotFound();
            if (option.index < 0 || option.index >= _options.Count) return DialogueOperations.OptionNotFound();
            if (!ReferenceEquals(_options[option.index].node, option.node))
                return DialogueOperations.OptionNotFound();
            if (!option.isAvailable) return DialogueOperations.OptionUnavailable();

            DialogueContext context = CreateContext(option.node, in option, actionSource);
            OperationResult canEnterResult = option.node.CanEnterInternal(in context);
            if (!canEnterResult)
            {
                option.node.OnNodeEnterFailed(in context, in canEnterResult);
                return canEnterResult;
            }

            OperationResult selectedResult = DialogueOperations.OptionSelected();
            if (!ReferenceEquals(_currentNode, null)) _currentNode.OnNodeExited(in context, in selectedResult);

            DialogueInteractionNode nextNode = option.node.GetNextNode();
            if (ReferenceEquals(nextNode, null)) return FinishDialogue(actionSource);

            return EnterNode(nextNode, actionSource);
        }

        /// <summary>
        ///     Verifies that the current NPC-only sequence can advance into its next node.
        /// </summary>
        public OperationResult CanAdvance(ActionSource actionSource = ActionSource.External)
        {
            if (!IsRunning) return DialogueOperations.DialogueNotRunning();
            if (_options.Count > 0) return DialogueOperations.OptionUnavailable();
            if (_currentNode is not NPCDialogueNode npcNode) return DialogueOperations.OptionNotFound();

            DialogueInteractionNode nextNode = npcNode.GetNextNode();
            if (ReferenceEquals(nextNode, null)) return DialogueOperations.NodeIsNull();

            DialogueOption emptyOption = default;
            DialogueContext context = CreateContext(nextNode, in emptyOption, actionSource);
            return nextNode.CanEnterInternal(in context);
        }

        public OperationResult Advance(ActionSource actionSource = ActionSource.External)
        {
            OperationResult canAdvanceResult = CanAdvance(actionSource);
            if (!canAdvanceResult)
            {
                if (canAdvanceResult.resultCode == DialogueOperations.ERROR_NODE_IS_NULL)
                    return FinishDialogue(actionSource);

                return canAdvanceResult;
            }

            NPCDialogueNode npcNode = (NPCDialogueNode) _currentNode;
            DialogueInteractionNode nextNode = npcNode.GetNextNode();
            DialogueOption emptyOption = default;
            DialogueContext context = CreateContext(nextNode, in emptyOption, actionSource);
            OperationResult advancedResult = DialogueOperations.NodeEntered();
            npcNode.OnNodeExited(in context, in advancedResult);

            return EnterNode(nextNode, actionSource);
        }

        private OperationResult EnterNode(
            [CanBeNull] DialogueInteractionNode node,
            ActionSource actionSource)
        {
            if (ReferenceEquals(node, null)) return DialogueOperations.NodeIsNull();

            DialogueOption emptyOption = default;
            DialogueContext context = CreateContext(node, in emptyOption, actionSource);
            OperationResult result = node.CanEnterInternal(in context);
            if (!result)
            {
                node.OnNodeEnterFailed(in context, in result);
                return result;
            }

            _currentNode = node;
            _currentGraph = node.graph as DialogueGraph;

            if (node is DialogueExitNode) return FinishDialogue(actionSource);
            if (node is ConditionalDialogueNode conditionalNode)
                return EnterNode(conditionalNode.GetNextNode(in context), actionSource);
            if (node is SwitchDialogueNode switchNode)
                return EnterNode(switchNode.GetNextNode(in context), actionSource);
            if (node is SubDialogueNode subDialogueNode && subDialogueNode.Graph)
            {
                DialogueInteractionNode subStartNode = subDialogueNode.Graph.GetStartNode(subDialogueNode.EntryId);
                if (ReferenceEquals(subStartNode, null)) return DialogueOperations.EntryNotFound();
                return EnterNode(subStartNode, actionSource);
            }

            RefreshView(actionSource);
            OperationResult enteredResult = DialogueOperations.NodeEntered();
            node.OnNodeEntered(in context, in enteredResult);
            return enteredResult;
        }

        private OperationResult FinishDialogue(ActionSource actionSource)
        {
            IsRunning = false;
            ClearActiveDialogue();
            OperationResult result = DialogueOperations.Finished();
            ClearRuntimeState();

            if (actionSource == ActionSource.External) OnDialogueFinished(in result);
            RenderCurrentState();
            return result;
        }

        private OperationResult FailStart(in OperationResult result, ActionSource actionSource)
        {
            if (actionSource == ActionSource.External) OnDialogueStartFailed(in result);
            return result;
        }

        private void RefreshView(ActionSource actionSource)
        {
            RebuildOptions(actionSource);

            string speakerName = string.Empty;
            string text = string.Empty;
            if (!ReferenceEquals(_currentNode, null))
            {
                DialogueOption emptyOption = default;
                DialogueContext context = CreateContext(_currentNode, in emptyOption, actionSource);
                speakerName = _currentNode.GetSpeakerName(in context);
                text = _currentNode.GetText(in context);
            }

            OperationResult canAdvanceResult = CanAdvance(actionSource);
            _viewContext.Set(this, _currentGraph, _currentNode, speakerName, text, IsRunning, canAdvanceResult);
            RenderCurrentState();
        }

        private void RebuildOptions(ActionSource actionSource)
        {
            _options.Clear();
            if (_currentNode is not NPCDialogueNode npcNode) return;

            int answerCount = npcNode.AnswerCount;
            for (int optionIndex = 0; optionIndex < answerCount; optionIndex++)
            {
                PlayerDialogueNode answerNode = npcNode.GetAnswerNode(optionIndex);
                if (ReferenceEquals(answerNode, null)) continue;

                DialogueOption emptyOption = default;
                DialogueContext context = CreateContext(answerNode, in emptyOption, actionSource);
                if (!answerNode.IsVisible(in context)) continue;

                bool isAvailable = answerNode.IsAvailable(in context) && answerNode.CanEnterInternal(in context);
                string text = answerNode.GetText(in context);
                _options.Add(new DialogueOption(this, answerNode, _options.Count, text, isAvailable));
            }
        }

        private DialogueContext CreateContext(
            [CanBeNull] DialogueInteractionNode targetNode,
            in DialogueOption selectedOption,
            ActionSource actionSource)
        {
            return new DialogueContext(
                this,
                _currentGraph,
                _currentNode,
                targetNode,
                in selectedOption,
                actionSource);
        }

        private void RenderCurrentState()
        {
            if (ReferenceEquals(_renderer, null)) return;

            if (IsRunning)
            {
                _renderer.RenderDialogue(_viewContext);
                return;
            }

            _renderer.ClearDialogue();
        }

        private void ClearRuntimeState()
        {
            _options.Clear();
            _currentNode = null;
            _currentGraph = null;
            _viewContext.Set(this, null, null, string.Empty, string.Empty, false, false);
        }

        private bool IsAnotherDialogueRunning()
        {
            if (ReferenceEquals(_activeDialogue, null)) return false;
            if (!_activeDialogue || !_activeDialogue.IsRunning)
            {
                _activeDialogue = null;
                return false;
            }

            return !ReferenceEquals(_activeDialogue, this);
        }

        private void ClearActiveDialogue()
        {
            if (ReferenceEquals(_activeDialogue, this)) _activeDialogue = null;
        }

        protected virtual void OnDialogueStarted(in OperationResult result)
        {
        }

        protected virtual void OnDialogueStartFailed(in OperationResult result)
        {
        }

        protected virtual void OnDialogueFinished(in OperationResult result)
        {
        }

        protected virtual void OnDialogueInterrupted(in OperationResult result)
        {
        }

        private void Awake()
        {
            _viewContext = new DialogueViewContext(_options);
            _renderer = FindRendererInScene();
        }

        private void OnDestroy()
        {
            ClearActiveDialogue();
        }

        [CanBeNull] private IDialogueRenderer FindRendererInScene()
        {
            Object[] candidates = FindObjectsByType(typeof(MonoBehaviour));
            for (int nCandidate = 0; nCandidate < candidates.Length; nCandidate++)
            {
                if (candidates[nCandidate] is IDialogueRenderer renderer)  return renderer;
            }

            return null;
        }

        internal void InitializeForTests(DialogueGraph graph, [CanBeNull] IDialogueRenderer renderer = null)
        {
            Graph = graph;
            _viewContext = new DialogueViewContext(_options);
            _renderer = renderer;
        }
    }
}
