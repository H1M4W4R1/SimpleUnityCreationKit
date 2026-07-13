using Systems.SimpleCore.Operations;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Data;
using Systems.SimpleDialogue.Operations;

namespace Systems.SimpleDialogue.Tests
{
    public enum TestDialogueRoute
    {
        First,
        Second,
        Unconnected
    }

    public sealed class TestConditionalNode : ConditionalDialogueNode
    {
        public bool Condition;

        protected internal override bool EvaluateCondition(in DialogueContext context) => Condition;
    }

    public sealed class TestSwitchNode : SwitchDialogueNode<TestDialogueRoute>
    {
        public TestDialogueRoute Route;

        protected internal override TestDialogueRoute GetSwitchValue(in DialogueContext context) => Route;
    }

    public sealed class TestNpcNode : NPCDialogueNode
    {
        public string Speaker = string.Empty;
        public string Line = string.Empty;
        public bool Visible = true;
        public bool Available = true;

        protected internal override bool IsVisible(in DialogueContext context) => Visible;

        protected internal override bool IsAvailable(in DialogueContext context) => Available;

        protected internal override string GetSpeakerName(in DialogueContext context) => Speaker;

        protected internal override string GetText(in DialogueContext context) => Line;
    }

    public sealed class TestPlayerNode : PlayerDialogueNode
    {
        public string Line = string.Empty;
        public bool Visible = true;
        public bool Available = true;

        protected internal override bool IsVisible(in DialogueContext context) => Visible;

        protected internal override bool IsAvailable(in DialogueContext context) => Available;

        protected internal override OperationResult CanEnter(in DialogueContext context)
        {
            return Available ? DialogueOperations.Permitted() : DialogueOperations.OptionUnavailable();
        }

        protected internal override string GetSpeakerName(in DialogueContext context) => string.Empty;

        protected internal override string GetText(in DialogueContext context) => Line;
    }

    public sealed class TestRenderer : IDialogueRenderer
    {
        public DialogueViewContext LastContext { get; private set; }
        public bool WasCleared { get; private set; }

        public void RenderDialogue(DialogueViewContext context)
        {
            LastContext = context;
            WasCleared = false;
        }

        public void ClearDialogue()
        {
            WasCleared = true;
        }
    }
}
