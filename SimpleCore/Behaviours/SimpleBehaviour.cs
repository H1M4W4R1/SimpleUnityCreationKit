using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Behaviours.Markers;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Timing;
using UnityEngine;

namespace Systems.SimpleCore.Behaviours
{
    /// <summary>
    ///     Contract-driven MonoBehaviour base that forwards Unity lifecycle messages and manages common runtime
    ///     behaviour contracts. Do not declare Unity lifecycle methods in subclasses; implement the relevant
    ///     contract instead.
    /// </summary>
    public abstract class SimpleBehaviour : MonoBehaviour
    {
        [CanBeNull] private IRegisterInDatabase _registerInDatabase;
        private RuntimeTypeHandle _behaviourTypeHandle;
        private bool _isUnique;
        private bool _isDuplicate;
        private bool _isEnabled;
        private bool _hasAwakened;
        private bool _isRuntimeDatabaseRegistered;
        private bool _isTickRegistered;
        private bool _isFrameUpdateRegistered;
        private bool _isFixedUpdateRegistered;
        private bool _isLateUpdateRegistered;

        /// <summary>Cached Transform for derived behaviours that use it frequently.</summary>
        [NotNull] protected Transform CachedTransform { get; private set; } = null!;

        /// <summary>Cached enabled state updated by the base OnEnable and OnDisable lifecycle methods.</summary>
        protected bool IsBehaviourEnabled => _isEnabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            UniqueBehaviourRegistry.Clear();
        }

        protected void Awake()
        {
            if (_hasAwakened) return;
            _hasAwakened = true;

            CachedTransform = transform;
            EnsureIdentifier();
            SetupAndValidateComponents();
            Initialize();

            _behaviourTypeHandle = GetType().TypeHandle;
            _isUnique = this is IUniqueBehaviour;
            _registerInDatabase = this as IRegisterInDatabase;

            if (_isUnique && !UniqueBehaviourRegistry.TryRegister(_behaviourTypeHandle, this))
            {
                _isDuplicate = true;
                Destroy(gameObject);
                return;
            }

            if (this is IPersistentBehaviour && Application.isPlaying) DontDestroyOnLoad(gameObject);

            RegisterForUpdates();

            if (this is IAwakeBehaviour) OnBehaviourAwake();

            if (!ReferenceEquals(_registerInDatabase, null))
                _isRuntimeDatabaseRegistered = _registerInDatabase.RegisterInDatabase(this);
        }

        protected void Start()
        {
            if (_isDuplicate) return;
            if (this is IStartBehaviour) OnBehaviourStarted();
        }

        protected void OnEnable()
        {
            if (_isDuplicate) return;

            _isEnabled = true;
            if (this is IEnableBehaviour) OnBehaviourEnabled();
            if (_isTickRegistered || !(this is ITickableBehaviour)) return;

            TickSystem.RegisterHandler(OnTick);
            _isTickRegistered = true;
        }

        protected void OnDisable()
        {
            if (_isDuplicate) return;

            _isEnabled = false;
            if (this is IDisableBehaviour) OnBehaviourDisabled();
            UnregisterTick();
        }

        protected void OnDestroy()
        {
            if (_isDuplicate) return;
            if (this is IDestroyBehaviour) OnBehaviourDestroyed();

            UnregisterTick();
            UnregisterFromUpdates();
            if (_isRuntimeDatabaseRegistered && !ReferenceEquals(_registerInDatabase, null))
                _registerInDatabase.UnregisterFromDatabase(this);

            if (_isUnique) UniqueBehaviourRegistry.Unregister(_behaviourTypeHandle, this);
        }

        internal void ExecuteFrameUpdate()
        {
            if (_isDuplicate) return;

            if (_isEnabled)
            {
                if (this is IActiveUpdate) OnBehaviourActiveUpdated();
                return;
            }

            if (this is IInactiveUpdate) OnBehaviourInactiveUpdated();
        }

        internal void ExecuteFixedUpdate()
        {
            if (_isDuplicate) return;

            if (_isEnabled)
            {
                if (this is IActiveFixedUpdate) OnBehaviourActiveFixedUpdated();
                return;
            }

            if (this is IInactiveFixedUpdate) OnBehaviourInactiveFixedUpdated();
        }

        internal void ExecuteLateUpdate()
        {
            if (_isDuplicate) return;

            if (_isEnabled)
            {
                if (this is IActiveLateUpdate) OnBehaviourActiveLateUpdated();
                return;
            }

            if (this is IInactiveLateUpdate) OnBehaviourInactiveLateUpdated();
        }

        private void EnsureIdentifier()
        {
            if (this is not IIdentifiable<Snowflake128> identifiable || identifiable.Identifier.IsCreated) return;
            identifiable.Identifier = Snowflake128.New();
        }

        private void RegisterForUpdates()
        {
            bool requiresFrameUpdate = this is IActiveUpdate || this is IInactiveUpdate;
            bool requiresFixedUpdate = this is IActiveFixedUpdate || this is IInactiveFixedUpdate;
            bool requiresLateUpdate = this is IActiveLateUpdate || this is IInactiveLateUpdate;
            if (!requiresFrameUpdate && !requiresFixedUpdate && !requiresLateUpdate) return;

            UpdateSystem.Register(this, requiresFrameUpdate, requiresFixedUpdate, requiresLateUpdate);
            _isFrameUpdateRegistered = requiresFrameUpdate;
            _isFixedUpdateRegistered = requiresFixedUpdate;
            _isLateUpdateRegistered = requiresLateUpdate;
        }

        private void UnregisterFromUpdates()
        {
            if (!_isFrameUpdateRegistered && !_isFixedUpdateRegistered && !_isLateUpdateRegistered) return;

            UpdateSystem.Unregister(this, _isFrameUpdateRegistered, _isFixedUpdateRegistered,
                _isLateUpdateRegistered);
            _isFrameUpdateRegistered = false;
            _isFixedUpdateRegistered = false;
            _isLateUpdateRegistered = false;
        }

        private void UnregisterTick()
        {
            if (!_isTickRegistered) return;

            TickSystem.UnregisterHandler(OnTick);
            _isTickRegistered = false;
        }

        /// <summary>
        ///     Assigns and validates required component references before initialization. The cached transform is
        ///     available at this point.
        /// </summary>
        protected virtual void SetupAndValidateComponents()
        {
        }

        /// <summary>Initializes behaviour state once after component setup and validation during Awake.</summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>Called during Awake when this behaviour implements <see cref="IAwakeBehaviour"/>.</summary>
        protected virtual void OnBehaviourAwake()
        {
        }

        /// <summary>Called during Start when this behaviour implements <see cref="IStartBehaviour"/>.</summary>
        protected virtual void OnBehaviourStarted()
        {
        }

        /// <summary>Called when enabled if this behaviour implements <see cref="IEnableBehaviour"/>.</summary>
        protected virtual void OnBehaviourEnabled()
        {
        }

        /// <summary>Called when disabled if this behaviour implements <see cref="IDisableBehaviour"/>.</summary>
        protected virtual void OnBehaviourDisabled()
        {
        }

        /// <summary>Called during destruction if this behaviour implements <see cref="IDestroyBehaviour"/>.</summary>
        protected virtual void OnBehaviourDestroyed()
        {
        }

        /// <summary>Called by <see cref="UpdateSystem"/> while enabled for <see cref="IActiveUpdate"/>.</summary>
        protected virtual void OnBehaviourActiveUpdated()
        {
        }

        /// <summary>
        ///     Called by <see cref="UpdateSystem"/> while disabled when this behaviour implements
        ///     <see cref="IInactiveUpdate"/>.
        /// </summary>
        protected virtual void OnBehaviourInactiveUpdated()
        {
        }

        /// <summary>
        ///     Called by <see cref="UpdateSystem"/> while enabled for <see cref="IActiveFixedUpdate"/>.
        /// </summary>
        protected virtual void OnBehaviourActiveFixedUpdated()
        {
        }

        /// <summary>
        ///     Called by <see cref="UpdateSystem"/> while disabled for <see cref="IInactiveFixedUpdate"/>.
        /// </summary>
        protected virtual void OnBehaviourInactiveFixedUpdated()
        {
        }

        /// <summary>Called by <see cref="UpdateSystem"/> while enabled for <see cref="IActiveLateUpdate"/>.</summary>
        protected virtual void OnBehaviourActiveLateUpdated()
        {
        }

        /// <summary>Called by <see cref="UpdateSystem"/> while disabled for <see cref="IInactiveLateUpdate"/>.</summary>
        protected virtual void OnBehaviourInactiveLateUpdated()
        {
        }

        /// <summary>Called on the global tick if this behaviour implements <see cref="ITickableBehaviour"/>.</summary>
        protected virtual void OnTick(float deltaTimeSeconds)
        {
        }

        private static class UniqueBehaviourRegistry
        {
            [NotNull] private static readonly Dictionary<RuntimeTypeHandle, SimpleBehaviour> instances =
                new Dictionary<RuntimeTypeHandle, SimpleBehaviour>();

            internal static bool TryRegister(RuntimeTypeHandle behaviourTypeHandle, [NotNull] SimpleBehaviour behaviour)
            {
                if (instances.TryGetValue(behaviourTypeHandle, out SimpleBehaviour existingBehaviour))
                {
                    if (existingBehaviour && !ReferenceEquals(existingBehaviour, behaviour)) return false;
                    instances.Remove(behaviourTypeHandle);
                }

                instances.Add(behaviourTypeHandle, behaviour);
                return true;
            }

            internal static void Unregister(RuntimeTypeHandle behaviourTypeHandle, [NotNull] SimpleBehaviour behaviour)
            {
                if (!instances.TryGetValue(behaviourTypeHandle, out SimpleBehaviour existingBehaviour)) return;
                if (!ReferenceEquals(existingBehaviour, behaviour)) return;
                instances.Remove(behaviourTypeHandle);
            }

            internal static void Clear()
            {
                instances.Clear();
            }
        }
    }
}
