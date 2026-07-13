using System;
using JetBrains.Annotations;

namespace Systems.SimpleEntities.Data.Enums
{
    [Flags] [Obsolete] [UsedImplicitly]
    public enum EntityTickFlags
    {
        None = 0,
        ForceTick = 1 << 0,
    }
}