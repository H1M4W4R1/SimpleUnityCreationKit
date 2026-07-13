using Systems.SimpleCore.Timing;

namespace Systems.SimpleSpawn.Abstract
{  
    /// <summary>
    ///     Controls automatic validation and cleanup before an entity is destroyed.
    /// </summary>
    public abstract class AutomaticDespawnControlBase : DespawnControlBase
    {
        protected virtual void OnEnable()
        {
            TickSystem.RegisterHandler(OnTick);
        }

        private void OnTick(float deltaTimeSeconds)
        {
            TryDespawn();
        }

        protected virtual void OnDisable()
        {
            TickSystem.UnregisterHandler(OnTick);
        }
    }
}
