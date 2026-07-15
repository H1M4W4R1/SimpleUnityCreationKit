using System;
using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleSaving.Abstract;

namespace Systems.SimpleFactions.Data.SaveFiles
{
    /// <summary>Serializable snapshot of one faction's outgoing relation values.</summary>
    [Serializable]
    public sealed class FactionRelationSaveFile : SaveFileBase
    {
        /// <summary>Serialized relation entries owned by the faction being saved.</summary>
        [NotNull] public FactionRelationSaveEntry[] Entries = Array.Empty<FactionRelationSaveEntry>();
    }

    /// <summary>Serializable value for one source faction, target, and relation type.</summary>
    [Serializable]
    public sealed class FactionRelationSaveEntry
    {
        /// <summary>Persistent <see cref="SimpleCore.Identifiers.HashIdentifier"/> value of the source faction type.</summary>
        public ulong SourceFactionTypeHash;

        /// <summary>Persistent <see cref="SimpleCore.Identifiers.HashIdentifier"/> value of the target faction type.</summary>
        public ulong TargetFactionTypeHash;

        /// <summary>Kind of target represented by this entry.</summary>
        public FactionRelationTargetKind TargetKind;

        /// <summary>Stable identifier of a runtime target when <see cref="TargetKind"/> is runtime.</summary>
        public Snowflake128 TargetRuntimeIdentifier;

        /// <summary>Persistent <see cref="SimpleCore.Identifiers.HashIdentifier"/> value of the relation type.</summary>
        public ulong RelationTypeHash;

        /// <summary>Saved numeric relation value.</summary>
        public int Value;
    }

    /// <summary>Describes how a saved faction relation target is resolved.</summary>
    public enum FactionRelationTargetKind : byte
    {
        /// <summary>The target is an addressable faction asset resolved by concrete type hash.</summary>
        Faction = 0,

        /// <summary>The target is a registered runtime object resolved by <see cref="Snowflake128"/>.</summary>
        RuntimeObject = 1
    }
}
