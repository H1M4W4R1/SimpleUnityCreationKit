using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Operations;

namespace Systems.SimpleRelations.Abstract
{
    /// <summary>Domain contract for a Unity object that owns outgoing relationships.</summary>
    public interface IRelatable : IRegisterInDatabase<RelatableObjectDatabase>
    {
        /// <summary>
        ///     Serialized backing entries owned by this relatable. Implement this member explicitly so entry
        ///     storage stays unavailable to code outside the relatable implementation.
        /// </summary>
        protected List<RelationEntry> RelationEntries { get; }

        /// <summary>All serialized outgoing relationships owned by this object.</summary>
        IReadOnlyList<RelationEntry> Relations => RelationEntries;

        /// <summary>Gets the current value, or the type's initial value when no entry exists.</summary>
        int GetRelationValue([CanBeNull] RelationTypeBase relationType, IRelatable target)
        {
            if (ReferenceEquals(relationType, null) || !relationType) return 0;
            if (!TryGetRelation(relationType, target, out RelationEntry relation))
                return relationType.GetInitialValue(this, target);

            return relation!.Value;
        }

        /// <summary>Attempts to locate the serialized entry for the supplied type and target.</summary>
        bool TryGetRelation([CanBeNull] RelationTypeBase relationType, IRelatable target, [CanBeNull] out RelationEntry relation)
        {
            relation = null;
            if (ReferenceEquals(relationType, null) || !relationType) return false;
            if (!RelationEntry.IsSupportedRelatable(target)) return false;

            List<RelationEntry> relationEntries = RelationEntries;
            if (ReferenceEquals(relationEntries, null)) return false;

            for (int index = 0; index < relationEntries.Count; index++)
            {
                RelationEntry current = relationEntries[index];
                if (ReferenceEquals(current, null)) continue;
                if (!current.Matches(relationType, target)) continue;

                relation = current;
                return true;
            }

            return false;
        }

        /// <summary>Attempts to locate an entry for a relation type resolved from its generic type.</summary>
        bool TryGetRelation<TRelationType>(IRelatable target, [CanBeNull] out RelationEntry relation)
            where TRelationType : RelationTypeBase, new()
        {
            relation = null;
            TRelationType relationType = RelationTypeDatabase.GetExact<TRelationType>();
            if (ReferenceEquals(relationType, null) || !relationType) return false;
            return TryGetRelation(relationType, target, out relation);
        }

        /// <summary>Attempts to read an entry value without falling back to the relation type's initial value.</summary>
        bool TryGetRelationValue(RelationTypeBase relationType, IRelatable target, out int value)
        {
            value = 0;
            if (!TryGetRelation(relationType, target, out RelationEntry relation)) return false;
            value = relation!.Value;
            return true;
        }

        /// <summary>Attempts to read an entry value for a relation type resolved from its generic type.</summary>
        bool TryGetRelationValue<TRelationType>(IRelatable target, out int value)
            where TRelationType : RelationTypeBase, new()
        {
            value = 0;
            if (!TryGetRelation<TRelationType>(target, out RelationEntry relation)) return false;
            value = relation!.Value;
            return true;
        }

        /// <summary>Applies the resolved change represented by <paramref name="context"/>.</summary>
        OperationResult ChangeRelation(in RelationChangeContext context)
        {
            RelationTypeBase relationType = context.relationType;
            if (ReferenceEquals(relationType, null) || !relationType)
                return RelationOperations.InvalidTarget();

            return relationType.ChangeRelation(this, in context);
        }

        /// <summary>Applies the resolved assignment represented by <paramref name="context"/>.</summary>
        OperationResult SetRelation(in RelationSetContext context)
        {
            RelationTypeBase relationType = context.relationType;
            if (ReferenceEquals(relationType, null) || !relationType)
                return RelationOperations.InvalidTarget();

            return relationType.SetRelation(this, in context);
        }

        /// <summary>Writes a resolved relation value after its relation type approves the operation.</summary>
        internal void SetRelationValue(
            RelationTypeBase relationType,
            IRelatable target,
            int previousValue,
            int newValue)
        {
            if (TryGetRelation(relationType, target, out RelationEntry relation))
            {
                relation!.SetValue(newValue);
                return;
            }

            List<RelationEntry> relationEntries = RelationEntries;
            if (ReferenceEquals(relationEntries, null)) return;

            RelationEntry newRelation = new(relationType, target, previousValue);
            newRelation.SetValue(newValue);
            relationEntries.Add(newRelation);
        }
    }
}
