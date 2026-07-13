using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Components;
using Systems.SimpleDialogue.Data;
using Systems.SimpleDialogue.Implementations;
using Systems.SimpleDialogue.Operations;
using UnityEngine;

namespace Systems.SimpleDialogue.Tests
{
    public sealed class DialogueRuntimeTests : SimpleDialogueTestBase
    {
        [Test]
        public void BeginDialogue_WhenGraphIsMissing_ReturnsGraphIsNull()
        {
            Dialogue dialogue = CreateDialogue(null);

            OperationResult result = dialogue.BeginDialogue();

            AssertSimilar(DialogueOperations.GraphIsNull(), result);
        }

        [Test]
        public void BeginDialogue_WhenEntryExists_RendersFirstNodeAndOptions()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestNpcNode npc = graph.AddNode<TestNpcNode>();
            TestPlayerNode answer = graph.AddNode<TestPlayerNode>();
            npc.Speaker = "Archivist";
            npc.Line = "The gate remembers every name.";
            answer.Line = "Then it knows mine.";
            Connect(entry, nameof(DialogueEntryNode.next), npc);
            Connect(npc, nameof(NPCDialogueNode.answers), answer);
            TestRenderer renderer = new();
            Dialogue dialogue = CreateDialogue(graph, renderer);

            OperationResult result = dialogue.BeginDialogue();

            AssertSimilar(DialogueOperations.Started(), result);
            Assert.IsTrue(dialogue.IsRunning);
            Assert.AreSame(npc, dialogue.CurrentNode);
            Assert.AreEqual("Archivist", renderer.LastContext.SpeakerName);
            Assert.AreEqual("The gate remembers every name.", renderer.LastContext.Text);
            Assert.AreEqual(1, renderer.LastContext.Options.Count);
            Assert.AreEqual("Then it knows mine.", renderer.LastContext.Options[0].text);
        }

        [Test]
        public void SelectOption_WhenAnswerHasNextNode_EntersNextNode()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestNpcNode firstNpc = graph.AddNode<TestNpcNode>();
            TestPlayerNode answer = graph.AddNode<TestPlayerNode>();
            TestNpcNode nextNpc = graph.AddNode<TestNpcNode>();
            firstNpc.Line = "Choose.";
            answer.Line = "I choose forward.";
            nextNpc.Line = "Forward it is.";
            Connect(entry, nameof(DialogueEntryNode.next), firstNpc);
            Connect(firstNpc, nameof(NPCDialogueNode.answers), answer);
            Connect(answer, nameof(PlayerDialogueNode.next), nextNpc);
            Dialogue dialogue = CreateDialogue(graph, new TestRenderer());
            dialogue.BeginDialogue();

            OperationResult result = dialogue.SelectOption(dialogue.Options[0]);

            AssertSimilar(DialogueOperations.NodeEntered(), result);
            Assert.AreSame(nextNpc, dialogue.CurrentNode);
        }

        [Test]
        public void Advance_WhenNpcNodesAreConnectedInSequence_EntersFollowingNpcNode()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestNpcNode firstNpc = graph.AddNode<TestNpcNode>();
            TestNpcNode secondNpc = graph.AddNode<TestNpcNode>();
            firstNpc.Line = "First line.";
            secondNpc.Line = "Second line.";
            Connect(entry, nameof(DialogueEntryNode.next), firstNpc);
            Connect(firstNpc, nameof(NPCDialogueNode.next), secondNpc);
            TestRenderer renderer = new();
            Dialogue dialogue = CreateDialogue(graph, renderer);
            dialogue.BeginDialogue();
            Assert.IsTrue(renderer.LastContext.CanAdvance);

            OperationResult result = dialogue.Advance();

            AssertSimilar(DialogueOperations.NodeEntered(), result);
            Assert.AreSame(secondNpc, dialogue.CurrentNode);
        }

        [Test]
        public void CanAdvance_WhenNextNpcNodeIsUnavailable_ReturnsNodeUnavailable()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestNpcNode firstNpc = graph.AddNode<TestNpcNode>();
            TestNpcNode nextNpc = graph.AddNode<TestNpcNode>();
            nextNpc.Available = false;
            Connect(entry, nameof(DialogueEntryNode.next), firstNpc);
            Connect(firstNpc, nameof(NPCDialogueNode.next), nextNpc);
            TestRenderer renderer = new();
            Dialogue dialogue = CreateDialogue(graph, renderer);
            dialogue.BeginDialogue();
            Assert.IsFalse(renderer.LastContext.CanAdvance);

            OperationResult result = dialogue.CanAdvance();

            AssertSimilar(DialogueOperations.NodeUnavailable(), result);
            Assert.AreSame(firstNpc, dialogue.CurrentNode);
        }

        [Test]
        public void BeginDialogue_WhenConditionalNodeIsEntered_FollowsSelectedOutput()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestConditionalNode conditional = graph.AddNode<TestConditionalNode>();
            TestNpcNode trueNode = graph.AddNode<TestNpcNode>();
            TestNpcNode falseNode = graph.AddNode<TestNpcNode>();
            conditional.Condition = true;
            Connect(entry, nameof(DialogueEntryNode.next), conditional);
            Connect(conditional, nameof(ConditionalDialogueNode.whenTrue), trueNode);
            Connect(conditional, nameof(ConditionalDialogueNode.whenFalse), falseNode);
            Dialogue dialogue = CreateDialogue(graph, new TestRenderer());

            OperationResult result = dialogue.BeginDialogue();

            AssertSimilar(DialogueOperations.Started(), result);
            Assert.AreSame(trueNode, dialogue.CurrentNode);
        }

        [Test]
        public void BeginDialogue_WhenSwitchNodeIsEntered_FollowsEnumOutput()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestSwitchNode switchNode = graph.AddNode<TestSwitchNode>();
            TestNpcNode firstNode = graph.AddNode<TestNpcNode>();
            TestNpcNode secondNode = graph.AddNode<TestNpcNode>();
            switchNode.Route = TestDialogueRoute.Second;
            Connect(entry, nameof(DialogueEntryNode.next), switchNode);
            Connect(switchNode, nameof(TestDialogueRoute.First), firstNode);
            Connect(switchNode, nameof(TestDialogueRoute.Second), secondNode);
            Dialogue dialogue = CreateDialogue(graph, new TestRenderer());

            OperationResult result = dialogue.BeginDialogue();

            AssertSimilar(DialogueOperations.Started(), result);
            Assert.AreSame(secondNode, dialogue.CurrentNode);
        }

        [Test]
        public void BeginDialogue_WhenAnotherDialogueIsRunning_ReturnsAnotherDialogueRunning()
        {
            DialogueGraph firstGraph = CreateGraph();
            DialogueEntryNode firstEntry = firstGraph.AddNode<DialogueEntryNode>();
            TestNpcNode firstNpc = firstGraph.AddNode<TestNpcNode>();
            Connect(firstEntry, nameof(DialogueEntryNode.next), firstNpc);
            Dialogue firstDialogue = CreateDialogue(firstGraph, new TestRenderer());
            firstDialogue.BeginDialogue();

            DialogueGraph secondGraph = CreateGraph();
            DialogueEntryNode secondEntry = secondGraph.AddNode<DialogueEntryNode>();
            TestNpcNode secondNpc = secondGraph.AddNode<TestNpcNode>();
            Connect(secondEntry, nameof(DialogueEntryNode.next), secondNpc);
            Dialogue secondDialogue = CreateDialogue(secondGraph, new TestRenderer());

            OperationResult result = secondDialogue.BeginDialogue();

            AssertSimilar(DialogueOperations.AnotherDialogueRunning(), result);
            Assert.IsFalse(secondDialogue.IsRunning);
        }

        [Test]
        public void SelectOption_WhenAnswerEndsBranch_FinishesDialogue()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestNpcNode npc = graph.AddNode<TestNpcNode>();
            TestPlayerNode answer = graph.AddNode<TestPlayerNode>();
            DialogueExitNode exit = graph.AddNode<DialogueExitNode>();
            Connect(entry, nameof(DialogueEntryNode.next), npc);
            Connect(npc, nameof(NPCDialogueNode.answers), answer);
            Connect(answer, nameof(PlayerDialogueNode.next), exit);
            TestRenderer renderer = new();
            Dialogue dialogue = CreateDialogue(graph, renderer);
            dialogue.BeginDialogue();

            OperationResult result = dialogue.SelectOption(dialogue.Options[0]);

            AssertSimilar(DialogueOperations.Finished(), result);
            Assert.IsFalse(dialogue.IsRunning);
            Assert.IsTrue(renderer.WasCleared);
        }

        [Test]
        public void BeginDialogue_HidesInvisibleOptionsAndDisablesUnavailableOptions()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestNpcNode npc = graph.AddNode<TestNpcNode>();
            TestPlayerNode hidden = graph.AddNode<TestPlayerNode>();
            TestPlayerNode blocked = graph.AddNode<TestPlayerNode>();
            hidden.Visible = false;
            hidden.Line = "Hidden";
            blocked.Available = false;
            blocked.Line = "Blocked";
            Connect(entry, nameof(DialogueEntryNode.next), npc);
            Connect(npc, nameof(NPCDialogueNode.answers), hidden);
            Connect(npc, nameof(NPCDialogueNode.answers), blocked);
            Dialogue dialogue = CreateDialogue(graph, new TestRenderer());

            dialogue.BeginDialogue();

            Assert.AreEqual(1, dialogue.Options.Count);
            Assert.AreEqual("Blocked", dialogue.Options[0].text);
            Assert.IsFalse(dialogue.Options[0].isAvailable);
        }

        [Test]
        public void InterruptDialogue_WhenRunning_ClearsRenderer()
        {
            DialogueGraph graph = CreateGraph();
            DialogueEntryNode entry = graph.AddNode<DialogueEntryNode>();
            TestNpcNode npc = graph.AddNode<TestNpcNode>();
            Connect(entry, nameof(DialogueEntryNode.next), npc);
            TestRenderer renderer = new();
            Dialogue dialogue = CreateDialogue(graph, renderer);
            dialogue.BeginDialogue();

            OperationResult result = dialogue.InterruptDialogue();

            AssertSimilar(DialogueOperations.Interrupted(), result);
            Assert.IsFalse(dialogue.IsRunning);
            Assert.IsTrue(renderer.WasCleared);
        }
    }
}
