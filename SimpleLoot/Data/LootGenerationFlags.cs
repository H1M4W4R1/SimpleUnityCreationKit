using System;

namespace Systems.SimpleLoot.Data
{
    [Flags]
    public enum LootGenerationFlags : uint
    {
        None = 0,
        IgnoreConditions = 1 << 0,
    }
}
