using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleEntities.Data;
using Systems.SimpleEntities.Data.Affinity;
using Systems.SimpleEntities.Data.Context;
using Systems.SimpleEntities.Data.Enums;
using Systems.SimpleEntities.Data.Resistances;
using Systems.SimpleEntities.Data.Status.Abstract;
using Systems.SimpleEntities.Data.Status.Storage;
using Systems.SimpleEntities.Operations;
using Systems.SimpleStats.Abstract;
using Systems.SimpleStats.Abstract.Modifiers;
using Systems.SimpleStats.Data;
using Systems.SimpleStats.Data.Collections;
using Systems.SimpleStats.Operations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleEntities.Components
{
    /// <summary>
    ///     Represents in-game entity that may have stats, inventory, etc.
    ///     Generally used to represent player, enemy, etc., but not objects such as chest.
    /// </summary>
    /// <remarks>
    ///     Intended to be used for objects that have health, statistics etc.
    /// </remarks>
    public abstract class AliveEntityBase : TickingEntityBase, IWithStatModifiers
    {
#region Entity Lifecycle

        protected override void AssignComponents()
        {
            // Refresh entity modifiers for first time
            RefreshModifiersIfNecessary();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Reset health to max
            ResetHealthToMax();
        }

        protected override void OnTick(float deltaTime)
        {
            HandleTimedModifiers(deltaTime);
            HandleStatusTick(deltaTime);
        }

        private void HandleTimedModifiers(float deltaTime)
        {
            bool hasExpired = false;

            // Update all non-expired timed modifiers
            for (int i = 0; i < statModifiers.Count; i++)
            {
                if (statModifiers[i] is not ITimedModifier {IsExpired: false} timed) continue;
                timed.UpdateTime(deltaTime);
                if (timed.IsExpired)
                    hasExpired = true;
            }

            if (!hasExpired) return;

            // Remove expired modifiers (iterate backwards for safe removal)
            IWithStatModifiers self = this;
            for (int i = statModifiers.Count - 1; i >= 0; i--)
            {
                if (statModifiers[i] is not ITimedModifier {IsExpired: true}) continue;

                IStatModifier modifier = statModifiers[i];
                statModifiers.RemoveAt(i);

                ModifierContext context = new(modifier, self);
                OperationResult expiredResult = ModifierOperations.ModifierRemoved();
                self.OnModifierExpired(in context, in expiredResult);
            }
        }


        /// <summary>
        ///     Resets health to max
        /// </summary>
        /// <remarks>
        ///     Can be overriden to perform additional computation e.g. using statistic modifiers.
        ///     Does not trigger heal events, should be used only for initial reset.
        /// </remarks>
        protected virtual void ResetHealthToMax() => CurrentHealth = MaxHealth;

#endregion

#region Damage and healing

        /// <summary>
        ///     Current health of the entity
        /// </summary>
        [field: SerializeField] public virtual long CurrentHealth { get; protected set; }

        /// <summary>
        ///     Maximum health of the entity. Must be at least 1.
        /// </summary>
        [field: SerializeField] public virtual long MaxHealth { get; protected set; }

        /// <summary>
        ///     Whether the entity is dead (health reached zero)
        /// </summary>
        public bool IsDead => CurrentHealth <= 0;

        protected virtual void OnValidate()
        {
            if (MaxHealth < 1) MaxHealth = 1;
        }

        // Reusable scratch collections to avoid per-call GC allocations
        private readonly List<ResistanceBase> _scratchResistances = new();
        private readonly List<IStatModifier> _scratchModifierList = new();
        private readonly StatModifierCollection _scratchModifierCollection = new();

        /// <summary>
        ///     Gets resistance of the entity
        /// </summary>
        /// <typeparam name="TAffinityType">Type of the affinity</typeparam>
        /// <returns>Value of the resistance</returns>
        public float GetResistance<TAffinityType>()
            where TAffinityType : AffinityType
        {
            float result = 0;

            // Get all resistances from entity's own modifiers
            IWithStatModifiers statsAccess = this;
            IReadOnlyList<IStatModifier> allModifiers = statsAccess.GetAllModifiers();

            // Collect unique resistance statistics that this entity actually has modifiers for
            _scratchResistances.Clear();
            for (int modifierIndex = 0; modifierIndex < allModifiers.Count; modifierIndex++)
            {
                ResistanceBase resistance = allModifiers[modifierIndex].GetStatisticAs<ResistanceBase>();
                if (ReferenceEquals(resistance, null)) continue;
                if (!resistance.IsValidFor<TAffinityType>()) continue;
                if (_scratchResistances.Contains(resistance)) continue;
                _scratchResistances.Add(resistance);
            }

            for (int resistanceIndex = 0; resistanceIndex < _scratchResistances.Count; resistanceIndex++)
            {
                ResistanceBase resistance = _scratchResistances[resistanceIndex];
                _scratchModifierCollection.Clear();
                statsAccess.GetAllModifiersFor(resistance, _scratchModifierList);
                _scratchModifierCollection.AddRange(_scratchModifierList);
                _scratchModifierList.Clear();

                float finalValue = resistance.GetFinalValue(_scratchModifierCollection);
                result += finalValue;
            }

            return result;
        }

        /// <summary>
        ///     Heals the entity
        /// </summary>
        /// <param name="context">Context of the healing event</param>
        /// <returns>Result of the operation with amount of healed health</returns>
        public OperationResult Heal(
            in HealContext context)
        {
            // Check if entity can be healed
            OperationResult canHealResult = CanBeHealed(context);
            if (!canHealResult)
            {
                OnHealFailed(context, canHealResult);
                return canHealResult;
            }

            // Compute amount of health to change
            // and update context with final amount
            long missingHealth = MaxHealth - CurrentHealth;
            long healthToChange = math.min(missingHealth, context.amount);

            // Add health and execute heal handlers
            CurrentHealth += healthToChange;

            OperationResult opResult = EntityOperations.Healed();
            OnHealReceived(context, opResult, healthToChange);
            return opResult;
        }

        /// <summary>
        ///     Deals damage to the entity
        /// </summary>
        /// <param name="source">Source of the damage</param>
        /// <param name="amount">Base amount of damage</param>
        /// <typeparam name="TDamageAffinity">Affinity of the damage</typeparam>
        public OperationResult Damage<TDamageAffinity>(
            [CanBeNull] object source,
            long amount)
            where TDamageAffinity : AffinityType, new()
        {
            DamageContext context = DamageContext.Create<TDamageAffinity>(this, source, amount);
            return Damage(context);
        }

        /// <summary>
        ///     Heals the entity
        /// </summary>
        /// <param name="source">Source of the healing</param>
        /// <param name="amount">Base amount of healing</param>
        /// <typeparam name="THealingAffinity">Affinity of the healing</typeparam>
        public OperationResult Heal<THealingAffinity>(
            [CanBeNull] object source,
            long amount)
            where THealingAffinity : AffinityType, new()
        {
            HealContext context = HealContext.Create<THealingAffinity>(this, source, amount);
            return Heal(context);
        }

        /// <summary>
        ///     Deals damage to the entity
        /// </summary>
        /// <param name="context">Context of the damage event</param>
        public OperationResult Damage(
            in DamageContext context)
        {
            // Check if entity can be damaged
            OperationResult canBeDamagedResult = CanBeDamaged(context);
            if (!canBeDamagedResult)
            {
                OnDamageFailed(context, canBeDamagedResult);
                return canBeDamagedResult;
            }

            // Compute amount of health to change
            long healthToChange = math.min(context.amount, CurrentHealth);

            // Subtract health and execute damage handlers
            CurrentHealth -= healthToChange;

            OperationResult opResult = EntityOperations.Damaged();
            OnDamageReceived(context, opResult, healthToChange);

            // If health is zero or less, kill the entity
            if (CurrentHealth > 0) return opResult;
            return Kill(context, healthBeforeDeath: healthToChange);
        }

        /// <summary>
        ///     Kills the entity
        /// </summary>
        /// <returns>Result of the operation with amount of health after "death"</returns>
        /// <remarks>
        ///     Entity may be saved from death, so this method will return <see cref="EntityOperations.SavedFromDeath"/>
        ///     with a success status code, even if entity is still alive.
        /// </remarks>
        public OperationResult Kill(
            in DamageContext context,
            long healthBeforeDeath = -1)
        {
            // Capture health before zeroing so OnDeath receives the correct value
            if (healthBeforeDeath < 0) healthBeforeDeath = CurrentHealth;

            // Reset health to zero
            CurrentHealth = 0;

            // Check death save
            DeathSaveContext deathSaveContext = CanSaveFromDeath(context);

            // If entity should be saved, set health to the value specified in death save context
            if (deathSaveContext.shouldBeSaved)
            {
                CurrentHealth = deathSaveContext.healthToSet;

                OperationResult deathSaveData =
                    EntityOperations.SavedFromDeath();

                OnSavedFromDeath(context, deathSaveContext, deathSaveData, CurrentHealth);
                return deathSaveData;
            }

            // Perform death events
            OnDeath(context, EntityOperations.Killed(), healthBeforeDeath);
            return EntityOperations.Killed();
        }


        /// <summary>
        ///     Checks if entity can be damaged
        /// </summary>
        protected virtual OperationResult CanBeDamaged(in DamageContext context)
        {
            if (IsDead) return EntityOperations.NotPermitted();
            if (!ReferenceEquals(context.affinityType, null)) return context.affinityType.CanBeDamaged(context);
            return EntityOperations.Permitted();
        }

        /// <summary>
        ///     Checks if entity can be healed
        /// </summary>
        protected virtual OperationResult CanBeHealed(in HealContext context)
        {
            if (IsDead) return EntityOperations.NotPermitted();
            if (!ReferenceEquals(context.affinityType, null)) return context.affinityType.CanBeHealed(context);
            return EntityOperations.Permitted();
        }

        /// <summary>
        ///     Called when damage is failed due to <see cref="CanBeDamaged"/>
        /// </summary>
        protected virtual void OnDamageFailed(
            in DamageContext context,
            in OperationResult result)
        {
            if (!ReferenceEquals(context.affinityType, null)) context.affinityType.OnDamageFailed(context, result);
        }

        /// <summary>
        ///     Executes when entity takes damage
        /// </summary>
        protected virtual void OnDamageReceived(
            in DamageContext context,
            in OperationResult result,
            long healthLost)
        {
            if (!ReferenceEquals(context.affinityType, null))
                context.affinityType.OnDamageReceived(context, result, healthLost);
        }


        /// <summary>
        ///     Called when healing is failed due to <see cref="CanBeHealed"/>
        /// </summary>
        protected virtual void OnHealFailed(in HealContext context, in OperationResult result)
        {
            if (!ReferenceEquals(context.affinityType, null))
                context.affinityType.OnHealingFailed(context, result);
        }

        /// <summary>
        ///     Executes when entity takes healing
        /// </summary>
        protected virtual void OnHealReceived(in HealContext context, in OperationResult result, long healthAdded)
        {
            if (!ReferenceEquals(context.affinityType, null))
                context.affinityType.OnHealingReceived(context, result, healthAdded);
        }

        /// <summary>
        ///     Checks if entity should be protected from death
        /// </summary>
        protected virtual DeathSaveContext CanSaveFromDeath(in DamageContext context)
        {
            if (!ReferenceEquals(context.affinityType, null))
                return context.affinityType.CanSaveFromDeath(context);
            return new DeathSaveContext(false, 0);
        }

        /// <summary>
        ///     Called when entity is saved from death
        /// </summary>
        protected virtual void OnSavedFromDeath(
            in DamageContext damageContext,
            in DeathSaveContext context,
            in OperationResult result,
            long healthSet)
        {
            if (!ReferenceEquals(damageContext.affinityType, null))
                damageContext.affinityType.OnSavedFromDeath(damageContext, context, result, healthSet);
        }

        /// <summary>
        ///     Executes when entity dies
        /// </summary>
        protected virtual void OnDeath(in DamageContext context, in OperationResult result, long healthLost)
        {
            if (!ReferenceEquals(context.affinityType, null))
                context.affinityType.OnDeath(context, result, healthLost);
        }

#endregion

#region Statistics and modifiers

        /// <summary>
        ///     Modifiers registered for this object
        /// </summary>
        protected readonly List<IStatModifier> statModifiers = new();

        /// <summary>
        ///     Gets all modifiers registered for this object
        /// </summary>
        /// <returns>Read-only list of modifiers</returns>
        public IReadOnlyList<IStatModifier> GetAllModifiers() => statModifiers;

        /// <summary>
        ///     Refresh statistic modifiers if necessary
        /// </summary>
        /// <remarks>
        ///     Intended to refresh modifiers when e.g. equipment changes.
        ///     Entity should cache all modifiers related to statistics it has including things
        ///     such as equipment, etc.
        /// </remarks>
        public virtual void RefreshModifiersIfNecessary()
        {
            // Do nothing by default
        }

#endregion

#region Status effects

        /// <summary>
        ///     Container for all applied status effects
        /// </summary>
        private readonly List<AppliedStatusData> _appliedStatuses = new();

        /// <summary>
        ///     Acquires all applied status effects
        /// </summary>
        /// <returns>Read-only list of applied status effects</returns>
        public IReadOnlyList<AppliedStatusData> GetAllAppliedStatuses() => _appliedStatuses;

        /// <summary>
        ///     Applies a status to the entity
        /// </summary>
        /// <param name="stackCount">Stack count to apply</param>
        /// <param name="flags">Flags to modify the application</param>
        /// <typeparam name="TStatusType">Type of the status to apply</typeparam>
        /// <returns>Result of the application with new stack count</returns>
        public OperationResult ApplyStatus<TStatusType>(
            int stackCount = 1,
            StatusModificationFlags flags = StatusModificationFlags.None)
            where TStatusType : StatusBase, new()
        {
            TStatusType status = StatusDatabase.GetExact<TStatusType>();
            Assert.IsFalse(ReferenceEquals(status, null), "Status not found in database");
            return ApplyStatus(status, stackCount, flags);
        }

        /// <summary>
        ///     Applies a status to the entity
        /// </summary>
        /// <param name="status">Status to apply</param>
        /// <param name="stackCount">Stack count to apply</param>
        /// <param name="flags">Flags to modify the application</param>
        /// <returns>Result of the application with new stack count</returns>
        public OperationResult ApplyStatus(
            [NotNull] StatusBase status,
            int stackCount = 1,
            StatusModificationFlags flags = StatusModificationFlags.None)
        {
            if (stackCount <= 0) return StatusOperations.InvalidStackCount();

            // Create status context
            StatusContext checkContext = new(this, status, stackCount);

            // Find status if already applied
            AppliedStatusData statusReference = default;
            int statusReferenceIndex = -1;
            for (int i = 0; i < _appliedStatuses.Count; i++)
            {
                if (_appliedStatuses[i].status != status) continue;
                statusReference = _appliedStatuses[i];
                statusReferenceIndex = i;
                break;
            }

            // Check if status can be applied to the entity
            OperationResult canApplyStatus = CanApplyStatus(checkContext);
            if (!canApplyStatus && (flags & StatusModificationFlags.IgnoreConditions) == 0)
            {
                OnStatusApplicationFailed(checkContext, canApplyStatus);
                return canApplyStatus;
            }

            bool respectStackLimit =
                status.MaxStack > 0 && (flags & StatusModificationFlags.IgnoreStackLimit) == 0;

            // If status is not applied, apply it
            if (ReferenceEquals(statusReference.status, null))
            {
                stackCount = respectStackLimit ? math.min(stackCount, status.MaxStack) : stackCount;

                StatusContext addStatusContext = new(this, status, stackCount);
                statusReference = new AppliedStatusData(status, stackCount);
                _appliedStatuses.Add(statusReference);
                OperationResult opResult = StatusOperations.StatusApplied();
                OnStatusApplied(addStatusContext, opResult, statusReference.stackCount);
                return opResult;
            }

            // If status is already applied, check if it can be stacked (or if max stack is reached)
            if (respectStackLimit && statusReference.stackCount >= status.MaxStack)
            {
                OperationResult opResult = StatusOperations.MaxStackReached();
                OnStatusApplicationFailed(checkContext, StatusOperations.MaxStackReached());
                return opResult;
            }

            // If status can be stacked, stack it
            // StatusContext requires amount changed to be present rather than new value
            int stackChange = respectStackLimit ? math.min(stackCount, status.MaxStack - statusReference.stackCount) : stackCount;
            StatusContext modifyStatusContext = new(this, status, stackChange);
            statusReference.stackCount += stackChange;
            _appliedStatuses[statusReferenceIndex] = statusReference;

            // Create operation result
            OperationResult opResult1 = StatusOperations.StatusStackChanged();

            // Call event
            OnStatusStackChanged(modifyStatusContext, opResult1, statusReference.stackCount);
            return opResult1;
        }

        /// <summary>
        ///     Removes a status from the entity
        /// </summary>
        /// <param name="stackCount">Stack count to remove</param>
        /// <param name="flags">Flags to modify the removal</param>
        /// <typeparam name="TStatusType">Type of the status to remove</typeparam>
        /// <returns>Result of the removal with new stack count</returns>
        public OperationResult RemoveStatus<TStatusType>(
            int stackCount = 1,
            StatusModificationFlags flags = StatusModificationFlags.None)
            where TStatusType : StatusBase, new()
        {
            TStatusType status = StatusDatabase.GetExact<TStatusType>();
            Assert.IsFalse(ReferenceEquals(status, null), "Status not found in database");
            return RemoveStatus(status, stackCount, flags);
        }

        /// <summary>
        ///     Removes a status from the entity
        /// </summary>
        /// <param name="status">Status to remove</param>
        /// <param name="stackCount">Stack count to remove</param>
        /// <param name="flags">Flags to modify the removal</param>
        /// <returns>Result of the removal</returns>
        public OperationResult RemoveStatus(
            [NotNull] StatusBase status,
            int stackCount = 1,
            StatusModificationFlags flags = StatusModificationFlags.None)
        {
            if (stackCount <= 0) return StatusOperations.InvalidStackCount();

            // Get status removal context
            StatusContext checkContext = new(this, status, stackCount);

            // Find status if already applied
            AppliedStatusData statusReference = default;
            int statusReferenceIndex = -1;
            for (int i = 0; i < _appliedStatuses.Count; i++)
            {
                if (_appliedStatuses[i].status != status) continue;

                statusReference = _appliedStatuses[i];
                statusReferenceIndex = i;
                break;
            }

            // If status is not applied, return invalid status
            if (ReferenceEquals(statusReference.status, null))
            {
                OperationResult opResult = StatusOperations.NotApplied();
                OnStatusRemovalFailed(checkContext, StatusOperations.NotApplied());
                return opResult;
            }

            // Check if status can be removed
            OperationResult canRemoveStatus = status.CanRemove(checkContext);
            if (!canRemoveStatus && (flags & StatusModificationFlags.IgnoreConditions) == 0)
            {
                OnStatusRemovalFailed(checkContext, canRemoveStatus);
                return canRemoveStatus;
            }

            // If status is applied, check if it can be removed.
            // When IgnoreStackLimit is set, this check is skipped and the removal is silently
            // clamped to the available stack count (it does not allow negative stacks).
            if (statusReference.stackCount - stackCount < 0 &&
                (flags & StatusModificationFlags.IgnoreStackLimit) == 0)
            {
                OperationResult opResult =
                    StatusOperations.NotEnoughStacks();
                OnStatusRemovalFailed(checkContext, StatusOperations.NotEnoughStacks());
                return opResult;
            }

            // Remove stacks or clear status to zero if stack are overflown
            int stackChange = math.min(stackCount, statusReference.stackCount);
            statusReference.stackCount -= stackChange;

            // If status is now empty, remove it from the list
            if (statusReference.stackCount == 0 && statusReferenceIndex != -1)
            {
                StatusContext removeStatusContext = new(this, status, 0);

                // Remove status from list
                _appliedStatuses.RemoveAt(statusReferenceIndex);

                OperationResult opResult = StatusOperations.StatusRemoved();
                OnStatusRemoved(removeStatusContext, opResult);
                return opResult;
            }
            else // If not, then handle stack reduction
            {
                StatusContext reduceStackContext = new(this, status, -stackChange);

                // Update applied statuses
                _appliedStatuses[statusReferenceIndex] = statusReference;

                OperationResult opResult =
                    StatusOperations.StatusStackChanged();
                OnStatusStackChanged(reduceStackContext, opResult, statusReference.stackCount);
                return opResult;
            }
        }

        /// <summary>
        ///     Checks if the entity has a status
        /// </summary>
        /// <typeparam name="TStatusType">Type of the status to check for</typeparam>
        /// <returns>True if the entity has the status, false otherwise</returns>
        public bool HasStatus<TStatusType>()
            where TStatusType : StatusBase, new()
        {
            TStatusType status = StatusDatabase.GetExact<TStatusType>();
            if (ReferenceEquals(status, null)) return false;
            return HasStatus(status);
        }

        /// <summary>
        ///     Checks if the entity has a status
        /// </summary>
        /// <param name="status">Status to check for</param>
        /// <returns>True if the entity has the status, false otherwise</returns>
        public bool HasStatus([NotNull] StatusBase status) =>
            GetStatusStackCount(status) > 0;

        /// <summary>
        ///     Gets the stack count of a status
        /// </summary>
        /// <typeparam name="TStatusType">Type of the status to get the stack count for</typeparam>
        /// <returns>Stack count of the status</returns>
        public int GetStatusStackCount<TStatusType>()
            where TStatusType : StatusBase, new()
        {
            TStatusType status = StatusDatabase.GetExact<TStatusType>();
            if (ReferenceEquals(status, null)) return 0;
            return GetStatusStackCount(status);
        }

        /// <summary>
        ///     Gets the stack count of a status
        /// </summary>
        /// <param name="status">Status to check for</param>
        /// <returns>Stack count of the status</returns>
        public int GetStatusStackCount([NotNull] StatusBase status)
        {
            for (int i = 0; i < _appliedStatuses.Count; i++)
            {
                if (_appliedStatuses[i].status != status) continue;

                return _appliedStatuses[i].stackCount;
            }

            return 0;
        }

        /// <summary>
        ///     Performs per-tick status handling
        /// </summary>
        protected void HandleStatusTick(float deltaTime)
        {
            // Iterate in reverse so that removals during tick don't skip entries or cause IndexOutOfRange
            for (int i = _appliedStatuses.Count - 1; i >= 0; i--)
            {
                StatusContext tickContext = new(this, _appliedStatuses[i].status, _appliedStatuses[i].stackCount);
                _appliedStatuses[i].status.OnStatusTick(tickContext, deltaTime);
            }
        }

        /// <summary>
        ///     Check if status can be applied to the entity
        /// </summary>
        protected virtual OperationResult CanApplyStatus(in StatusContext context) =>
            context.status.CanApply(context);

        /// <summary>
        ///     Checks if status can be removed from the entity
        /// </summary>
        protected virtual OperationResult CanRemoveStatus(in StatusContext context) =>
            context.status.CanRemove(context);

        /// <summary>
        ///     Executes when status is applied to the entity
        /// </summary>
        protected virtual void OnStatusApplied(
            in StatusContext context,
            in OperationResult result,
            int currentStackCount) =>
            context.status.OnStatusApplied(context, result, currentStackCount);

        /// <summary>
        ///     Executes when status application fails
        /// </summary>
        protected virtual void OnStatusApplicationFailed(
            in StatusContext context,
            in OperationResult result) =>
            context.status.OnStatusApplicationFailed(context, result);

        /// <summary>
        ///     Executes when status is removed from the entity
        /// </summary>
        protected virtual void OnStatusRemoved(
            in StatusContext context,
            in OperationResult result) =>
            context.status.OnStatusRemoved(context, result);

        /// <summary>
        ///     Executes when status removal fails
        /// </summary>
        protected virtual void OnStatusRemovalFailed(
            in StatusContext context,
            in OperationResult result) =>
            context.status.OnStatusRemovalFailed(context, result);

        /// <summary>
        ///     Executes when status stack count changes
        /// </summary>
        protected virtual void OnStatusStackChanged(
            in StatusContext context,
            in OperationResult result,
            int currentStackCount) =>
            context.status.OnStatusStackChanged(context, result, currentStackCount);

#endregion
    }
}
