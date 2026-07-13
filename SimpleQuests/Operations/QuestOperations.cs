using Systems.SimpleCore.Operations;

namespace Systems.SimpleQuests.Operations
{
    public static class QuestOperations
    {
        public const ushort SYSTEM_QUESTS = 0x0009;

        public const ushort SUCCESS_STARTED = 0x0001;
        public const ushort ALREADY_STARTED = 0x0002;
        public const ushort QUEST_NOT_FOUND = 0x0003;
        public const ushort ALREADY_FINISHED = 0x0004;
        
        public static OperationResult Permitted() => 
            OperationResult.Success(SYSTEM_QUESTS, OperationResult.SUCCESS_PERMITTED);
        
        public static OperationResult QuestAlreadyStarted() => 
            OperationResult.Error(SYSTEM_QUESTS, ALREADY_STARTED);
        
        public static OperationResult QuestAlreadyFinished() => 
            OperationResult.Error(SYSTEM_QUESTS, ALREADY_FINISHED);
        
        public static OperationResult QuestNotFound() =>
            OperationResult.Error(SYSTEM_QUESTS, QUEST_NOT_FOUND);

        public static OperationResult Started() =>
            OperationResult.Success(SYSTEM_QUESTS, SUCCESS_STARTED);
    }
}