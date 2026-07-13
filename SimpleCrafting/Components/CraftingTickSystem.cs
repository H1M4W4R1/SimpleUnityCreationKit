using Systems.SimpleCore.Timing;
using Systems.SimpleCrafting.Utility;
using UnityEngine;

namespace Systems.SimpleCrafting.Components
{
    /// <summary>
    ///     Advances all active timed crafting instances from the global SimpleCore tick.
    ///     Add one instance to a persistent scene or bootstrap it from the game's startup code.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CraftingTickSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            TickSystem.RegisterHandler(OnTickExecuted);
        }

        private void OnDisable()
        {
            TickSystem.UnregisterHandler(OnTickExecuted);
        }

        private void OnTickExecuted(float deltaTimeSeconds)
        {
            CraftingAPI.AdvanceAllCrafting(deltaTimeSeconds);
        }
    }
}
