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
        /// <summary>Changes a relation resolved from a generic relation type without creating a context.</summary>
        public static OperationResult Change<TRelationType>(IRelatable source, IRelatable target, int amount)
            where TRelationType : RelationTypeBase, new()
        {
            TRelationType relationType = RelationTypeDatabase.GetExact<TRelationType>();
            if (ReferenceEquals(relationType, null) || !relationType)
                return RelationOperations.RelationTypeNotFound();

            return Change(source, target, relationType, amount);
        }

        /// <summary>Changes a relation using the supplied relation type asset without creating a context.</summary>
        public static OperationResult Change(
            IRelatable source,
            IRelatable target,
            RelationTypeBase relationType,
            int amount)
        {
            if (!AreValid(source, target)) return RelationOperations.InvalidTarget();
            if (ReferenceEquals(relationType, null) || !relationType)
                return RelationOperations.RelationTypeNotFound();

            RelationChangeContext context = new(relationType, target, amount);
            return source.ChangeRelation(in context);
        }

        /// <summary>Changes the typed outgoing relation described by <paramref name="context"/>.</summary>
        public static OperationResult Change<TRelationType>(in RelationChangeContext<TRelationType> context)
            where TRelationType : RelationTypeBase, new()
        {
            return Change<TRelationType>(context.source, context.target, context.amount);
        }

        /// <summary>Sets a relation resolved from a generic relation type without creating a context.</summary>
        public static OperationResult Set<TRelationType>(IRelatable source, IRelatable target, int value)
            where TRelationType : RelationTypeBase, new()
        {
            TRelationType relationType = RelationTypeDatabase.GetExact<TRelationType>();
            if (ReferenceEquals(relationType, null) || !relationType)
                return RelationOperations.RelationTypeNotFound();

            return Set(source, target, relationType, value);
        }

        /// <summary>Sets a relation using the supplied relation type asset without creating a context.</summary>
        public static OperationResult Set(
            IRelatable source,
            IRelatable target,
            RelationTypeBase relationType,
            int value)
        {
            if (!AreValid(source, target)) return RelationOperations.InvalidTarget();
            if (ReferenceEquals(relationType, null) || !relationType)
                return RelationOperations.RelationTypeNotFound();

            RelationSetContext context = new(relationType, target, value);
            return source.SetRelation(in context);
        }

        /// <summary>Sets the typed outgoing relation described by <paramref name="context"/>.</summary>
        public static OperationResult Set<TRelationType>(in RelationSetContext<TRelationType> context)
            where TRelationType : RelationTypeBase, new()
        {
            return Set<TRelationType>(context.source, context.target, context.value);
        }

        /// <summary>Tries to get a relation resolved from a generic relation type without creating a context.</summary>
        public static bool TryGetValue<TRelationType>(IRelatable source, IRelatable target, out int value)
            where TRelationType : RelationTypeBase, new()
        {
            TRelationType relationType = RelationTypeDatabase.GetExact<TRelationType>();
            return TryGetValue(source, target, relationType, out value);
        }

        /// <summary>Tries to get a relation using the supplied relation type asset without creating a context.</summary>
        public static bool TryGetValue(
            IRelatable source,
            IRelatable target,
            RelationTypeBase relationType,
            out int value)
        {
            value = 0;
            if (!AreValid(source, target)) return false;
            if (ReferenceEquals(relationType, null) || !relationType) return false;

            value = source.GetRelationValue(relationType, target);
            return true;
        }

        /// <summary>Tries to query the typed outgoing relation described by <paramref name="context"/>.</summary>
        public static bool TryGetValue<TRelationType>(
            in RelationQueryContext<TRelationType> context,
            out int value)
            where TRelationType : RelationTypeBase, new()
        {
            return TryGetValue<TRelationType>(context.source, context.target, out value);
        }

        /// <summary>Gets a relation resolved from a generic relation type without creating a context.</summary>
        public static int GetValue<TRelationType>(IRelatable source, IRelatable target)
            where TRelationType : RelationTypeBase, new()
        {
            return TryGetValue<TRelationType>(source, target, out int value) ? value : 0;
        }

        /// <summary>Gets a relation using the supplied relation type asset without creating a context.</summary>
        public static int GetValue(IRelatable source, IRelatable target, RelationTypeBase relationType)
        {
            return TryGetValue(source, target, relationType, out int value) ? value : 0;
        }

        /// <summary>Returns the typed value, or zero when the request cannot be resolved.</summary>
        public static int GetValue<TRelationType>(in RelationQueryContext<TRelationType> context)
            where TRelationType : RelationTypeBase, new()
        {
            return GetValue<TRelationType>(context.source, context.target);
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
