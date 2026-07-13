using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleDialogue.Components;
using Systems.SimpleDialogue.Data;

namespace Systems.SimpleDialogue.Utility
{
    /// <summary>
    ///     Static facade for common SimpleDialogue operations.
    /// </summary>
    public static class DialogueAPI
    {
        public static OperationResult Begin(
            [NotNull] Dialogue dialogue,
            ActionSource actionSource = ActionSource.External)
        {
            return dialogue.BeginDialogue(dialogue.DefaultEntryId, actionSource);
        }

        public static OperationResult Begin(
            [NotNull] Dialogue dialogue,
            string entryId,
            ActionSource actionSource = ActionSource.External)
        {
            return dialogue.BeginDialogue(entryId, actionSource);
        }

        public static OperationResult Select(
            in DialogueOption option,
            ActionSource actionSource = ActionSource.External)
        {
            return option.Select(actionSource);
        }

        public static OperationResult Interrupt(
            [NotNull] Dialogue dialogue,
            ActionSource actionSource = ActionSource.External)
        {
            return dialogue.InterruptDialogue(actionSource);
        }

        public static OperationResult CanAdvance(
            [NotNull] Dialogue dialogue,
            ActionSource actionSource = ActionSource.External)
        {
            return dialogue.CanAdvance(actionSource);
        }

        public static OperationResult Advance(
            [NotNull] Dialogue dialogue,
            ActionSource actionSource = ActionSource.External)
        {
            return dialogue.Advance(actionSource);
        }
    }
}
