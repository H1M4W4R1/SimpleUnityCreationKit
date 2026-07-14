using Systems.SimpleBrain.Components;

namespace Systems.SimpleBrain.Data.Context
{
    /// <summary>
    ///     Stack-only context supplied to a brain subprocess callback.
    /// </summary>
    public readonly ref struct BrainSubprocessContext
    {
        public readonly BrainBase brain;
        public readonly bool isComaInduced;
        public readonly float deltaTimeSeconds;

        public BrainSubprocessContext(
            BrainBase brain,
            bool isComaInduced = false,
            float deltaTimeSeconds = 0f)
        {
            this.brain = brain;
            this.isComaInduced = isComaInduced;
            this.deltaTimeSeconds = deltaTimeSeconds;
        }
    }
}
