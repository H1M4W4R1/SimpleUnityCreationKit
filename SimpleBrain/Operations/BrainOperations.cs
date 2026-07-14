using Systems.SimpleCore.Operations;

namespace Systems.SimpleBrain.Operations
{
    /// <summary>
    ///     Operation results emitted by SimpleBrain.
    /// </summary>
    public static class BrainOperations
    {
        public const ushort SYSTEM_BRAIN = 0x0021;

        public const ushort SUCCESS_KNOWLEDGE_LEARNED = 1;
        public const ushort SUCCESS_KNOWLEDGE_ALREADY_LEARNED = 2;
        public const ushort SUCCESS_DECISION_MADE = 3;
        public const ushort SUCCESS_SUBPROCESS_STARTED = 4;
        public const ushort SUCCESS_SUBPROCESS_ALREADY_RUNNING = 5;
        public const ushort SUCCESS_SUBPROCESS_STOPPED = 6;
        public const ushort SUCCESS_SUBPROCESS_ALREADY_STOPPED = 7;
        public const ushort SUCCESS_SUBPROCESS_PAUSED = 8;
        public const ushort SUCCESS_SUBPROCESS_RESUMED = 9;
        public const ushort SUCCESS_SUBPROCESS_FINISHED = 10;
        public const ushort SUCCESS_COMA_ENTERED = 11;
        public const ushort SUCCESS_ALREADY_IN_COMA = 12;
        public const ushort SUCCESS_COMA_EXITED = 13;

        public const ushort ERROR_SUBPROCESS_NOT_CREATED = 1;
        public const ushort ERROR_SUBPROCESS_NOT_RUNNING = 2;
        public const ushort ERROR_SUBPROCESS_NOT_PAUSED = 3;
        public const ushort ERROR_SUBPROCESS_IS_PAUSED = 4;
        public const ushort ERROR_SUBPROCESS_NOT_OWNED = 5;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_BRAIN, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult KnowledgeLearned()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_KNOWLEDGE_LEARNED);

        public static OperationResult KnowledgeAlreadyLearned()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_KNOWLEDGE_ALREADY_LEARNED);

        public static OperationResult DecisionMade()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_DECISION_MADE);

        public static OperationResult SubprocessStarted()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_SUBPROCESS_STARTED);

        public static OperationResult SubprocessAlreadyRunning()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_SUBPROCESS_ALREADY_RUNNING);

        public static OperationResult SubprocessStopped()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_SUBPROCESS_STOPPED);

        public static OperationResult SubprocessAlreadyStopped()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_SUBPROCESS_ALREADY_STOPPED);

        public static OperationResult SubprocessPaused()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_SUBPROCESS_PAUSED);

        public static OperationResult SubprocessResumed()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_SUBPROCESS_RESUMED);

        public static OperationResult SubprocessFinished()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_SUBPROCESS_FINISHED);

        public static OperationResult ComaEntered()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_COMA_ENTERED);

        public static OperationResult AlreadyInComa()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_ALREADY_IN_COMA);

        public static OperationResult ComaExited()
            => OperationResult.Success(SYSTEM_BRAIN, SUCCESS_COMA_EXITED);

        public static OperationResult SubprocessNotCreated()
            => OperationResult.Error(SYSTEM_BRAIN, ERROR_SUBPROCESS_NOT_CREATED);

        public static OperationResult SubprocessNotRunning()
            => OperationResult.Error(SYSTEM_BRAIN, ERROR_SUBPROCESS_NOT_RUNNING);

        public static OperationResult SubprocessNotPaused()
            => OperationResult.Error(SYSTEM_BRAIN, ERROR_SUBPROCESS_NOT_PAUSED);

        public static OperationResult SubprocessIsPaused()
            => OperationResult.Error(SYSTEM_BRAIN, ERROR_SUBPROCESS_IS_PAUSED);

        public static OperationResult SubprocessNotOwned()
            => OperationResult.Error(SYSTEM_BRAIN, ERROR_SUBPROCESS_NOT_OWNED);
    }
}
