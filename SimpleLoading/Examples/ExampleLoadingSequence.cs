using Systems.SimpleLoading.Abstract;
using Systems.SimpleLoading.Data;

namespace Systems.SimpleLoading.Examples
{
    /// <summary>Example two-stage sequence representing game-data and world-data loading.</summary>
    public sealed class ExampleLoadingSequence : LoadingSequenceBase
    {
        private readonly LoadingStageBase[] _stages =
        {
            new ExampleDelayedLoadingStage(0.9f, 1f),
            new ExampleDelayedLoadingStage(1.1f, 2f)
        };

        protected internal override int GetStageCount() => _stages.Length;
        protected internal override LoadingStageBase GetStage(int stageIndex) => _stages[stageIndex];

        protected internal override void OnLoadingProgressed(in LoadingContext context, float progress)
        {
            UnityEngine.Debug.Log("[SimpleLoading] Example loading progress: " + progress.ToString("P0"));
        }
    }
}
