using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Interfaces;
using Systems.SimpleFactions.Operations;
using UnityEngine;

[assembly: InternalsVisibleTo("SimpleFactions.Editor")]
[assembly: InternalsVisibleTo("SimpleFactions.Tests")]
namespace Systems.SimpleFactions.Abstract
{
    /// <summary>
    ///     Base class for all factions. Extend this (or <see cref="FactionBase{TFactionObject}"/>)
    ///     to create a custom faction with its own behaviour. Concrete sealed subclasses are
    ///     auto-created in <c>Assets/Generated/Factions/</c> and registered in
    ///     <see cref="FactionDatabase"/> via the <c>AutoCreate</c> attribute.
    /// </summary>
    /// <remarks>
    ///     Faction levels are optional. Assign <see cref="ReputationLevelBase"/> assets to
    ///     <c>_levels</c> in the Inspector, or implement <see cref="IForFaction{TFaction}"/> on
    ///     your level types to have them assigned automatically on script reload.
    /// </remarks>
    [AutoCreate("Factions", FactionDatabase.LABEL)]
    public abstract class FactionBase : ScriptableObject
    {
        [SerializeField] private List<ReputationLevelBase> _levels = new();

        /// <summary>
        ///     Ordered list of reputation levels for this faction (index 0 = lowest rank).
        ///     May be empty if this faction does not use a reputation level system.
        /// </summary>
        public IReadOnlyList<ReputationLevelBase> Levels => _levels;

        /// <summary>
        ///     Returns the zero-based index of <paramref name="level"/> inside
        ///     <see cref="Levels"/>, or <c>-1</c> if it is not present.
        /// </summary>
        public int GetLevelIndex([NotNull] ReputationLevelBase level)
        {
            for (int i = 0; i < _levels.Count; i++)
            {
                if (ReferenceEquals(_levels[i], level)) return i;
            }

            return -1;
        }

        /// <summary>
        ///     Adds <paramref name="level"/> to <see cref="Levels"/> if it is not already present,
        ///     then sorts the list by <see cref="ReputationLevelBase.PromotionThreshold"/> ascending.
        ///     Called by the editor postprocessor when processing <see cref="IForFaction{TFaction}"/>
        ///     implementations.
        /// </summary>
        /// <returns><c>true</c> when the list changed; otherwise <c>false</c>.</returns>
        internal bool AssignLevel([NotNull] ReputationLevelBase level)
        {
            for (int i = 0; i < _levels.Count; i++)
            {
                if (ReferenceEquals(_levels[i], level)) return false;
            }

            _levels.Add(level);
            SortLevels();
            return true;
        }

        /// <summary>
        ///     Removes <paramref name="level"/> from <see cref="Levels"/>.
        ///     Called by the editor postprocessor when a level's <see cref="IForFaction{TFaction}"/>
        ///     implementation is removed or changed.
        /// </summary>
        internal void RemoveLevel([NotNull] ReputationLevelBase level)
        {
            for (int i = _levels.Count - 1; i >= 0; i--)
            {
                if (!ReferenceEquals(_levels[i], level)) continue;
                _levels.RemoveAt(i);
                return;
            }
        }

        private void SortLevels()
        {
            _levels.Sort((a, b) =>
                a.PromotionThreshold.CompareTo(b.PromotionThreshold));
        }

        #region Checks

        /// <summary>
        ///     Determines whether the object described by <paramref name="context"/> may join
        ///     this faction. Override to add custom join conditions.
        /// </summary>
        [UsedImplicitly] protected internal virtual OperationResult CanJoin(in JoinFactionContext context)
            => FactionOperations.Permitted();

        /// <summary>
        ///     Determines whether the object described by <paramref name="context"/> may leave
        ///     this faction. Override to add custom leave conditions.
        /// </summary>
        [UsedImplicitly] protected internal virtual OperationResult CanLeave(in LeaveFactionContext context)
            => FactionOperations.Permitted();

        /// <summary>
        ///     Determines whether the reputation change described by <paramref name="context"/>
        ///     is permitted. Override to add custom conditions.
        /// </summary>
        [UsedImplicitly] protected internal virtual OperationResult CanChangeReputation(in ReputationChangeContext context)
            => FactionOperations.Permitted();

        /// <summary>
        ///     Determines whether the promotion described by <paramref name="context"/> is permitted.
        ///     Override to block or gate automatic promotions at the faction level.
        /// </summary>
        [UsedImplicitly] protected internal virtual OperationResult CanBePromoted(in FactionLevelChangeContext context)
            => FactionOperations.Permitted();

        /// <summary>
        ///     Determines whether the demotion described by <paramref name="context"/> is permitted.
        ///     Override to block or gate automatic demotions at the faction level.
        /// </summary>
        [UsedImplicitly] protected internal virtual OperationResult CanBeDemoted(in FactionLevelChangeContext context)
            => FactionOperations.Permitted();

        #endregion

        #region Events

        /// <summary>Called when an object successfully joins this faction.</summary>
        protected internal virtual void OnJoined(in JoinFactionContext context, in OperationResult result) { }

        /// <summary>Called when a join attempt on this faction fails.</summary>
        protected internal virtual void OnJoinFailed(in JoinFactionContext context, in OperationResult result) { }

        /// <summary>Called when an object successfully leaves this faction.</summary>
        protected internal virtual void OnLeft(in LeaveFactionContext context, in OperationResult result) { }

        /// <summary>Called when a leave attempt on this faction fails.</summary>
        protected internal virtual void OnLeaveFailed(in LeaveFactionContext context, in OperationResult result) { }

        /// <summary>Called when an object's reputation with this faction changes successfully.</summary>
        protected internal virtual void OnReputationChanged(in ReputationChangeContext context, in OperationResult result) { }

        /// <summary>Called when a reputation change attempt on this faction fails.</summary>
        protected internal virtual void OnReputationChangeFailed(in ReputationChangeContext context, in OperationResult result) { }

        /// <summary>Called when an object's active reputation level for this faction changes.</summary>
        protected internal virtual void OnLevelChanged(in FactionLevelChangeContext context, in OperationResult result) { }

        #endregion
    }

    /// <summary>
    ///     Generic base for factions whose callbacks carry a typed reference to the member holder.
    ///     Override the typed <c>On*</c> overloads (e.g. <see cref="OnJoined(in JoinFactionContext{TFactionObject}, in OperationResult)"/>)
    ///     instead of the non-generic ones.
    /// </summary>
    /// <typeparam name="TFactionObject">
    ///     The type of the holder object that can be a member of this faction
    ///     (e.g. a player controller MonoBehaviour or an interface).
    /// </typeparam>
    public abstract class FactionBase<TFactionObject> : FactionBase where TFactionObject : class
    {
        #region Typed check overrides

        /// <inheritdoc/>
        protected internal sealed override OperationResult CanJoin(in JoinFactionContext context)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            return CanJoin(new JoinFactionContext<TFactionObject>(member, context.faction));
        }

        /// <summary>
        ///     Typed override of <see cref="FactionBase.CanJoin"/>.
        ///     Override this to add custom join conditions with a strongly-typed member reference.
        /// </summary>
        protected internal virtual OperationResult CanJoin(in JoinFactionContext<TFactionObject> context)
            => FactionOperations.Permitted();

        /// <inheritdoc/>
        protected internal sealed override OperationResult CanLeave(in LeaveFactionContext context)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            return CanLeave(new LeaveFactionContext<TFactionObject>(member, context.faction));
        }

        /// <summary>
        ///     Typed override of <see cref="FactionBase.CanLeave"/>.
        ///     Override this to add custom leave conditions with a strongly-typed member reference.
        /// </summary>
        protected internal virtual OperationResult CanLeave(in LeaveFactionContext<TFactionObject> context)
            => FactionOperations.Permitted();

        /// <inheritdoc/>
        protected internal sealed override OperationResult CanChangeReputation(in ReputationChangeContext context)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            return CanChangeReputation(new ReputationChangeContext<TFactionObject>(
                member, context.faction, context.amountRequested, context.previousReputation));
        }

        /// <summary>
        ///     Typed override of <see cref="FactionBase.CanChangeReputation"/>.
        ///     Override this to add custom conditions with a strongly-typed member reference.
        /// </summary>
        protected internal virtual OperationResult CanChangeReputation(in ReputationChangeContext<TFactionObject> context)
            => FactionOperations.Permitted();

        /// <inheritdoc/>
        protected internal sealed override OperationResult CanBePromoted(in FactionLevelChangeContext context)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            return CanBePromoted(new FactionLevelChangeContext<TFactionObject>(
                member, context.faction, context.previousLevel, context.newLevel,
                context.previousLevelIndex, context.newLevelIndex));
        }

        /// <summary>
        ///     Typed override of <see cref="FactionBase.CanBePromoted"/>.
        ///     Override this to block or gate automatic promotions with a strongly-typed member reference.
        /// </summary>
        protected internal virtual OperationResult CanBePromoted(in FactionLevelChangeContext<TFactionObject> context)
            => FactionOperations.Permitted();

        /// <inheritdoc/>
        protected internal sealed override OperationResult CanBeDemoted(in FactionLevelChangeContext context)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            return CanBeDemoted(new FactionLevelChangeContext<TFactionObject>(
                member, context.faction, context.previousLevel, context.newLevel,
                context.previousLevelIndex, context.newLevelIndex));
        }

        /// <summary>
        ///     Typed override of <see cref="FactionBase.CanBeDemoted"/>.
        ///     Override this to block or gate automatic demotions with a strongly-typed member reference.
        /// </summary>
        protected internal virtual OperationResult CanBeDemoted(in FactionLevelChangeContext<TFactionObject> context)
            => FactionOperations.Permitted();

        #endregion

        #region Typed event overrides

        /// <inheritdoc/>
        protected internal sealed override void OnJoined(in JoinFactionContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            OnJoined(new JoinFactionContext<TFactionObject>(member, context.faction), result);
        }

        /// <summary>
        ///     Typed override called when an object successfully joins this faction.
        ///     Override to react with a strongly-typed member reference.
        /// </summary>
        protected internal virtual void OnJoined(in JoinFactionContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnJoinFailed(in JoinFactionContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            OnJoinFailed(new JoinFactionContext<TFactionObject>(member, context.faction), result);
        }

        /// <summary>
        ///     Typed override called when a join attempt fails.
        ///     Override to react with a strongly-typed member reference.
        /// </summary>
        protected internal virtual void OnJoinFailed(in JoinFactionContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnLeft(in LeaveFactionContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            OnLeft(new LeaveFactionContext<TFactionObject>(member, context.faction), result);
        }

        /// <summary>
        ///     Typed override called when an object successfully leaves this faction.
        ///     Override to react with a strongly-typed member reference.
        /// </summary>
        protected internal virtual void OnLeft(in LeaveFactionContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnLeaveFailed(in LeaveFactionContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            OnLeaveFailed(new LeaveFactionContext<TFactionObject>(member, context.faction), result);
        }

        /// <summary>
        ///     Typed override called when a leave attempt fails.
        ///     Override to react with a strongly-typed member reference.
        /// </summary>
        protected internal virtual void OnLeaveFailed(in LeaveFactionContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnReputationChanged(in ReputationChangeContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            OnReputationChanged(new ReputationChangeContext<TFactionObject>(
                member, context.faction, context.amountRequested, context.previousReputation), result);
        }

        /// <summary>
        ///     Typed override called when reputation changes successfully.
        ///     Override to react with a strongly-typed member reference.
        /// </summary>
        protected internal virtual void OnReputationChanged(in ReputationChangeContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnReputationChangeFailed(in ReputationChangeContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            OnReputationChangeFailed(new ReputationChangeContext<TFactionObject>(
                member, context.faction, context.amountRequested, context.previousReputation), result);
        }

        /// <summary>
        ///     Typed override called when a reputation change attempt fails.
        ///     Override to react with a strongly-typed member reference.
        /// </summary>
        protected internal virtual void OnReputationChangeFailed(in ReputationChangeContext<TFactionObject> context, in OperationResult result) { }

        /// <inheritdoc/>
        protected internal sealed override void OnLevelChanged(in FactionLevelChangeContext context, in OperationResult result)
        {
            TFactionObject member = (context.membership as IHolderProvider<TFactionObject>)?.Holder;
            OnLevelChanged(new FactionLevelChangeContext<TFactionObject>(
                member, context.faction, context.previousLevel, context.newLevel,
                context.previousLevelIndex, context.newLevelIndex), result);
        }

        /// <summary>
        ///     Typed override called when an object's active reputation level changes.
        ///     Override to react with a strongly-typed member reference.
        /// </summary>
        protected internal virtual void OnLevelChanged(in FactionLevelChangeContext<TFactionObject> context, in OperationResult result) { }

        #endregion
    }
}
