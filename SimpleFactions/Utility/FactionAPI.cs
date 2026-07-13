using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleFactions.Abstract;
using UnityEngine;

namespace Systems.SimpleFactions.Utility
{
    /// <summary>
    ///     Static facade for the SimpleFactions system. All faction operations can be performed
    ///     through this class without holding direct references to internal components.
    /// </summary>
    public static class FactionAPI
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState() { }

        /// <summary>
        ///     Attempts to make the object tracked by <paramref name="membership"/> join
        ///     <typeparamref name="TFaction"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="Operations.FactionOperations.Joined()"/> on success,
        ///     or an error result describing the reason for failure.
        /// </returns>
        public static OperationResult Join<TFaction, THolder>(
            [NotNull] FactionMembershipBase<THolder> membership,
            ActionSource actionSource = ActionSource.External)
            where TFaction : FactionBase<THolder>, new()
            where THolder : class
            => membership.JoinFaction<TFaction>(actionSource);

        /// <summary>
        ///     Attempts to make the object tracked by <paramref name="membership"/> leave
        ///     <typeparamref name="TFaction"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="Operations.FactionOperations.Left()"/> on success,
        ///     or an error result describing the reason for failure.
        /// </returns>
        public static OperationResult Leave<TFaction, THolder>(
            [NotNull] FactionMembershipBase<THolder> membership,
            ActionSource actionSource = ActionSource.External)
            where TFaction : FactionBase<THolder>, new()
            where THolder : class
            => membership.LeaveFaction<TFaction>(actionSource);

        /// <summary>
        ///     Adds <paramref name="amount"/> to the object's reputation with
        ///     <typeparamref name="TFaction"/>. Use a negative value to subtract reputation.
        ///     Automatic promotion and demotion thresholds are evaluated after the change.
        /// </summary>
        /// <returns>
        ///     <see cref="Operations.FactionOperations.ReputationChanged()"/> on success,
        ///     or an error result describing the reason for failure.
        /// </returns>
        public static OperationResult ChangeReputation<TFaction, THolder>(
            [NotNull] FactionMembershipBase<THolder> membership,
            long amount,
            ActionSource actionSource = ActionSource.External)
            where TFaction : FactionBase<THolder>, new()
            where THolder : class
            => membership.ChangeReputation<TFaction>(amount, actionSource);

        /// <summary>
        ///     Returns the currently active <see cref="ReputationLevelBase"/> for
        ///     <typeparamref name="TFaction"/>, or <c>null</c> if no level is assigned.
        /// </summary>
        [CanBeNull]
        public static ReputationLevelBase GetLevel<TFaction, THolder>(
            [NotNull] FactionMembershipBase<THolder> membership)
            where TFaction : FactionBase<THolder>, new()
            where THolder : class
            => membership.GetCurrentLevel<TFaction>();

        /// <summary>
        ///     Returns <c>true</c> if the object's current reputation level for
        ///     <typeparamref name="TFaction"/> is equal to or higher than <paramref name="level"/>
        ///     (determined by index position in the faction's level list).
        /// </summary>
        public static bool IsAtLeastLevel<TFaction, THolder>(
            [NotNull] FactionMembershipBase<THolder> membership,
            [NotNull] ReputationLevelBase level)
            where TFaction : FactionBase<THolder>, new()
            where THolder : class
            => membership.IsAtLeastLevel<TFaction>(level);

        /// <summary>
        ///     Manually assigns <paramref name="level"/> as the active reputation level for
        ///     <typeparamref name="TFaction"/>. Pass <c>null</c> to clear the current level.
        ///     This bypasses automatic promotion/demotion checks and is intended for unconditional
        ///     overrides such as a king granting knighthood.
        /// </summary>
        /// <returns>
        ///     <see cref="Operations.FactionOperations.LevelAssigned()"/> or
        ///     <see cref="Operations.FactionOperations.LevelCleared()"/> on success,
        ///     or an error result describing the reason for failure.
        /// </returns>
        public static OperationResult AssignLevel<TFaction, THolder>(
            [NotNull] FactionMembershipBase<THolder> membership,
            [CanBeNull] ReputationLevelBase level,
            ActionSource actionSource = ActionSource.External)
            where TFaction : FactionBase<THolder>, new()
            where THolder : class
            => membership.AssignLevel<TFaction>(level, actionSource);
    }
}
