using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
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
            [NotNull] Dialogue dialogue)
        {
            return dialogue.BeginDialogue(dialogue.DefaultEntryId);
        }

        public static OperationResult Begin(
            [NotNull] Dialogue dialogue,
            string entryId)
        {
            return dialogue.BeginDialogue(entryId);
        }

        public static OperationResult Select(
            in DialogueOption option)
        {
            return option.Select();
        }

        public static OperationResult Interrupt(
            [NotNull] Dialogue dialogue)
        {
            return dialogue.InterruptDialogue();
        }

        public static OperationResult CanAdvance(
            [NotNull] Dialogue dialogue)
        {
            return dialogue.CanAdvance();
        }

        public static OperationResult Advance(
            [NotNull] Dialogue dialogue)
        {
            return dialogue.Advance();
        }
    }
}
