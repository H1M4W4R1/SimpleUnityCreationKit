using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Operations;
using UnityEngine;

namespace Systems.SimpleRelations.Components
{
    /// <summary>
    ///     Serialized component that owns one-way relationships to other relation components.
    ///     Attach a concrete subclass to each actor that should track relations.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class RelationComponentBase : MonoBehaviour, IRelatable
    {
        [SerializeField] private RelationStorage _relations = new();

        /// <inheritdoc />
        public IReadOnlyList<RelationEntry> Relations => _relations.Entries;

        /// <inheritdoc />
        public int GetRelationValue([NotNull] RelationTypeBase relationType, [NotNull] IRelatable target)
        {
            return _relations.GetValue(relationType, target);
        }

        /// <inheritdoc />
        public bool TryGetRelation(
            [NotNull] RelationTypeBase relationType,
            [NotNull] IRelatable target,
            [CanBeNull] out RelationEntry relation)
        {
            return _relations.TryGet(relationType, target, out relation);
        }

        /// <summary>
        ///     Attempts to locate a serialized entry for the configured generic relation type.
        /// </summary>
        public bool TryGetRelation<TRelationType>(
            [NotNull] IRelatable target,
            [CanBeNull] out RelationEntry relation)
            where TRelationType : RelationTypeBase, new()
        {
            relation = null;
            TRelationType relationType = RelationTypeDatabase.GetExact<TRelationType>();
            if (ReferenceEquals(relationType, null) || !relationType) return false;
            return TryGetRelation(relationType, target, out relation);
        }

        /// <summary>Attempts to read a serialized relation value without falling back to its default.</summary>
        public bool TryGetRelationValue(
            [NotNull] RelationTypeBase relationType,
            [NotNull] IRelatable target,
            out int value)
        {
            value = 0;
            if (!TryGetRelation(relationType, target, out RelationEntry relation)) return false;
            value = relation.Value;
            return true;
        }

        /// <summary>Attempts to read a serialized value for a configured relation type.</summary>
        public bool TryGetRelationValue<TRelationType>([NotNull] IRelatable target, out int value)
            where TRelationType : RelationTypeBase, new()
        {
            value = 0;
            if (!TryGetRelation<TRelationType>(target, out RelationEntry relation)) return false;
            value = relation.Value;
            return true;
        }

        /// <inheritdoc />
        public OperationResult ChangeRelation(in RelationChangeContext context)
        {
            if (context.amountRequested == 0) return RelationOperations.InvalidAmount();
            if (!TryValidateTarget(context.relationType, context.target)) return RelationOperations.InvalidTarget();

            int previousValue = _relations.GetValue(context.relationType, context.target);
            if (WouldOverflow(previousValue, context.amountRequested)) return RelationOperations.ValueOverflow();

            int newValue = previousValue + context.amountRequested;
            RelationChangeContext evaluatedContext = new(
                context.relationType,
                context.target,
                context.amountRequested,
                previousValue,
                newValue);
            OperationResult validationResult = CanChangeRelation(in evaluatedContext);
            if (!validationResult)
            {
                OnRelationChangeFailed(in evaluatedContext, in validationResult);
                return validationResult;
            }

            _relations.SetValue(context.relationType, context.target, previousValue, newValue);
            OnRelationChanged(in evaluatedContext);
            return RelationOperations.RelationChanged();
        }

        /// <inheritdoc />
        public OperationResult SetRelation(in RelationSetContext context)
        {
            if (!TryValidateTarget(context.relationType, context.target)) return RelationOperations.InvalidTarget();

            int previousValue = _relations.GetValue(context.relationType, context.target);
            RelationSetContext evaluatedContext = new(
                context.relationType,
                context.target,
                context.value,
                previousValue);
            OperationResult validationResult = CanSetRelation(in evaluatedContext);
            if (!validationResult)
            {
                OnRelationSetFailed(in evaluatedContext, in validationResult);
                return validationResult;
            }

            _relations.SetValue(context.relationType, context.target, previousValue, context.value);
            OnRelationSet(in evaluatedContext);
            return RelationOperations.RelationSet();
        }

        /// <summary>Validates a relation change before its value is written.</summary>
        protected virtual OperationResult CanChangeRelation(in RelationChangeContext context)
        {
            return RelationOperations.Permitted();
        }

        /// <summary>Validates an exact relation assignment before its value is written.</summary>
        protected virtual OperationResult CanSetRelation(in RelationSetContext context)
        {
            return RelationOperations.Permitted();
        }

        /// <summary>Called after a relation change has been written successfully.</summary>
        protected virtual void OnRelationChanged(in RelationChangeContext context) { }

        /// <summary>Called when <see cref="CanChangeRelation"/> rejects a change.</summary>
        protected virtual void OnRelationChangeFailed(
            in RelationChangeContext context,
            in OperationResult result) { }

        /// <summary>Called after an exact relation value has been written successfully.</summary>
        protected virtual void OnRelationSet(in RelationSetContext context) { }

        /// <summary>Called when <see cref="CanSetRelation"/> rejects an assignment.</summary>
        protected virtual void OnRelationSetFailed(
            in RelationSetContext context,
            in OperationResult result) { }

        private bool TryValidateTarget(
            [CanBeNull] RelationTypeBase relationType,
            [CanBeNull] IRelatable target)
        {
            if (ReferenceEquals(relationType, null) || !relationType) return false;
            if (!RelationEntry.IsSupportedRelatable(target)) return false;
            return !ReferenceEquals(target, this);
        }

        private static bool WouldOverflow(int value, int amount)
        {
            if (amount > 0) return value > int.MaxValue - amount;
            return value < int.MinValue - amount;
        }
    }
}
