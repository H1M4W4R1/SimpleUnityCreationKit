using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using UnityEngine;

namespace Systems.SimpleBuilding.Examples
{
    /// <summary>
    ///     Minimal entry used by the Building Playground scene.
    /// </summary>
    public sealed class ExampleBuildingEntry : BuildingEntryBase
    {
        [SerializeField] [CanBeNull] private BuildingBase _examplePrefab;

        public void Configure([NotNull] BuildingBase prefab)
        {
            _examplePrefab = prefab;
        }

        [CanBeNull] protected internal override BuildingBase GetPrefab() => _examplePrefab;
    }
}
