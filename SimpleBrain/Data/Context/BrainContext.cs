using Systems.SimpleBrain.Components;

namespace Systems.SimpleBrain.Data.Context
{
    /// <summary>
    ///     Stack-only context supplied to knowledge and decisions owned by a brain.
    /// </summary>
    public readonly ref struct BrainContext
    {
        public readonly BrainBase brain;

        public BrainContext(BrainBase brain)
        {
            this.brain = brain;
        }
    }
}
