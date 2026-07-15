using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Interfaces;
using Systems.SimpleFactions.Operations;
using UnityEngine;

namespace Systems.SimpleFactions.Abstract
{
    /// <summary>Non-generic base for components that track faction membership.</summary>
    public abstract class FactionMembershipBase : MonoBehaviour
    {
        /// <summary>Returns whether this object is currently a member of <paramref name="faction"/>.</summary>
        public abstract bool IsMember([NotNull] FactionBase faction);
    }

    /// <summary>
    ///     Component that tracks membership in any number of factions. Reputation is intentionally
    ///     not membership state; model it as a relation owned by a faction instead.
    /// </summary>
    /// <typeparam name="THolder">Type passed to typed faction membership callbacks.</typeparam>
    public abstract class FactionMembershipBase<THolder> : FactionMembershipBase, IHolderProvider<THolder>
        where THolder : class
    {
        private readonly Dictionary<Type, FactionMemberState> _states = new Dictionary<Type, FactionMemberState>();

        /// <summary>Resolves the holder associated with this membership component.</summary>
        [CanBeNull]
        protected virtual THolder GetHolder()
        {
            if (this is THolder self) return self;
            return GetComponent<THolder>();
        }

        [CanBeNull] THolder IHolderProvider<THolder>.Holder => GetHolder();

        private FactionMemberState GetOrCreate([NotNull] Type factionType)
        {
            if (_states.TryGetValue(factionType, out FactionMemberState state)) return state;

            state = new FactionMemberState();
            _states[factionType] = state;
            return state;
        }

        /// <inheritdoc/>
        public sealed override bool IsMember(FactionBase faction)
        {
            return _states.TryGetValue(faction.GetType(), out FactionMemberState state) && state.isMember;
        }

        /// <summary>Attempts to join <typeparamref name="TFaction"/>.</summary>
        public OperationResult JoinFaction<TFaction>() where TFaction : FactionBase<THolder>, new()
        {
            TFaction faction = FactionDatabase.GetExact<TFaction>();
            if (ReferenceEquals(faction, null) || !faction) return FactionOperations.FactionNotFound();

            FactionMemberState state = GetOrCreate(typeof(TFaction));
            if (state.isMember) return FactionOperations.AlreadyMember();

            JoinFactionContext context = new JoinFactionContext(this, faction);
            JoinFactionContext<THolder> typedContext = new JoinFactionContext<THolder>(GetHolder(), faction);
            OperationResult factionResult = faction.CanJoin(in context);
            if (!factionResult)
            {
                OnJoinFailed<TFaction>(in typedContext, in factionResult);
                return factionResult;
            }

            OperationResult memberResult = CanJoinFaction<TFaction>(in typedContext);
            if (!memberResult)
            {
                OnJoinFailed<TFaction>(in typedContext, in memberResult);
                return memberResult;
            }

            state.isMember = true;
            OperationResult result = FactionOperations.Joined();
            OnJoinedFaction<TFaction>(in typedContext, in result);
            return result;
        }

        /// <summary>Attempts to leave <typeparamref name="TFaction"/>.</summary>
        public OperationResult LeaveFaction<TFaction>() where TFaction : FactionBase<THolder>, new()
        {
            TFaction faction = FactionDatabase.GetExact<TFaction>();
            if (ReferenceEquals(faction, null) || !faction) return FactionOperations.FactionNotFound();

            FactionMemberState state = GetOrCreate(typeof(TFaction));
            if (!state.isMember) return FactionOperations.NotAMember();

            LeaveFactionContext context = new LeaveFactionContext(this, faction);
            LeaveFactionContext<THolder> typedContext = new LeaveFactionContext<THolder>(GetHolder(), faction);
            OperationResult factionResult = faction.CanLeave(in context);
            if (!factionResult)
            {
                OnLeaveFailed<TFaction>(in typedContext, in factionResult);
                return factionResult;
            }

            OperationResult memberResult = CanLeaveFaction<TFaction>(in typedContext);
            if (!memberResult)
            {
                OnLeaveFailed<TFaction>(in typedContext, in memberResult);
                return memberResult;
            }

            state.isMember = false;
            OperationResult result = FactionOperations.Left();
            OnLeftFaction<TFaction>(in typedContext, in result);
            return result;
        }

        /// <summary>Returns whether this component is a member of <typeparamref name="TFaction"/>.</summary>
        public bool IsMemberOf<TFaction>() where TFaction : FactionBase<THolder>, new()
        {
            return _states.TryGetValue(typeof(TFaction), out FactionMemberState state) && state.isMember;
        }

        /// <summary>Validates a component-specific join after the faction accepts it.</summary>
        protected virtual OperationResult CanJoinFaction<TFaction>(in JoinFactionContext<THolder> context)
            where TFaction : FactionBase<THolder>, new()
            => FactionOperations.Permitted();

        /// <summary>Validates a component-specific leave after the faction accepts it.</summary>
        protected virtual OperationResult CanLeaveFaction<TFaction>(in LeaveFactionContext<THolder> context)
            where TFaction : FactionBase<THolder>, new()
            => FactionOperations.Permitted();

        /// <summary>Called after a successful join. The default implementation notifies the faction.</summary>
        protected virtual void OnJoinedFaction<TFaction>(in JoinFactionContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            JoinFactionContext untypedContext = new JoinFactionContext(this, context.faction);
            context.faction.OnJoined(in untypedContext, in result);
        }

        /// <summary>Called after a rejected join. The default implementation notifies the faction.</summary>
        protected virtual void OnJoinFailed<TFaction>(in JoinFactionContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            JoinFactionContext untypedContext = new JoinFactionContext(this, context.faction);
            context.faction.OnJoinFailed(in untypedContext, in result);
        }

        /// <summary>Called after a successful leave. The default implementation notifies the faction.</summary>
        protected virtual void OnLeftFaction<TFaction>(in LeaveFactionContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            LeaveFactionContext untypedContext = new LeaveFactionContext(this, context.faction);
            context.faction.OnLeft(in untypedContext, in result);
        }

        /// <summary>Called after a rejected leave. The default implementation notifies the faction.</summary>
        protected virtual void OnLeaveFailed<TFaction>(in LeaveFactionContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            LeaveFactionContext untypedContext = new LeaveFactionContext(this, context.faction);
            context.faction.OnLeaveFailed(in untypedContext, in result);
        }
    }
}
