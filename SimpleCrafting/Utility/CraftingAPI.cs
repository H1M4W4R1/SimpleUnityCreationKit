using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleCrafting.Abstract;
using Systems.SimpleCrafting.Components;
using Systems.SimpleCrafting.Data.Context;
using Systems.SimpleCrafting.Data.Enums;
using Systems.SimpleCrafting.Data.Runtime;
using Systems.SimpleCrafting.Operations;
using UnityEngine;

namespace Systems.SimpleCrafting.Utility
{
    /// <summary>
    ///     Orchestrates the common crafting transaction while recipes own their domain-specific behavior.
    /// </summary>
    public static class CraftingAPI
    {
        private static readonly List<CraftingInstance> _activeInstances = new List<CraftingInstance>();
        public static IReadOnlyList<CraftingInstance> ActiveInstances => _activeInstances;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _activeInstances.Clear();
        }

        public static OperationResult CanCraft(
            [CanBeNull] CraftingRecipeBase recipe,
            [CanBeNull] CraftingStationBase station = null,
            [CanBeNull] ICraftingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            CraftingContext context = new CraftingContext(recipe, station, user, actionSource);
            return CanCraft(in context);
        }

        public static OperationResult CanCraft(
            [CanBeNull] CraftingRecipeBase recipe,
            [CanBeNull] IReadOnlyList<CraftingStationBase> stations,
            [CanBeNull] ICraftingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            CraftingContext context = new CraftingContext(recipe, stations, user, actionSource);
            return CanCraft(in context);
        }

        public static OperationResult CanCraft(in CraftingContext context)
        {
            if (ReferenceEquals(context.recipe, null)) return CraftingOperations.RecipeIsNull();
            if (!context.recipe) return CraftingOperations.RecipeIsNull();

            CraftingRecipeBase recipe = context.recipe;
            OperationResult result = recipe.CanCraft(in context);
            if (!result) return result;

            result = ValidateStations(in context);
            if (!result) return result;

            result = recipe.CanConsumeCraftingIngredients(in context);
            if (!result) return result;

            return recipe.CanGrantCraftingResult(in context);
        }

        public static OperationResult TryStartCrafting(
            [CanBeNull] CraftingRecipeBase recipe,
            [CanBeNull] out CraftingInstance instance,
            [CanBeNull] CraftingStationBase station = null,
            [CanBeNull] ICraftingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            CraftingContext context = new CraftingContext(recipe, station, user, actionSource);
            return TryStartCrafting(in context, out instance);
        }

        public static OperationResult TryStartCrafting(
            [CanBeNull] CraftingRecipeBase recipe,
            [CanBeNull] out CraftingInstance instance,
            [CanBeNull] IReadOnlyList<CraftingStationBase> stations,
            [CanBeNull] ICraftingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            CraftingContext context = new CraftingContext(recipe, stations, user, actionSource);
            return TryStartCrafting(in context, out instance);
        }

        public static OperationResult TryStartCrafting(
            in CraftingContext context,
            [CanBeNull] out CraftingInstance instance)
        {
            instance = null;
            OperationResult result = CanCraft(in context);
            if (!result)
            {
                NotifyStartFailed(in context, result);
                return result;
            }

            result = context.recipe!.TryConsumeCraftingIngredients(in context);
            if (!result)
            {
                NotifyStartFailed(in context, result);
                return result;
            }

            float durationSeconds = GetCraftingDurationSeconds(context.recipe);
            if (durationSeconds <= 0f)
            {
                result = context.recipe.TryGrantCraftingResult(in context);
                if (!result)
                {
                    OperationResult refundResult = context.recipe.TryRefundCraftingIngredients(in context);
                    OperationResult finalResult = refundResult ? result : CraftingOperations.RefundFailed();
                    context.recipe.OnCraftingCompletionFailed(in context, finalResult);
                    return finalResult;
                }

                context.recipe.OnCraftingCompleted(in context, result);
                return result;
            }

            instance = new CraftingInstance(
                context.recipe,
                context.station,
                context.stations,
                context.user,
                durationSeconds,
                true);

            _activeInstances.Add(instance);
            context.recipe.OnCraftingStarted(instance);
            return CraftingOperations.Started();
        }

        /// <summary>
        ///     Advances and completes every timed craft started through this API.
        ///     <see cref="CraftingTickSystem"/> calls this from the global tick.
        /// </summary>
        public static OperationResult AdvanceAllCrafting(float deltaTime)
        {
            if (deltaTime < 0f) return CraftingOperations.InvalidDeltaTime();

            OperationResult lastResult = CraftingOperations.Permitted();
            for (int i = _activeInstances.Count - 1; i >= 0; i--)
            {
                CraftingInstance instance = _activeInstances[i];
                if (ReferenceEquals(instance, null))
                {
                    _activeInstances.RemoveAt(i);
                    continue;
                }

                OperationResult advanceResult = AdvanceCrafting(instance, deltaTime);
                if (!advanceResult && instance.State == CraftingInstanceState.InProgress)
                {
                    lastResult = advanceResult;
                    continue;
                }

                lastResult = advanceResult;

                if (instance.IsReadyToComplete)
                {
                    OperationResult completionResult = TryCompleteCrafting(instance);
                    lastResult = completionResult;
                }
            }

            return lastResult;
        }

        public static OperationResult AdvanceCrafting([CanBeNull] CraftingInstance instance, float deltaTime)
        {
            if (ReferenceEquals(instance, null)) return CraftingOperations.InstanceIsNull();
            if (deltaTime < 0f) return CraftingOperations.InvalidDeltaTime();
            if (instance.State == CraftingInstanceState.Cancelled) return CraftingOperations.InstanceCancelled();
            if (instance.State != CraftingInstanceState.InProgress)
                return CraftingOperations.InstanceAlreadyFinished();

            instance.Advance(deltaTime);
            if (instance.IsReadyToComplete) return CraftingOperations.Ready();
            return CraftingOperations.ProgressUpdated();
        }

        public static OperationResult TryCompleteCrafting([CanBeNull] CraftingInstance instance)
        {
            if (ReferenceEquals(instance, null)) return CraftingOperations.InstanceIsNull();
            if (instance.State == CraftingInstanceState.Cancelled) return CraftingOperations.InstanceCancelled();
            if (instance.State != CraftingInstanceState.InProgress)
                return CraftingOperations.InstanceAlreadyFinished();
            if (!instance.IsReadyToComplete) return CraftingOperations.InstanceNotReady();

            CraftingContext context = CreateContext(instance);
            OperationResult result = instance.Recipe.CanGrantCraftingResult(in context);
            if (!result) return FailCompletion(instance, in context, result);

            result = instance.Recipe.TryGrantCraftingResult(in context);
            if (!result) return FailCompletion(instance, in context, result);

            instance.MarkCompleted();
            _activeInstances.Remove(instance);
            instance.Recipe.OnCraftingCompleted(in context, result);
            return result;
        }

        public static OperationResult TryCancelCrafting([CanBeNull] CraftingInstance instance)
        {
            if (ReferenceEquals(instance, null)) return CraftingOperations.InstanceIsNull();
            if (instance.State == CraftingInstanceState.Cancelled) return CraftingOperations.InstanceCancelled();
            if (instance.State != CraftingInstanceState.InProgress)
                return CraftingOperations.InstanceAlreadyFinished();

            CraftingContext context = CreateContext(instance);
            OperationResult result = instance.HasConsumedIngredients
                ? instance.Recipe.TryRefundCraftingIngredients(in context)
                : CraftingOperations.Permitted();

            if (!result)
            {
                instance.MarkFailed();
                _activeInstances.Remove(instance);
                instance.Recipe.OnCraftingCompletionFailed(in context, result);
                return result;
            }

            instance.MarkCancelled();
            _activeInstances.Remove(instance);
            OperationResult cancelled = CraftingOperations.Cancelled();
            instance.Recipe.OnCraftingCancelled(instance, cancelled);
            return cancelled;
        }

        private static OperationResult ValidateStations(in CraftingContext context)
        {
            if (!ReferenceEquals(context.station, null))
            {
                if (!context.station) return CraftingOperations.StationMissing();
                return context.station.CanUseStation(in context);
            }

            if (ReferenceEquals(context.stations, null)) return CraftingOperations.Permitted();

            for (int i = 0; i < context.stations.Count; i++)
            {
                CraftingStationBase station = context.stations[i];
                if (ReferenceEquals(station, null) || !station) return CraftingOperations.StationMissing();

                OperationResult result = station.CanUseStation(in context);
                if (!result) return result;
            }

            return CraftingOperations.Permitted();
        }

        private static float GetCraftingDurationSeconds([NotNull] CraftingRecipeBase recipe)
        {
            if (recipe is not ITimedCrafting timedCrafting) return 0f;
            return Mathf.Max(0f, timedCrafting.DurationSeconds);
        }

        private static OperationResult FailCompletion(
            [NotNull] CraftingInstance instance,
            in CraftingContext context,
            in OperationResult failure)
        {
            OperationResult refundResult = instance.HasConsumedIngredients
                ? instance.Recipe.TryRefundCraftingIngredients(in context)
                : CraftingOperations.Permitted();
            OperationResult finalResult = refundResult ? failure : CraftingOperations.RefundFailed();
            instance.MarkFailed();
            _activeInstances.Remove(instance);
            instance.Recipe.OnCraftingCompletionFailed(in context, finalResult);
            return finalResult;
        }

        private static void NotifyStartFailed(in CraftingContext context, in OperationResult result)
        {
            if (ReferenceEquals(context.recipe, null)) return;
            if (!context.recipe) return;
            context.recipe.OnCraftingStartFailed(in context, result);
        }

        private static CraftingContext CreateContext([NotNull] CraftingInstance instance)
        {
            if (!ReferenceEquals(instance.Stations, null))
            {
                return new CraftingContext(
                    instance.Recipe,
                    instance.Stations,
                    instance.User);
            }

            return new CraftingContext(
                instance.Recipe,
                instance.Station,
                instance.User);
        }
    }
}
