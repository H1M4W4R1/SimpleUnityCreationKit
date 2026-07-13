using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleCore.Operations;
using UnityEngine;

namespace Systems.SimpleBuilding.Examples
{
    /// <summary>
    ///     Free-placement building used by the Building Playground scene.
    /// </summary>
    public sealed class ExampleBuilding : BuildingBase
    {
        protected internal override void OnBuildingPlaced(in BuildingPlacementContext context, in OperationResult result)
        {
            base.OnBuildingPlaced(in context, in result);
            Debug.Log($"Building placed isSave: {context.isSaveSystemRequest}");
        }

        protected internal override void OnBuildingDemolished(in BuildingDemolitionContext context, in OperationResult result)
        {
            base.OnBuildingDemolished(in context, in result);
            Debug.Log($"Building demolished isSave: {context.isSaveSystemRequest}");
        }
    }
}
