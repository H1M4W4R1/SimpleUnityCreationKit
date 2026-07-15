using System.Collections.Generic;
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Data;

namespace Systems.SimpleRelations.Abstract
{
    /// <summary>Domain contract for a component that owns outgoing relationships.</summary>
    public interface IRelatable
    {
        /// <summary>All serialized outgoing relationships owned by this object.</summary>
        IReadOnlyList<RelationEntry> Relations { get; }

        /// <summary>Gets the current value, or the type's initial value when no entry exists.</summary>
        int GetRelationValue(RelationTypeBase relationType, IRelatable target);

        /// <summary>Attempts to locate the serialized entry for the supplied type and target.</summary>
        bool TryGetRelation(RelationTypeBase relationType, IRelatable target, out RelationEntry relation);

        /// <summary>Applies the resolved change represented by <paramref name="context"/>.</summary>
        OperationResult ChangeRelation(in RelationChangeContext context);

        /// <summary>Applies the resolved assignment represented by <paramref name="context"/>.</summary>
        OperationResult SetRelation(in RelationSetContext context);
    }
}
