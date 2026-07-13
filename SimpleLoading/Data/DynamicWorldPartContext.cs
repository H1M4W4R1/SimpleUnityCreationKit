using JetBrains.Annotations;
using Systems.SimpleLoading.Components;
using UnityEngine;

namespace Systems.SimpleLoading.Data
{
    /// <summary>Context passed to dynamic-world-part lifecycle callbacks.</summary>
    public readonly ref struct DynamicWorldPartContext
    {
        public readonly DynamicWorldPart worldPart;
        [CanBeNull] public readonly Transform target;

        internal DynamicWorldPartContext(DynamicWorldPart worldPart, [CanBeNull] Transform target)
        {
            this.worldPart = worldPart;
            this.target = target;
        }
    }
}
