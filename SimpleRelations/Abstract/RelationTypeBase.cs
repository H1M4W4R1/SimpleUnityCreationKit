using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Operations;
using UnityEngine;

namespace Systems.SimpleRelations.Abstract
{
    /// <summary>
    ///     Configuration for one independently tracked kind of relationship, such as trust,
    ///     affinity, fear, friendship, rivalry, or hostility.
    /// </summary>
    /// <remarks>
    ///     Each concrete relation type is generated as an addressable asset. The asset resolves the value
    ///     used when a relation is first created; interpretation of the resulting value is deliberately left
    ///     to game logic or a progression system.
    /// </remarks>
    [AutoCreate("Relations", RelationTypeDatabase.LABEL)]
    public abstract class RelationTypeBase : ScriptableObject
    {
        /// <summary>Resolves the value assigned when this relation type is first created for a source and target.</summary>
        protected virtual int GetInitialValue(in RelationInitialValueContext context)
        {
            return 0;
        }

        internal int GetInitialValue(IRelatable source, IRelatable target)
        {
            RelationInitialValueContext context = new(source, target);
            return GetInitialValue(in context);
        }

        /// <summary>Executes a validated change to a relation of this type.</summary>
        internal OperationResult ChangeRelation(IRelatable source, in RelationChangeContext context)
        {
            if (!IsValidTarget(source, context.target)) return RelationOperations.InvalidTarget();
            if (context.amountRequested == 0) return RelationOperations.InvalidAmount();

            int previousValue = source.GetRelationValue(this, context.target);
            if (WouldOverflow(previousValue, context.amountRequested))
                return RelationOperations.ValueOverflow();

            int newValue = previousValue + context.amountRequested;
            RelationChangeContext evaluatedContext = new(
                this,
                source,
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

            source.SetRelationValue(this, context.target, previousValue, newValue);
            OnRelationChanged(in evaluatedContext);
            return RelationOperations.RelationChanged();
        }

        /// <summary>Executes a validated exact assignment to a relation of this type.</summary>
        internal OperationResult SetRelation(IRelatable source, in RelationSetContext context)
        {
            if (!IsValidTarget(source, context.target)) return RelationOperations.InvalidTarget();

            int previousValue = source.GetRelationValue(this, context.target);
            RelationSetContext evaluatedContext = new(this, source, context.target, context.value, previousValue);
            OperationResult validationResult = CanSetRelation(in evaluatedContext);
            if (!validationResult)
            {
                OnRelationSetFailed(in evaluatedContext, in validationResult);
                return validationResult;
            }

            source.SetRelationValue(this, context.target, previousValue, context.value);
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

        private static bool IsValidTarget(IRelatable source, IRelatable target)
        {
            if (!RelationEntry.IsSupportedRelatable(source)) return false;
            if (!RelationEntry.IsSupportedRelatable(target)) return false;
            return !ReferenceEquals(source, target);
        }

        private static bool WouldOverflow(int value, int amount)
        {
            if (amount > 0) return value > int.MaxValue - amount;
            return value < int.MinValue - amount;
        }
    }
}
