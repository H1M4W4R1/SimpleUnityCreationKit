using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCrafting.Abstract;
using Systems.SimpleCrafting.Data.Enums;
using UnityEngine;

namespace Systems.SimpleCrafting.Data.Runtime
{
    public sealed class CraftingInstance
    {
        [NotNull] public CraftingRecipeBase Recipe { get; }
        [CanBeNull] public CraftingStationBase Station { get; }
        [CanBeNull] public IReadOnlyList<CraftingStationBase> Stations { get; }
        [CanBeNull] public ICraftingUser User { get; }
        public float DurationSeconds { get; }
        public float ElapsedSeconds { get; private set; }
        public CraftingInstanceState State { get; private set; }
        public bool HasConsumedIngredients { get; private set; }
        public float RemainingSeconds => Mathf.Max(0f, DurationSeconds - ElapsedSeconds);
        public bool IsReadyToComplete => State == CraftingInstanceState.InProgress && RemainingSeconds <= 0f;

        internal CraftingInstance(
            [NotNull] CraftingRecipeBase recipe,
            [CanBeNull] CraftingStationBase station,
            [CanBeNull] IReadOnlyList<CraftingStationBase> stations,
            [CanBeNull] ICraftingUser user,
            float durationSeconds,
            bool hasConsumedIngredients)
        {
            Recipe = recipe;
            Station = station;
            Stations = stations;
            User = user;
            DurationSeconds = Mathf.Max(0f, durationSeconds);
            ElapsedSeconds = 0f;
            State = CraftingInstanceState.InProgress;
            HasConsumedIngredients = hasConsumedIngredients;
        }

        internal void Advance(float deltaTime)
        {
            ElapsedSeconds = Mathf.Min(DurationSeconds, ElapsedSeconds + deltaTime);
        }

        internal void MarkCompleted()
        {
            State = CraftingInstanceState.Completed;
            HasConsumedIngredients = false;
        }

        internal void MarkCancelled()
        {
            State = CraftingInstanceState.Cancelled;
            HasConsumedIngredients = false;
        }

        internal void MarkFailed()
        {
            State = CraftingInstanceState.Failed;
            HasConsumedIngredients = false;
        }
    }
}
