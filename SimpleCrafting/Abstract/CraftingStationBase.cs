using Systems.SimpleCore.Operations;
using Systems.SimpleCrafting.Data.Context;
using Systems.SimpleCrafting.Operations;
using UnityEngine;

namespace Systems.SimpleCrafting.Abstract
{
    /// <summary>
    ///     World component that can participate in a crafting operation.
    /// </summary>
    public abstract class CraftingStationBase : MonoBehaviour
    {
        protected internal virtual OperationResult CanUseStation(in CraftingContext context)
            => CraftingOperations.Permitted();
    }
}
