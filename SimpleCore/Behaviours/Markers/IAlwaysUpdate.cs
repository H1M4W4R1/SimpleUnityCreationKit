namespace Systems.SimpleCore.Behaviours.Markers
{
    /// <summary>
    ///     Combines <see cref="IActiveUpdate"/> and <see cref="IInactiveUpdate"/> so the behaviour receives one
    ///     centralized update in both cached enabled states.
    /// </summary>
    public interface IAlwaysUpdate : IActiveUpdate, IInactiveUpdate
    {
    }
}
