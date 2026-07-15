namespace Systems.SimpleCore.Behaviours.Markers
{
    /// <summary>Combines <see cref="IActiveFixedUpdate"/> and <see cref="IInactiveFixedUpdate"/>.</summary>
    public interface IAlwaysFixedUpdate : IActiveFixedUpdate, IInactiveFixedUpdate
    {
    }
}
