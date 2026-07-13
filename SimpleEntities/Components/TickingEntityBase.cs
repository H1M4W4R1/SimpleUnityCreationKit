using Systems.SimpleCore.Timing;

namespace Systems.SimpleEntities.Components
{
    public abstract class TickingEntityBase : EntityBase
    {
        private bool _startCompleted;
        private bool _pendingTickSubscription;

        protected override void OnEntitySetupComplete()
        {
            base.OnEntitySetupComplete();
            _startCompleted = true;

            // If OnEnable fired before Start, subscribe now
            if (_pendingTickSubscription)
            {
                _pendingTickSubscription = false;
                TickSystem.RegisterHandler(OnTick);
            }
        }

        protected override void OnEntityActivated()
        {
            base.OnEntityActivated();

            if (_startCompleted)
                TickSystem.RegisterHandler(OnTick);
            else
                _pendingTickSubscription = true;
        }

        protected override void OnEntityDeactivated()
        {
            base.OnEntityDeactivated();
            _pendingTickSubscription = false;
            TickSystem.UnregisterHandler(OnTick);
        }

        protected virtual void OnTick(float deltaTime)
        {
        }
    }
}