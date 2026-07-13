namespace Systems.SimpleRequests.Abstract
{
    /// <summary>
    ///     Marks a value type as data that can be sent through the SimpleRequests system.
    /// </summary>
    /// <remarks>
    ///     Request contexts should contain only the data required by their handlers. Use a separate
    ///     response value type when handlers need to produce a result.
    /// </remarks>
    public interface IRequestContext
    {
    }
}
