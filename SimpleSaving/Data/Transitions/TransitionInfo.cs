using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Systems.SimpleSaving.Data.Transitions
{
    /// <summary>Result of a transition path search.</summary>
    public readonly struct TransitionInfo
    {
        public Type From { get; }
        public Type To { get; }
        public bool IsPossible { get; }
        [NotNull] public IReadOnlyList<SaveFileTransitionStep> Steps { get; }
        public TransitionInfo(Type from, Type to, bool isPossible, [CanBeNull] IReadOnlyList<SaveFileTransitionStep> steps)
        {
            From = from;
            To = to;
            IsPossible = isPossible;
            Steps = steps ?? Array.Empty<SaveFileTransitionStep>();
        }
        [NotNull] public override string ToString()
        {
            if (!IsPossible) return $"No transition path from {From?.Name} to {To?.Name}.";
            if (Steps.Count == 0) return $"No-op transition (same type): {From?.Name}.";
            return $"Transition {From?.Name} -> {To?.Name}: {string.Join(" | ", Steps.Select(step => step.ToString()))}";
        }
    }
}
