using Systems.SimpleCore.Operations;

namespace Systems.SimpleDialogue.Operations
{
    /// <summary>
    ///     Factory methods for <see cref="OperationResult"/> values produced by SimpleDialogue.
    /// </summary>
    public static class DialogueOperations
    {
        public const ushort SYSTEM_DIALOGUE = OperationResult.USER_SPACE_START + 18;

        public const ushort SUCCESS_STARTED = OperationResult.USER_SPACE_START + 1;
        public const ushort SUCCESS_FINISHED = OperationResult.USER_SPACE_START + 2;
        public const ushort SUCCESS_INTERRUPTED = OperationResult.USER_SPACE_START + 3;
        public const ushort SUCCESS_NODE_ENTERED = OperationResult.USER_SPACE_START + 4;
        public const ushort SUCCESS_OPTION_SELECTED = OperationResult.USER_SPACE_START + 5;

        public const ushort ERROR_GRAPH_IS_NULL = OperationResult.USER_SPACE_START + 1;
        public const ushort ERROR_ENTRY_NOT_FOUND = OperationResult.USER_SPACE_START + 2;
        public const ushort ERROR_NODE_IS_NULL = OperationResult.USER_SPACE_START + 3;
        public const ushort ERROR_NODE_UNAVAILABLE = OperationResult.USER_SPACE_START + 4;
        public const ushort ERROR_OPTION_NOT_FOUND = OperationResult.USER_SPACE_START + 5;
        public const ushort ERROR_OPTION_UNAVAILABLE = OperationResult.USER_SPACE_START + 6;
        public const ushort ERROR_DIALOGUE_NOT_RUNNING = OperationResult.USER_SPACE_START + 7;
        public const ushort ERROR_RENDERER_INVALID = OperationResult.USER_SPACE_START + 8;
        public const ushort ERROR_ANOTHER_DIALOGUE_RUNNING = OperationResult.USER_SPACE_START + 9;

        public static OperationResult Permitted() =>
            OperationResult.Success(SYSTEM_DIALOGUE, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Started() =>
            OperationResult.Success(SYSTEM_DIALOGUE, SUCCESS_STARTED);

        public static OperationResult Finished() =>
            OperationResult.Success(SYSTEM_DIALOGUE, SUCCESS_FINISHED);

        public static OperationResult Interrupted() =>
            OperationResult.Success(SYSTEM_DIALOGUE, SUCCESS_INTERRUPTED);

        public static OperationResult NodeEntered() =>
            OperationResult.Success(SYSTEM_DIALOGUE, SUCCESS_NODE_ENTERED);

        public static OperationResult OptionSelected() =>
            OperationResult.Success(SYSTEM_DIALOGUE, SUCCESS_OPTION_SELECTED);

        public static OperationResult GraphIsNull() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_GRAPH_IS_NULL);

        public static OperationResult EntryNotFound() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_ENTRY_NOT_FOUND);

        public static OperationResult NodeIsNull() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_NODE_IS_NULL);

        public static OperationResult NodeUnavailable() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_NODE_UNAVAILABLE);

        public static OperationResult OptionNotFound() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_OPTION_NOT_FOUND);

        public static OperationResult OptionUnavailable() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_OPTION_UNAVAILABLE);

        public static OperationResult DialogueNotRunning() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_DIALOGUE_NOT_RUNNING);

        public static OperationResult RendererInvalid() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_RENDERER_INVALID);

        public static OperationResult AnotherDialogueRunning() =>
            OperationResult.Error(SYSTEM_DIALOGUE, ERROR_ANOTHER_DIALOGUE_RUNNING);
    }
}
