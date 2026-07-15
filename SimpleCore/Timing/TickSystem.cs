using JetBrains.Annotations;
using UnityEngine;
using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;

namespace Systems.SimpleCore.Timing
{
    /// <summary>
    ///     Global tick system
    /// </summary>
    public sealed class TickSystem : SimpleBehaviour, IUniqueBehaviour, IPersistentBehaviour, IAwakeBehaviour,
        IActiveUpdate, IDestroyBehaviour
    {
        private static TickSystem _instance;
        
        public delegate void TickHandler(float deltaTimeSeconds);
        
        /// <summary>
        ///     Timer for tick interval
        /// </summary>
        private float _tickTimer;

        /// <summary>
        ///     Tick interval, if less or equal to 0, ticks every frame
        /// </summary>
        public float TickInterval { get; set; } = 0f;

        /// <summary>
        ///     Disable time passing
        /// </summary>
        public bool CanTimePass { get; set; } = true;
        
        /// <summary>
        ///     If true tick will be executed automatically in update
        /// </summary>
        public bool AutomaticTick { get; set; } = true;
        
        /// <summary>
        ///     Registered handlers will be called every frame or every turn.
        /// </summary>
        internal static event TickHandler OnTickExecuted;

        public static void RegisterHandler([CanBeNull] TickHandler handler)
        {
            if (handler == null) return;
            EnsureExists();
            OnTickExecuted += handler;
        }

        public static void UnregisterHandler(TickHandler handler)
        {
            if (!_instance) return;
            OnTickExecuted -= handler;
        }

        protected override void OnBehaviourAwake()
        {
            _instance = this;
        }

        protected override void OnBehaviourActiveUpdated()
        {
            if (!AutomaticTick) return;
            HandleTick();
        }

        protected override void OnBehaviourDestroyed()
        {
            if (ReferenceEquals(_instance, this)) _instance = null;
        }

        public void Tick() => HandleTick();
        
        internal void HandleTick()
        {
            float timePassedSeconds = Time.deltaTime;

            // Skip if time cannot pass
            if (!CanTimePass) return;

            if (TickInterval <= 0f) // Prevents infinite loops
                OnTickExecuted?.Invoke(timePassedSeconds);
            else
            {
                _tickTimer += timePassedSeconds;

                // Handle interval passed, skip if tick cannot be performed
                // execute for all ticks that completed on this frame
                while (_tickTimer >= TickInterval)
                {
                    _tickTimer -= TickInterval;
                    OnTickExecuted?.Invoke(TickInterval);
                }
            }
        }

        internal static void EnsureExists()
        {
            if (_instance) return;

            _instance = FindAnyObjectByType<TickSystem>(FindObjectsInactive.Include);

            if (_instance)
            {
                _instance.enabled = true;
                _instance.gameObject.SetActive(true);
                return;
            }
            
            _instance = new GameObject("TickSystem").AddComponent<TickSystem>();
        }
    }
}
