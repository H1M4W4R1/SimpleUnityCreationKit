using JetBrains.Annotations;
using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Data;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleBuilding.Operations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using UnityEngine;

namespace Systems.SimpleBuilding.Abstract
{
    /// <summary>
    ///     Configuration and game-specific rules for one type of building.
    /// </summary>
    [AutoCreate("Buildings", BuildingEntryDatabase.LABEL)]
    public abstract class BuildingEntryBase : ScriptableObject
    {
        [field: SerializeField] [CanBeNull] public BuildingBase Prefab { get; private set; }
        [field: SerializeField] [CanBeNull] public GameObject GhostPrefab { get; private set; }

        /// <summary>
        ///     Returns the runtime prefab used for completed buildings.
        /// </summary>
        [CanBeNull] protected internal virtual BuildingBase GetPrefab() => Prefab;

        /// <summary>
        ///     Determines whether this entry can be displayed or selected by a building controller.
        /// </summary>
        protected internal virtual OperationResult IsAvailable(in BuildingSelectionContext context)
            => BuildingOperations.Permitted();

        /// <summary>
        ///     Performs placement-specific validation after the common placement checks succeed.
        /// </summary>
        protected internal virtual OperationResult CanBuild(in BuildingPlacementContext context)
            => BuildingOperations.Permitted();

        /// <summary>
        ///     Consumes the complete placement cost atomically.
        /// </summary>
        protected internal virtual OperationResult TryConsumeResources(in BuildingPlacementContext context)
            => BuildingOperations.Permitted();

        /// <summary>
        ///     Performs demolition-specific validation after the common demolition checks succeed.
        /// </summary>
        protected internal virtual OperationResult CanDemolish(in BuildingDemolitionContext context)
            => BuildingOperations.Permitted();

        /// <summary>
        ///     Refunds demolition resources atomically. Override this to opt into demolition refunds.
        /// </summary>
        protected internal virtual OperationResult TryRefundResources(in BuildingDemolitionContext context)
            => BuildingOperations.Permitted();

        protected internal virtual void OnBuildingPlaced(
            in BuildingPlacementContext context,
            [NotNull] BuildingBase building,
            in OperationResult result)
        {
        }

        protected internal virtual void OnBuildingPlacementFailed(
            in BuildingPlacementContext context,
            in OperationResult result)
        {
        }

        protected internal virtual void OnBuildingDemolished(
            in BuildingDemolitionContext context,
            in OperationResult result)
        {
        }

        protected internal virtual void OnBuildingDemolitionFailed(
            in BuildingDemolitionContext context,
            in OperationResult result)
        {
        }

        [CanBeNull]
        internal GameObject GetGhostPrefab()
        {
            if (!ReferenceEquals(GhostPrefab, null) && GhostPrefab) return GhostPrefab;

            BuildingBase prefab = GetPrefab();
            if (ReferenceEquals(prefab, null) || !prefab) return null;
            return prefab.gameObject;
        }
    }
}
