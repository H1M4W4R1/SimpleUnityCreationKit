using System;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleLoading.Abstract;
using Systems.SimpleLoading.Data;
using Systems.SimpleLoading.Operations;
using Systems.SimpleLoading.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleLoading.Tests
{
    public sealed class LoadingAPITests
    {
        private TestSequence _sequence;

        [SetUp]
        public void SetUp()
        {
            _sequence = ScriptableObject.CreateInstance<TestSequence>();
        }

        [TearDown]
        public void TearDown()
        {
            LoadingAPI.ClearForTests();
            if (_sequence) Object.DestroyImmediate(_sequence);
        }

        [Test]
        public void Load_TracksGenericHandleAndWeightedProgressAcrossStages()
        {
            _sequence.Configure(new TestStage(1f, 1), new TestStage(3f, 1));

            LoadingSequenceHandle<TestSequence> handle = LoadingAPI.Load(_sequence);
            LoadingAPI.Advance(0.1f);

            Assert.IsTrue(handle.IsValid);
            Assert.AreEqual(1, LoadingAPI.GetCurrentStage(handle));
            Assert.That(LoadingAPI.GetCurrentTotalPercentage(handle), Is.EqualTo(0.25f).Within(0.001f));

            LoadingAPI.Advance(0.1f);

            Assert.IsTrue(LoadingAPI.IsLoadingComplete(handle));
            Assert.That(LoadingAPI.GetCurrentTotalPercentage(handle), Is.EqualTo(1f));
            Assert.AreEqual(2, _sequence.completedStageCount);
            Assert.AreEqual(1, _sequence.completedSequenceCount);
        }

        [Test]
        public void AbortLoading_CancelsCurrentOperationAndPreservesTerminalState()
        {
            TestStage stage = new TestStage(1f, 10);
            _sequence.Configure(stage);
            LoadingSequenceHandle<TestSequence> handle = LoadingAPI.Load(_sequence);

            OperationResult result = LoadingAPI.AbortLoading(handle);

            Assert.IsTrue(result);
            Assert.AreEqual(1, stage.cancelCount);
            Assert.AreEqual(LoadingStatus.Cancelled, LoadingAPI.GetStatus(handle));
            Assert.IsFalse(LoadingAPI.IsLoadingComplete(handle));
        }

        [Test]
        public void TryLoad_ReportsSequenceValidationFailureWithoutAHandle()
        {
            _sequence.AllowStart = false;

            LoadingSequenceStartResult<TestSequence> result = LoadingAPI.TryLoad(_sequence);

            Assert.IsFalse(result.result);
            Assert.IsFalse(result.handle.IsValid);
            Assert.AreEqual(1, _sequence.failedSequenceCount);
        }

        [Test]
        public void ShouldLoadWorldPart_UsesUnloadDistanceAsHysteresis()
        {
            Vector3 partPosition = Vector3.zero;

            Assert.IsTrue(LoadingAPI.ShouldLoadWorldPart(new Vector3(7f, 0f, 0f), partPosition, 8f, 12f, false));
            Assert.IsFalse(LoadingAPI.ShouldLoadWorldPart(new Vector3(9f, 0f, 0f), partPosition, 8f, 12f, false));
            Assert.IsTrue(LoadingAPI.ShouldLoadWorldPart(new Vector3(9f, 0f, 0f), partPosition, 8f, 12f, true));
            Assert.IsFalse(LoadingAPI.ShouldLoadWorldPart(new Vector3(13f, 0f, 0f), partPosition, 8f, 12f, true));
        }

        [Test]
        public void AddressableDatabaseStage_StartsDatabaseAndCompletesFromItsPollingState()
        {
            TestAddressableDatabase database = new TestAddressableDatabase();
            _sequence.Configure(new TestDatabaseStage(database));

            LoadingSequenceHandle<TestSequence> handle = LoadingAPI.Load(_sequence);

            Assert.AreEqual(1, database.beginLoadingCount);
            Assert.IsFalse(LoadingAPI.IsLoadingComplete(handle));

            database.IsLoaded = true;
            database.IsLoadingComplete = true;
            LoadingAPI.Advance(0f);

            Assert.IsTrue(LoadingAPI.IsLoadingComplete(handle));
        }

        private sealed class TestSequence : LoadingSequenceBase
        {
            private LoadingStageBase[] _stages = Array.Empty<LoadingStageBase>();

            public bool AllowStart { get; set; } = true;
            public int completedStageCount { get; private set; }
            public int completedSequenceCount { get; private set; }
            public int failedSequenceCount { get; private set; }

            public void Configure(params LoadingStageBase[] stages)
            {
                _stages = stages;
            }

            protected internal override int GetStageCount() => _stages.Length;
            protected internal override LoadingStageBase GetStage(int stageIndex) => _stages[stageIndex];

            protected internal override OperationResult CanStartLoading(in LoadingContext context)
                => AllowStart ? LoadingOperations.Permitted() : LoadingOperations.SequenceIsNull();

            protected internal override void OnStageCompleted(in LoadingContext context, int stageIndex)
            {
                completedStageCount++;
            }

            protected internal override void OnLoadingCompleted(in LoadingContext context, in OperationResult result)
            {
                completedSequenceCount++;
            }

            protected internal override void OnLoadingFailed(in LoadingContext context, in OperationResult result)
            {
                failedSequenceCount++;
            }
        }

        [Serializable]
        private sealed class TestStage : LoadingStageBase
        {
            private readonly float _weight;
            private readonly int _updatesToComplete;

            public int cancelCount { get; private set; }
            public override float TimeWeight => _weight;

            public TestStage(float weight, int updatesToComplete)
            {
                _weight = weight;
                _updatesToComplete = updatesToComplete;
            }

            public override ILoadingStageOperation CreateOperation(in LoadingContext context)
                => new TestOperation(this, _updatesToComplete);

            private sealed class TestOperation : ILoadingStageOperation
            {
                private readonly TestStage _stage;
                private readonly int _updatesToComplete;
                private int _updateCount;

                public TestOperation(TestStage stage, int updatesToComplete)
                {
                    _stage = stage;
                    _updatesToComplete = updatesToComplete;
                }

                public OperationResult Begin(in LoadingContext context) => LoadingOperations.Permitted();

                public LoadingStageUpdate Update(in LoadingContext context, float deltaTime)
                {
                    _updateCount++;
                    return _updateCount >= _updatesToComplete
                        ? LoadingStageUpdate.Complete()
                        : LoadingStageUpdate.Continue((float)_updateCount / _updatesToComplete);
                }

                public void Cancel(in LoadingContext context)
                {
                    _stage.cancelCount++;
                }
            }
        }

        private sealed class TestDatabaseStage : DatabaseLoadingStageBase<TestAddressableDatabase>
        {
            private readonly TestAddressableDatabase _database;

            protected override TestAddressableDatabase Database => _database;

            public TestDatabaseStage(TestAddressableDatabase database)
            {
                _database = database;
            }
        }

        private sealed class TestAddressableDatabase : IDatabaseLoading
        {
            public bool IsLoading { get; private set; }
            public bool IsLoadingComplete { get; set; }
            public bool IsLoaded { get; set; }
            public float CurrentLoadProgress => IsLoaded ? 1f : 0.5f;
            public int beginLoadingCount { get; private set; }

            public void BeginLoading()
            {
                beginLoadingCount++;
                IsLoading = true;
            }
        }
    }
}
