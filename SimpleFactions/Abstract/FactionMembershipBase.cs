using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Interfaces;
using Systems.SimpleFactions.Operations;
using UnityEngine;

namespace Systems.SimpleFactions.Abstract
{
    /// <summary>
    ///     Non-generic base for <see cref="FactionMembershipBase{THolder}"/>.
    ///     Attach to any <c>GameObject</c> to enable faction membership tracking.
    ///     Use <c>GetComponents&lt;FactionMembershipBase&gt;()</c> to enumerate all faction
    ///     memberships on an object without knowing the holder type.
    /// </summary>
    public abstract class FactionMembershipBase : MonoBehaviour
    {
        /// <summary>Returns whether this object is currently a member of <paramref name="faction"/>.</summary>
        public abstract bool IsMember([NotNull] FactionBase faction);

        /// <summary>Returns the current reputation with <paramref name="faction"/>.</summary>
        public abstract long GetReputation([NotNull] FactionBase faction);

        /// <summary>
        ///     Returns the currently active <see cref="ReputationLevelBase"/> for
        ///     <paramref name="faction"/>, or <c>null</c> if no level has been assigned.
        /// </summary>
        [CanBeNull] public abstract ReputationLevelBase GetCurrentLevel([NotNull] FactionBase faction);
    }

    /// <summary>
    ///     Single-component faction tracker. Attach one instance to a <c>GameObject</c> to manage
    ///     membership and reputation across <em>any number</em> of factions. Runtime state for each
    ///     faction is stored in an internal dictionary keyed by the faction's concrete type.
    /// </summary>
    /// <typeparam name="THolder">
    ///     The type of the holder object that is passed to <see cref="FactionBase{TFactionObject}"/>
    ///     callbacks. The default implementation of <see cref="GetHolder"/> returns
    ///     <c>this</c> cast to <typeparamref name="THolder"/>, then falls back to
    ///     <c>GetComponent&lt;THolder&gt;()</c>.
    /// </typeparam>
    public abstract class FactionMembershipBase<THolder> : FactionMembershipBase, IHolderProvider<THolder>
        where THolder : class
    {
        private readonly Dictionary<Type, FactionMemberState> _states = new();

        /// <summary>
        ///     Returns the holder object associated with this membership component.
        ///     Override to provide a specific reference (e.g. a sibling component or a
        ///     serialized field).
        /// </summary>
        [CanBeNull]
        protected virtual THolder GetHolder()
        {
            if (this is THolder self) return self;
            return GetComponent<THolder>();
        }

        [CanBeNull] THolder IHolderProvider<THolder>.Holder => GetHolder();

        private FactionMemberState GetOrCreate([NotNull] Type key)
        {
            if (_states.TryGetValue(key, out FactionMemberState state)) return state;
            state = new FactionMemberState();
            _states[key] = state;
            return state;
        }

        #region Non-generic overrides

        /// <inheritdoc/>
        public sealed override bool IsMember(FactionBase faction)
        {
            return _states.TryGetValue(faction.GetType(), out FactionMemberState state) && state.isMember;
        }

        /// <inheritdoc/>
        public sealed override long GetReputation(FactionBase faction)
        {
            return _states.TryGetValue(faction.GetType(), out FactionMemberState state) ? state.reputation : 0L;
        }

        /// <inheritdoc/>
        public sealed override ReputationLevelBase GetCurrentLevel(FactionBase faction)
        {
            if (!_states.TryGetValue(faction.GetType(), out FactionMemberState state)) return null;
            IReadOnlyList<ReputationLevelBase> levels = faction.Levels;
            if (state.currentLevelIndex < 0 || state.currentLevelIndex >= levels.Count) return null;
            return levels[state.currentLevelIndex];
        }

        #endregion

        #region Typed public API

        /// <summary>
        ///     Attempts to join <typeparamref name="TFaction"/>.
        ///     Fires <see cref="OnJoinedFaction{TFaction}"/> and the faction's
        ///     <see cref="FactionBase.OnJoined"/> on success.
        /// </summary>
        public OperationResult JoinFaction<TFaction>(ActionSource actionSource = ActionSource.External)
            where TFaction : FactionBase<THolder>, new()
        {
            TFaction faction = FactionDatabase.GetExact<TFaction>();
            if (ReferenceEquals(faction, null)) return FactionOperations.FactionNotFound();

            FactionMemberState state = GetOrCreate(typeof(TFaction));

            if (state.isMember) return FactionOperations.AlreadyMember();

            JoinFactionContext nonGenericCtx = new(this, faction);
            JoinFactionContext<THolder> typedCtx = new(GetHolder(), faction);

            OperationResult factionCheck = faction.CanJoin(nonGenericCtx);
            if (!factionCheck)
            {
                if (actionSource != ActionSource.Internal) OnJoinFailed<TFaction>(typedCtx, factionCheck);
                return factionCheck;
            }

            OperationResult memberCheck = CanJoinFaction<TFaction>(typedCtx);
            if (!memberCheck)
            {
                if (actionSource != ActionSource.Internal) OnJoinFailed<TFaction>(typedCtx, memberCheck);
                return memberCheck;
            }

            state.isMember = true;

            OperationResult result = FactionOperations.Joined();
            if (actionSource != ActionSource.Internal) OnJoinedFaction<TFaction>(typedCtx, result);
            return result;
        }

        /// <summary>
        ///     Attempts to leave <typeparamref name="TFaction"/>.
        ///     Fires <see cref="OnLeftFaction{TFaction}"/> and the faction's
        ///     <see cref="FactionBase.OnLeft"/> on success.
        /// </summary>
        public OperationResult LeaveFaction<TFaction>(ActionSource actionSource = ActionSource.External)
            where TFaction : FactionBase<THolder>, new()
        {
            TFaction faction = FactionDatabase.GetExact<TFaction>();
            if (ReferenceEquals(faction, null)) return FactionOperations.FactionNotFound();

            FactionMemberState state = GetOrCreate(typeof(TFaction));

            if (!state.isMember) return FactionOperations.NotAMember();

            LeaveFactionContext nonGenericCtx = new(this, faction);
            LeaveFactionContext<THolder> typedCtx = new(GetHolder(), faction);

            OperationResult factionCheck = faction.CanLeave(nonGenericCtx);
            if (!factionCheck)
            {
                if (actionSource != ActionSource.Internal) OnLeaveFailed<TFaction>(typedCtx, factionCheck);
                return factionCheck;
            }

            OperationResult memberCheck = CanLeaveFaction<TFaction>(typedCtx);
            if (!memberCheck)
            {
                if (actionSource != ActionSource.Internal) OnLeaveFailed<TFaction>(typedCtx, memberCheck);
                return memberCheck;
            }

            state.isMember = false;

            OperationResult result = FactionOperations.Left();
            if (actionSource != ActionSource.Internal) OnLeftFaction<TFaction>(typedCtx, result);
            return result;
        }

        /// <summary>
        ///     Adds <paramref name="amount"/> to the reputation with <typeparamref name="TFaction"/>
        ///     (use a negative value to subtract). Fires <see cref="OnReputationChanged{TFaction}"/>
        ///     and automatically evaluates promotion/demotion thresholds when the faction has levels.
        /// </summary>
        public OperationResult ChangeReputation<TFaction>(long amount, ActionSource actionSource = ActionSource.External)
            where TFaction : FactionBase<THolder>, new()
        {
            if (amount == 0) return FactionOperations.InvalidReputation();

            TFaction faction = FactionDatabase.GetExact<TFaction>();
            if (ReferenceEquals(faction, null)) return FactionOperations.FactionNotFound();

            FactionMemberState state = GetOrCreate(typeof(TFaction));

            if (!state.isMember) return FactionOperations.NotAMember();

            long previousReputation = state.reputation;
            ReputationChangeContext nonGenericCtx = new(this, faction, amount, previousReputation);
            ReputationChangeContext<THolder> typedCtx = new(GetHolder(), faction, amount, previousReputation);

            OperationResult factionCheck = faction.CanChangeReputation(nonGenericCtx);
            if (!factionCheck)
            {
                if (actionSource != ActionSource.Internal) OnReputationChangeFailed<TFaction>(typedCtx, factionCheck);
                return factionCheck;
            }

            OperationResult memberCheck = CanChangeReputation<TFaction>(typedCtx);
            if (!memberCheck)
            {
                if (actionSource != ActionSource.Internal) OnReputationChangeFailed<TFaction>(typedCtx, memberCheck);
                return memberCheck;
            }

            state.reputation += amount;

            OperationResult result = FactionOperations.ReputationChanged();
            if (actionSource != ActionSource.Internal) OnReputationChanged<TFaction>(typedCtx, result);

            if (faction.Levels.Count > 0)
                HandleAutomaticLevelChange(faction, state, previousReputation, state.reputation, actionSource);

            return result;
        }

        /// <summary>
        ///     Manually assigns <paramref name="level"/> as the active reputation level for
        ///     <typeparamref name="TFaction"/>. Pass <c>null</c> to clear the active level.
        ///     This bypasses <see cref="CanBePromoted{TFaction}"/> and
        ///     <see cref="CanBeDemoted{TFaction}"/> — it is an unconditional override (e.g. a king
        ///     granting knighthood regardless of reputation score).
        /// </summary>
        public OperationResult AssignLevel<TFaction>([CanBeNull] ReputationLevelBase level, ActionSource actionSource = ActionSource.External)
            where TFaction : FactionBase<THolder>, new()
        {
            TFaction faction = FactionDatabase.GetExact<TFaction>();
            if (ReferenceEquals(faction, null)) return FactionOperations.FactionNotFound();

            FactionMemberState state = GetOrCreate(typeof(TFaction));

            if (!state.isMember) return FactionOperations.NotAMember();

            if (ReferenceEquals(level, null))
            {
                int previousIndex = state.currentLevelIndex;
                ReputationLevelBase previousLevel = GetLevelAt(faction, previousIndex);
                state.currentLevelIndex = -1;

                if (actionSource != ActionSource.Internal)
                    FireLevelCallbacks(faction, state, previousLevel, null, previousIndex, -1, isPromotion: false);

                return FactionOperations.LevelCleared();
            }

            int targetIndex = faction.GetLevelIndex(level);
            if (targetIndex == -1) return FactionOperations.LevelNotInFaction();

            int prevIndex = state.currentLevelIndex;
            bool isPromotion = targetIndex > prevIndex;
            ReputationLevelBase prevLevel = GetLevelAt(faction, prevIndex);
            state.currentLevelIndex = targetIndex;

            if (actionSource != ActionSource.Internal)
                FireLevelCallbacks(faction, state, prevLevel, level, prevIndex, targetIndex, isPromotion);

            return FactionOperations.LevelAssigned();
        }

        /// <summary>
        ///     Returns <c>true</c> if the object is a member of <typeparamref name="TFaction"/>.
        /// </summary>
        public bool IsMemberOf<TFaction>() where TFaction : FactionBase<THolder>, new()
        {
            return _states.TryGetValue(typeof(TFaction), out FactionMemberState state) && state.isMember;
        }

        /// <summary>
        ///     Returns the current reputation with <typeparamref name="TFaction"/>.
        ///     Returns <c>0</c> if no state exists yet.
        /// </summary>
        public long GetReputation<TFaction>() where TFaction : FactionBase<THolder>, new()
        {
            return _states.TryGetValue(typeof(TFaction), out FactionMemberState state) ? state.reputation : 0L;
        }

        /// <summary>
        ///     Returns the active <see cref="ReputationLevelBase"/> for <typeparamref name="TFaction"/>,
        ///     or <c>null</c> if no level is assigned or the faction is not found.
        /// </summary>
        [CanBeNull]
        public ReputationLevelBase GetCurrentLevel<TFaction>() where TFaction : FactionBase<THolder>, new()
        {
            TFaction faction = FactionDatabase.GetExact<TFaction>();
            if (ReferenceEquals(faction, null)) return null;
            if (!_states.TryGetValue(typeof(TFaction), out FactionMemberState state)) return null;
            return GetLevelAt(faction, state.currentLevelIndex);
        }

        /// <summary>
        ///     Returns <c>true</c> if the object's current level index for
        ///     <typeparamref name="TFaction"/> is greater than or equal to the index of
        ///     <paramref name="level"/> in the faction's level list.
        /// </summary>
        public bool IsAtLeastLevel<TFaction>([NotNull] ReputationLevelBase level)
            where TFaction : FactionBase<THolder>, new()
        {
            TFaction faction = FactionDatabase.GetExact<TFaction>();
            if (ReferenceEquals(faction, null)) return false;

            int requiredIndex = faction.GetLevelIndex(level);
            if (requiredIndex == -1) return false;

            if (!_states.TryGetValue(typeof(TFaction), out FactionMemberState state)) return false;
            return state.currentLevelIndex >= requiredIndex;
        }

        #endregion

        #region Member-level checks (overridable)

        /// <summary>
        ///     Member-level check for joining <typeparamref name="TFaction"/>.
        ///     Called after <see cref="FactionBase.CanJoin"/> passes.
        ///     Override to add conditions specific to this component.
        /// </summary>
        protected virtual OperationResult CanJoinFaction<TFaction>(in JoinFactionContext<THolder> context)
            where TFaction : FactionBase<THolder>, new()
            => FactionOperations.Permitted();

        /// <summary>
        ///     Member-level check for leaving <typeparamref name="TFaction"/>.
        ///     Called after <see cref="FactionBase.CanLeave"/> passes.
        ///     Override to add conditions specific to this component.
        /// </summary>
        protected virtual OperationResult CanLeaveFaction<TFaction>(in LeaveFactionContext<THolder> context)
            where TFaction : FactionBase<THolder>, new()
            => FactionOperations.Permitted();

        /// <summary>
        ///     Member-level check for a reputation change in <typeparamref name="TFaction"/>.
        ///     Called after <see cref="FactionBase.CanChangeReputation"/> passes.
        ///     Override to add conditions specific to this component.
        /// </summary>
        protected virtual OperationResult CanChangeReputation<TFaction>(in ReputationChangeContext<THolder> context)
            where TFaction : FactionBase<THolder>, new()
            => FactionOperations.Permitted();

        /// <summary>
        ///     Member-level check for an automatic promotion in <typeparamref name="TFaction"/>.
        ///     Called after both faction and target-level checks pass.
        ///     Override to add conditions specific to this component.
        /// </summary>
        protected virtual OperationResult CanBePromoted<TFaction>(in FactionLevelChangeContext<THolder> context)
            where TFaction : FactionBase<THolder>, new()
            => FactionOperations.Permitted();

        /// <summary>
        ///     Member-level check for an automatic demotion in <typeparamref name="TFaction"/>.
        ///     Called after faction, current-level, and target-level checks pass.
        ///     Override to add conditions specific to this component.
        /// </summary>
        protected virtual OperationResult CanBeDemoted<TFaction>(in FactionLevelChangeContext<THolder> context)
            where TFaction : FactionBase<THolder>, new()
            => FactionOperations.Permitted();

        #endregion

        #region Member-level events (overridable, delegate to faction config)

        /// <summary>
        ///     Called after a successful join. Default implementation fires
        ///     <see cref="FactionBase.OnJoined"/> on the faction config.
        ///     Override to add per-component behaviour before or instead of the delegation.
        /// </summary>
        protected virtual void OnJoinedFaction<TFaction>(in JoinFactionContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            JoinFactionContext nonGenericCtx = new(this, context.faction);
            context.faction.OnJoined(nonGenericCtx, result);
        }

        /// <summary>Called after a failed join attempt. Delegates to <see cref="FactionBase.OnJoinFailed"/>.</summary>
        protected virtual void OnJoinFailed<TFaction>(in JoinFactionContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            JoinFactionContext nonGenericCtx = new(this, context.faction);
            context.faction.OnJoinFailed(nonGenericCtx, result);
        }

        /// <summary>Called after a successful leave. Delegates to <see cref="FactionBase.OnLeft"/>.</summary>
        protected virtual void OnLeftFaction<TFaction>(in LeaveFactionContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            LeaveFactionContext nonGenericCtx = new(this, context.faction);
            context.faction.OnLeft(nonGenericCtx, result);
        }

        /// <summary>Called after a failed leave attempt. Delegates to <see cref="FactionBase.OnLeaveFailed"/>.</summary>
        protected virtual void OnLeaveFailed<TFaction>(in LeaveFactionContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            LeaveFactionContext nonGenericCtx = new(this, context.faction);
            context.faction.OnLeaveFailed(nonGenericCtx, result);
        }

        /// <summary>Called after reputation changes. Delegates to <see cref="FactionBase.OnReputationChanged"/>.</summary>
        protected virtual void OnReputationChanged<TFaction>(in ReputationChangeContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            ReputationChangeContext nonGenericCtx = new(this, context.faction, context.amountRequested, context.previousReputation);
            context.faction.OnReputationChanged(nonGenericCtx, result);
        }

        /// <summary>Called after a failed reputation change. Delegates to <see cref="FactionBase.OnReputationChangeFailed"/>.</summary>
        protected virtual void OnReputationChangeFailed<TFaction>(in ReputationChangeContext<THolder> context, in OperationResult result)
            where TFaction : FactionBase<THolder>, new()
        {
            ReputationChangeContext nonGenericCtx = new(this, context.faction, context.amountRequested, context.previousReputation);
            context.faction.OnReputationChangeFailed(nonGenericCtx, result);
        }

        #endregion

        #region Level change helpers

        private void HandleAutomaticLevelChange<TFaction>(
            [NotNull] TFaction faction,
            FactionMemberState state,
            long previousRep,
            long newRep,
            ActionSource actionSource)
            where TFaction : FactionBase<THolder>, new()
        {
            IReadOnlyList<ReputationLevelBase> levels = faction.Levels;
            int levelCount = levels.Count;

            // PROMOTION — only when reputation increased
            if (newRep > previousRep)
            {
                int bestPromotion = -1;
                for (int i = 0; i < levelCount; i++)
                {
                    ReputationLevelBase candidate = levels[i];
                    if (!candidate.AutomaticPromotion) continue;
                    if (newRep >= candidate.PromotionThreshold && i > state.currentLevelIndex)
                        bestPromotion = i;
                }

                if (bestPromotion != -1)
                {
                    int prevIndex = state.currentLevelIndex;
                    ReputationLevelBase prevLevel = GetLevelAt(faction, prevIndex);
                    ReputationLevelBase newLevel = levels[bestPromotion];

                    FactionLevelChangeContext levelCtx = new(this, faction, prevLevel, newLevel, prevIndex, bestPromotion);
                    FactionLevelChangeContext<THolder> typedLevelCtx = new(GetHolder(), faction, prevLevel, newLevel, prevIndex, bestPromotion);

                    OperationResult factionCheck = faction.CanBePromoted(levelCtx);
                    if (!factionCheck) return;

                    OperationResult levelCheck = newLevel.CanPromoteTo(levelCtx);
                    if (!levelCheck) return;

                    OperationResult memberCheck = CanBePromoted<TFaction>(typedLevelCtx);
                    if (!memberCheck) return;

                    state.currentLevelIndex = bestPromotion;
                    if (actionSource != ActionSource.Internal)
                        FireLevelCallbacks(faction, state, prevLevel, newLevel, prevIndex, bestPromotion, isPromotion: true);
                }

                return;
            }

            // DEMOTION — only when reputation decreased and a level is currently active
            if (newRep < previousRep && state.currentLevelIndex >= 0)
            {
                while (state.currentLevelIndex >= 0)
                {
                    int demotionIndex = -1;
                    for (int i = state.currentLevelIndex; i >= 0; i--)
                    {
                        ReputationLevelBase currentAtIndex = levels[i];
                        if (!currentAtIndex.AutomaticDemotion) continue;
                        if (newRep >= currentAtIndex.DemotionThreshold) continue;
                        demotionIndex = i;
                        break;
                    }

                    if (demotionIndex == -1) break;

                    int prevIndex = state.currentLevelIndex;
                    int newIndex = demotionIndex - 1;
                    ReputationLevelBase prevLevel = levels[prevIndex];
                    ReputationLevelBase newLevel = GetLevelAt(faction, newIndex);

                    FactionLevelChangeContext levelCtx = new(this, faction, prevLevel, newLevel, prevIndex, newIndex);
                    FactionLevelChangeContext<THolder> typedLevelCtx = new(GetHolder(), faction, prevLevel, newLevel, prevIndex, newIndex);

                    OperationResult factionCheck = faction.CanBeDemoted(levelCtx);
                    if (!factionCheck) break;

                    OperationResult fromCheck = prevLevel.CanDemoteFrom(levelCtx);
                    if (!fromCheck) break;

                    if (!ReferenceEquals(newLevel, null))
                    {
                        OperationResult toCheck = newLevel.CanDemoteTo(levelCtx);
                        if (!toCheck) break;
                    }

                    OperationResult memberCheck = CanBeDemoted<TFaction>(typedLevelCtx);
                    if (!memberCheck) break;

                    state.currentLevelIndex = newIndex;
                    if (actionSource != ActionSource.Internal)
                        FireLevelCallbacks(faction, state, prevLevel, newLevel, prevIndex, newIndex, isPromotion: false);
                }
            }
        }

        private void FireLevelCallbacks(
            [NotNull] FactionBase faction,
            FactionMemberState state,
            [CanBeNull] ReputationLevelBase previousLevel,
            [CanBeNull] ReputationLevelBase newLevel,
            int previousIndex,
            int newIndex,
            bool isPromotion)
        {
            OperationResult result = ReferenceEquals(newLevel, null)
                ? FactionOperations.LevelCleared()
                : FactionOperations.LevelAssigned();

            FactionLevelChangeContext levelCtx = new(this, faction, previousLevel, newLevel, previousIndex, newIndex);

            if (!ReferenceEquals(newLevel, null))
            {
                newLevel.OnLevelChanged(levelCtx, result);
                if (isPromotion)
                {
                    newLevel.OnLevelAchieved(levelCtx, result);
                    newLevel.OnLevelIncreased(levelCtx, result);
                }
                else
                {
                    newLevel.OnLevelDecreased(levelCtx, result);
                }
            }

            faction.OnLevelChanged(levelCtx, result);
        }

        [CanBeNull]
        private static ReputationLevelBase GetLevelAt([NotNull] FactionBase faction, int index)
        {
            IReadOnlyList<ReputationLevelBase> levels = faction.Levels;
            if (index < 0 || index >= levels.Count) return null;
            return levels[index];
        }

        #endregion
    }
}
