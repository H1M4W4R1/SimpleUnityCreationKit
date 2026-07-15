namespace Systems.SimpleCore.Behaviours.Markers
{
    /// <summary>Combines <see cref="IActiveLateUpdate"/> and <see cref="IInactiveLateUpdate"/>.</summary>
    public interface IAlwaysLateUpdate : IActiveLateUpdate, IInactiveLateUpdate
    {
    }
}
