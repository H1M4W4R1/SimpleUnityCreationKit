using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleCore.Behaviours
{
    /// <summary>
    ///     Owns the Unity frame callbacks for <see cref="SimpleBehaviour"/> instances. Behaviours register only when
    ///     they implement an update contract, avoiding a Unity callback bridge on every component.
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public sealed class UpdateSystem : MonoBehaviour
    {
        [NotNull] private readonly List<SimpleBehaviour> _frameBehaviours = new List<SimpleBehaviour>();
        [NotNull] private readonly List<SimpleBehaviour> _fixedBehaviours = new List<SimpleBehaviour>();
        [NotNull] private readonly List<SimpleBehaviour> _lateBehaviours = new List<SimpleBehaviour>();
        [NotNull] private readonly List<SimpleBehaviour> _pendingFrameUnregistrations = new List<SimpleBehaviour>();
        [NotNull] private readonly List<SimpleBehaviour> _pendingFixedUnregistrations = new List<SimpleBehaviour>();
        [NotNull] private readonly List<SimpleBehaviour> _pendingLateUnregistrations = new List<SimpleBehaviour>();
        private bool _isExecutingFrameUpdates;
        private bool _isExecutingFixedUpdates;
        private bool _isExecutingLateUpdates;
        private static UpdateSystem _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBeforeSceneLoad()
        {
            EnsureExists();
        }

        private void Awake()
        {
            if (_instance && !ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(_instance, this)) _instance = null;
        }

        private void Update()
        {
            ExecuteFrameUpdates();
        }

        private void FixedUpdate()
        {
            ExecuteFixedUpdates();
        }

        private void LateUpdate()
        {
            ExecuteLateUpdates();
        }

        internal static void Register([NotNull] SimpleBehaviour behaviour, bool requiresFrameUpdate,
            bool requiresFixedUpdate, bool requiresLateUpdate)
        {
            if (!requiresFrameUpdate && !requiresFixedUpdate && !requiresLateUpdate) return;

            UpdateSystem updateSystem = EnsureExists();
            if (requiresFrameUpdate) updateSystem._frameBehaviours.Add(behaviour);
            if (requiresFixedUpdate) updateSystem._fixedBehaviours.Add(behaviour);
            if (requiresLateUpdate) updateSystem._lateBehaviours.Add(behaviour);
        }

        internal static void Unregister([NotNull] SimpleBehaviour behaviour, bool requiresFrameUpdate,
            bool requiresFixedUpdate, bool requiresLateUpdate)
        {
            if (!_instance) return;

            if (requiresFrameUpdate) _instance.UnregisterFrameBehaviour(behaviour);
            if (requiresFixedUpdate) _instance.UnregisterFixedBehaviour(behaviour);
            if (requiresLateUpdate) _instance.UnregisterLateBehaviour(behaviour);
        }

        [NotNull] private static UpdateSystem EnsureExists()
        {
            if (_instance) return _instance;

            UpdateSystem existingSystem = FindAnyObjectByType<UpdateSystem>(FindObjectsInactive.Include);
            if (existingSystem)
            {
                _instance = existingSystem;
                _instance.enabled = true;
                _instance.gameObject.SetActive(true);
                return _instance;
            }

            GameObject systemObject = new GameObject(nameof(UpdateSystem));
            return systemObject.AddComponent<UpdateSystem>();
        }

        private void ExecuteFrameUpdates()
        {
            _isExecutingFrameUpdates = true;
            try
            {
                for (int behaviourIndex = _frameBehaviours.Count - 1; behaviourIndex >= 0; behaviourIndex--)
                {
                    SimpleBehaviour behaviour = _frameBehaviours[behaviourIndex];
                    behaviour.ExecuteFrameUpdate();
                }
            }
            finally
            {
                _isExecutingFrameUpdates = false;
                FlushUnregistrations(_frameBehaviours, _pendingFrameUnregistrations);
            }
        }

        private void ExecuteFixedUpdates()
        {
            _isExecutingFixedUpdates = true;
            try
            {
                for (int behaviourIndex = _fixedBehaviours.Count - 1; behaviourIndex >= 0; behaviourIndex--)
                {
                    SimpleBehaviour behaviour = _fixedBehaviours[behaviourIndex];
                    behaviour.ExecuteFixedUpdate();
                }
            }
            finally
            {
                _isExecutingFixedUpdates = false;
                FlushUnregistrations(_fixedBehaviours, _pendingFixedUnregistrations);
            }
        }

        private void ExecuteLateUpdates()
        {
            _isExecutingLateUpdates = true;
            try
            {
                for (int behaviourIndex = _lateBehaviours.Count - 1; behaviourIndex >= 0; behaviourIndex--)
                {
                    SimpleBehaviour behaviour = _lateBehaviours[behaviourIndex];
                    behaviour.ExecuteLateUpdate();
                }
            }
            finally
            {
                _isExecutingLateUpdates = false;
                FlushUnregistrations(_lateBehaviours, _pendingLateUnregistrations);
            }
        }

        private void UnregisterFrameBehaviour([NotNull] SimpleBehaviour behaviour)
        {
            if (_isExecutingFrameUpdates)
            {
                _pendingFrameUnregistrations.Add(behaviour);
                return;
            }

            _frameBehaviours.Remove(behaviour);
        }

        private void UnregisterFixedBehaviour([NotNull] SimpleBehaviour behaviour)
        {
            if (_isExecutingFixedUpdates)
            {
                _pendingFixedUnregistrations.Add(behaviour);
                return;
            }

            _fixedBehaviours.Remove(behaviour);
        }

        private void UnregisterLateBehaviour([NotNull] SimpleBehaviour behaviour)
        {
            if (_isExecutingLateUpdates)
            {
                _pendingLateUnregistrations.Add(behaviour);
                return;
            }

            _lateBehaviours.Remove(behaviour);
        }

        private static void FlushUnregistrations([NotNull] List<SimpleBehaviour> behaviours,
            [NotNull] List<SimpleBehaviour> pendingUnregistrations)
        {
            for (int behaviourIndex = 0; behaviourIndex < pendingUnregistrations.Count; behaviourIndex++)
            {
                behaviours.Remove(pendingUnregistrations[behaviourIndex]);
            }

            pendingUnregistrations.Clear();
        }
    }
}
