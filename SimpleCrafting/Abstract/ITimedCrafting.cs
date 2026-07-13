namespace Systems.SimpleCrafting.Abstract
{
    /// <summary>
    ///     Opts a recipe into the timed crafting flow.
    /// </summary>
    public interface ITimedCrafting
    {
        float DurationSeconds { get; }
    }
}
