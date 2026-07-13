using System;
using Systems.SimpleCore.Operations;
using Systems.SimpleLoading.Abstract;
using Systems.SimpleLoading.Data;
using Systems.SimpleLoading.Operations;

namespace Systems.SimpleLoading.Examples
{
    /// <summary>Simple serializable stage used by the example to simulate independently progressing data work.</summary>
    [Serializable]
    public sealed class ExampleDelayedLoadingStage : LoadingStageBase
    {
        private readonly float _durationSeconds;
        private readonly float _weight;

        public override float TimeWeight => _weight;

        public ExampleDelayedLoadingStage(float durationSeconds, float weight)
        {
            _durationSeconds = durationSeconds;
            _weight = weight;
        }

        public override ILoadingStageOperation CreateOperation(in LoadingContext context)
            => new Operation(_durationSeconds);

        private sealed class Operation : ILoadingStageOperation
        {
            private readonly float _durationSeconds;
            private float _elapsedSeconds;

            public Operation(float durationSeconds)
            {
                _durationSeconds = durationSeconds;
            }

            public OperationResult Begin(in LoadingContext context) => LoadingOperations.Permitted();

            public LoadingStageUpdate Update(in LoadingContext context, float deltaTime)
            {
                _elapsedSeconds += deltaTime;
                if (_elapsedSeconds < _durationSeconds)
                    return LoadingStageUpdate.Continue(_elapsedSeconds / _durationSeconds);
                return LoadingStageUpdate.Complete();
            }

            public void Cancel(in LoadingContext context) { }
        }
    }
}
