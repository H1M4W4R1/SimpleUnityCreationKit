using System;

namespace Systems.SimpleQuests.Data.Enums
{
    [Flags] public enum StartQuestFlags
    {
        AllowStartUniqueIfRunning = 1,
        AllowStartUniqueIfFinished = 2
    }
}