using Systems.SimpleCore.Timing;

using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;

namespace Systems.SimpleEntities.Components
{
    public abstract class TickingEntityBase : EntityBase, ITickableBehaviour
    {
        protected override void OnTick(float deltaTime)
        {
        }
    }
}
