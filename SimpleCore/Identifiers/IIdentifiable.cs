using Systems.SimpleCore.Identifiers.Abstract;

namespace Systems.SimpleCore.Identifiers
{
    /// <summary>Exposes a stable identifier owned by an object.</summary>
    /// <typeparam name="TIdentifier">The value type used to identify the object.</typeparam>
    public interface IIdentifiable<TIdentifier> where TIdentifier : struct, IIdentifier
    {
        /// <summary>Stable identifier of this object.</summary>
        TIdentifier Identifier { get; }
    }
}
