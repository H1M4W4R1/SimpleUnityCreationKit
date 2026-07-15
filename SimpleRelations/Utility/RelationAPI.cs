using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Operations;

namespace Systems.SimpleRelations.Utility
{
    /// <summary>Static facade for typed SimpleRelations operations.</summary>
    public static class RelationAPI
    {
        /// <summary>Changes the typed outgoing relation described by <paramref name="context"/>.</summary>
        public static OperationResult Change<TRelationType>(in RelationChangeContext<TRelationType> context)
            where TRelationType : RelationTypeBase, new()
        {
            if (!AreValid(context.source, context.target)) return RelationOperations.InvalidTarget();

            TRelationType relationType = RelationTypeDatabase.GetExact<TRelationType>();
            if (ReferenceEquals(relationType, null) || !relationType)
                return RelationOperations.RelationTypeNotFound();

            RelationChangeContext resolvedContext = new(relationType, context.target, context.amount);
            return context.source.ChangeRelation(in resolvedContext);
        }

        /// <summary>Sets the typed outgoing relation described by <paramref name="context"/>.</summary>
        public static OperationResult Set<TRelationType>(in RelationSetContext<TRelationType> context)
            where TRelationType : RelationTypeBase, new()
        {
            if (!AreValid(context.source, context.target)) return RelationOperations.InvalidTarget();

            TRelationType relationType = RelationTypeDatabase.GetExact<TRelationType>();
            if (ReferenceEquals(relationType, null) || !relationType)
                return RelationOperations.RelationTypeNotFound();

            RelationSetContext resolvedContext = new(relationType, context.target, context.value);
            return context.source.SetRelation(in resolvedContext);
        }

        /// <summary>Tries to query the typed outgoing relation described by <paramref name="context"/>.</summary>
        public static bool TryGetValue<TRelationType>(
            in RelationQueryContext<TRelationType> context,
            out int value)
            where TRelationType : RelationTypeBase, new()
        {
            value = 0;
            if (!AreValid(context.source, context.target)) return false;

            TRelationType relationType = RelationTypeDatabase.GetExact<TRelationType>();
            if (ReferenceEquals(relationType, null) || !relationType) return false;

            value = context.source.GetRelationValue(relationType, context.target);
            return true;
        }

        /// <summary>Returns the typed value, or zero when the request cannot be resolved.</summary>
        public static int GetValue<TRelationType>(in RelationQueryContext<TRelationType> context)
            where TRelationType : RelationTypeBase, new()
        {
            return TryGetValue(context, out int value) ? value : 0;
        }

        private static bool AreValid([CanBeNull] IRelatable source, [CanBeNull] IRelatable target)
        {
            if (ReferenceEquals(source, null) || ReferenceEquals(target, null)) return false;
            if (!RelationEntry.IsSupportedRelatable(source)) return false;
            if (!RelationEntry.IsSupportedRelatable(target)) return false;
            return !ReferenceEquals(source, target);
        }
    }
}
