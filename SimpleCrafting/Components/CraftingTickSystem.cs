using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;
using Systems.SimpleCrafting.Utility;
using UnityEngine;

namespace Systems.SimpleCrafting.Components
{
    /// <summary>
    ///     Advances all active timed crafting instances from the global SimpleCore tick.
    ///     Add one instance to a persistent scene or bootstrap it from the game's startup code.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CraftingTickSystem : SimpleBehaviour, ITickableBehaviour
    {
        protected override void OnTick(float deltaTimeSeconds)
        {
            CraftingAPI.AdvanceAllCrafting(deltaTimeSeconds);
        }
    }
}
