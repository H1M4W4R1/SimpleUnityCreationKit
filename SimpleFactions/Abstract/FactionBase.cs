using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Interfaces;
using Systems.SimpleFactions.Operations;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using UnityEngine;

[assembly: InternalsVisibleTo("SimpleFactions.Tests")]
namespace Systems.SimpleFactions.Abstract
{
    /// <summary>
    ///     Base class for all factions. Each faction is an addressable configuration asset that can
    ///     validate membership and own outgoing SimpleRelations.
    /// </summary>
    [AutoCreate("Factions", FactionDatabase.LABEL)]
    public abstract class FactionBase : ScriptableObject, IRelatable
    {
        [SerializeField] private List<RelationEntry> _relationEntries = new List<RelationEntry>();

        List<RelationEntry> IRelatable.RelationEntries => _relationEntries;

        /// <summary>Removes every runtime relation currently owned by this faction.</summary>
        internal void ClearRelations()
        {
            _relationEntries.Clear();
        }

        /// <summary>Determines whether the object described by <paramref name="context"/> may join this faction.</summary>
        [UsedImplicitly]
        protected internal virtual OperationResult CanJoin(in JoinFactionContext context)
            => FactionOperations.Permitted();

        /// <summary>Determines whether the object described by <paramref name="context"/> may leave this faction.</summary>
        [UsedImplicitly]
        protected internal virtual OperationResult CanLeave(in LeaveFactionContext context)
            => FactionOperations.Permitted();

        /// <summary>Called when an object successfully joins this faction.</summary>
        protected internal virtual void OnJoined(in JoinFactionContext context, in OperationResult result) { }

        /// <summary>Called when a join attempt on this faction fails.</summary>
        protected internal virtual void OnJoinFailed(in JoinFactionContext context, in OperationResult result) { }

        /// <summary>Called when an object successfully leaves this faction.</summary>
        protected internal virtual void OnLeft(in LeaveFactionContext context, in OperationResult result) { }

        /// <summary>Called when a leave attempt on this faction fails.</summary>
        protected internal virtual void OnLeaveFailed(in LeaveFactionContext context, in OperationResult result) { }
    }

    /// <summary>Generic faction base whose membership callbacks receive a typed holder reference.</summary>
    /// <typeparam name="TFactionObject">Type of object that can become a member.</typeparam>
    public abstract class FactionBase<TFactionObject> : FactionBase where TFactionObject : class
    {
        /// <inheritdoc/>
        protected internal sealed override OperationResult CanJoin(in JoinFactionContext context)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            JoinFactionContext<TFactionObject> typedContext = new JoinFactionContext<TFactionObject>(member, context.faction);
            return CanJoin(in typedContext);
        }

        /// <summary>Validates a typed membership join.</summary>
        protected internal virtual OperationResult CanJoin(in JoinFactionContext<TFactionObject> context)
            => FactionOperations.Permitted();

        /// <inheritdoc/>
        protected internal sealed override OperationResult CanLeave(in LeaveFactionContext context)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            LeaveFactionContext<TFactionObject> typedContext = new LeaveFactionContext<TFactionObject>(member, context.faction);
            return CanLeave(in typedContext);
        }

        /// <summary>Validates a typed membership leave.</summary>
        protected internal virtual OperationResult CanLeave(in LeaveFactionContext<TFactionObject> context)
            => FactionOperations.Permitted();

        /// <inheritdoc/>
        protected internal sealed override void OnJoined(in JoinFactionContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            JoinFactionContext<TFactionObject> typedContext = new JoinFactionContext<TFactionObject>(member, context.faction);
            OnJoined(in typedContext, in result);
        }

        /// <summary>Called after a typed membership join succeeds.</summary>
        protected internal virtual void OnJoined(in JoinFactionContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnJoinFailed(in JoinFactionContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            JoinFactionContext<TFactionObject> typedContext = new JoinFactionContext<TFactionObject>(member, context.faction);
            OnJoinFailed(in typedContext, in result);
        }

        /// <summary>Called after a typed membership join is rejected.</summary>
        protected internal virtual void OnJoinFailed(in JoinFactionContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnLeft(in LeaveFactionContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            LeaveFactionContext<TFactionObject> typedContext = new LeaveFactionContext<TFactionObject>(member, context.faction);
            OnLeft(in typedContext, in result);
        }

        /// <summary>Called after a typed membership leave succeeds.</summary>
        protected internal virtual void OnLeft(in LeaveFactionContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnLeaveFailed(in LeaveFactionContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            LeaveFactionContext<TFactionObject> typedContext = new LeaveFactionContext<TFactionObject>(member, context.faction);
            OnLeaveFailed(in typedContext, in result);
        }

        /// <summary>Called after a typed membership leave is rejected.</summary>
        protected internal virtual void OnLeaveFailed(in LeaveFactionContext<TFactionObject> context, in OperationResult result) { }
    }
}
