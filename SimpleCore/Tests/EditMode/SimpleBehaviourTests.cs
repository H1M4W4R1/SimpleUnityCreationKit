using NUnit.Framework;
using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleCore.Timing;
using UnityEngine;

namespace Systems.SimpleCore.Tests
{
    public sealed class SimpleBehaviourTests
    {
        private GameObject _probeObject;
        private GameObject _tickSystemObject;
        private GameObject _updateSystemObject;
        private TickSystem.TickHandler _registeredTickHandler;

        [SetUp]
        public void SetUp()
        {
            AutoRegistrationDatabase.ClearForTests();
        }

        [TearDown]
        public void TearDown()
        {
            if (!ReferenceEquals(_registeredTickHandler, null))
            {
                TickSystem.UnregisterHandler(_registeredTickHandler);
                _registeredTickHandler = null;
            }

            if (!ReferenceEquals(_probeObject, null)) Object.DestroyImmediate(_probeObject);
            if (!ReferenceEquals(_tickSystemObject, null)) Object.DestroyImmediate(_tickSystemObject);
            if (!ReferenceEquals(_updateSystemObject, null)) Object.DestroyImmediate(_updateSystemObject);
            AutoRegistrationDatabase.ClearForTests();
        }

        [Test]
        public void Awake_InitializesOnlyOnceAndAutomaticallyRegistersInDatabase()
        {
            _probeObject = new GameObject("SimpleBehaviour Probe");
            AutoRegistrationProbe probe = _probeObject.AddComponent<AutoRegistrationProbe>();
            probe.InitializeForTests();
            probe.InitializeForTests();

            bool isRegistered = AutoRegistrationDatabase.TryGet(probe.Identifier, out IAutoRegistrationProbe registeredProbe);

            Assert.AreEqual(1, probe.AwakeCallCount);
            Assert.IsTrue(probe.Identifier.IsCreated);
            Assert.IsTrue(isRegistered);
            Assert.AreSame(probe, registeredProbe);
        }

        [Test]
        public void TickableBehaviour_ReceivesGlobalTicksWithoutManualSubscription()
        {
            TickSystem.TickHandler handler = TickProbe;
            _registeredTickHandler = handler;
            TickSystem.RegisterHandler(handler);
            TickSystem tickSystem = Object.FindAnyObjectByType<TickSystem>(FindObjectsInactive.Include);
            _tickSystemObject = tickSystem.gameObject;
            _probeObject = new GameObject("Tickable Probe");
            TickableProbe probe = _probeObject.AddComponent<TickableProbe>();

            probe.InitializeForTests();
            probe.EnableForTests();

            tickSystem.Tick();

            Assert.AreEqual(1, probe.TickCallCount);
        }

        [Test]
        public void Awake_UpdateContractAutomaticallyCreatesUpdateSystem()
        {
            _probeObject = new GameObject("Update Probe");
            UpdateProbe probe = _probeObject.AddComponent<UpdateProbe>();
            probe.InitializeForTests();

            UpdateSystem updateSystem = Object.FindAnyObjectByType<UpdateSystem>(FindObjectsInactive.Include);

            Assert.IsNotNull(updateSystem);
            _updateSystemObject = updateSystem.gameObject;
        }

        private interface IAutoRegistrationProbe
        {
        }

        private sealed class AutoRegistrationProbe : SimpleBehaviour, IAutoRegistrationProbe,
            IRegisterInDatabase<AutoRegistrationDatabase>, IIdentifiable<Snowflake128>, IAwakeBehaviour
        {
            public int AwakeCallCount { get; private set; }
            public Snowflake128 Identifier { get; set; }

            protected override void OnBehaviourAwake()
            {
                AwakeCallCount++;
            }

            public void InitializeForTests()
            {
                Awake();
            }
        }

        private sealed class TickableProbe : SimpleBehaviour, ITickableBehaviour
        {
            public int TickCallCount { get; private set; }

            protected override void OnTick(float deltaTimeSeconds)
            {
                TickCallCount++;
            }

            public void InitializeForTests()
            {
                Awake();
            }

            public void EnableForTests()
            {
                OnEnable();
            }
        }

        private sealed class UpdateProbe : SimpleBehaviour, IActiveUpdate
        {
            public void InitializeForTests()
            {
                Awake();
            }
        }

        private sealed class AutoRegistrationDatabase :
            RuntimeDatabase<AutoRegistrationDatabase, IAutoRegistrationProbe>
        {
            internal static void ClearForTests()
            {
                Clear();
            }
        }

        private static void TickProbe(float deltaTimeSeconds)
        {
        }
    }
}
