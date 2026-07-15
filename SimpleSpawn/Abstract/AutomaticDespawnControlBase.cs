using Systems.SimpleCore.Timing;

using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;

namespace Systems.SimpleSpawn.Abstract
{  
    /// <summary>
    ///     Controls automatic validation and cleanup before an entity is destroyed.
    /// </summary>
    public abstract class AutomaticDespawnControlBase : DespawnControlBase, ITickableBehaviour
    {
        protected override void OnTick(float deltaTimeSeconds)
        {
            TryDespawn();
        }
    }
}
