using Systems.SimpleCore.Identifiers.Abstract;

namespace Systems.SimpleCore.Identifiers
{
    /// <summary>Exposes a stable identifier owned by an object.</summary>
    /// <typeparam name="TIdentifier">The value type used to identify the object.</typeparam>
    public interface IIdentifiable<TIdentifier> where TIdentifier : struct, IIdentifier
    {
        /// <summary>
        ///     Stable identifier of this object. Identifiable <see cref="Behaviours.SimpleBehaviour"/> instances
        ///     receive a <see cref="Snowflake128"/> automatically during Awake when this value has not been created.
        /// </summary>
        TIdentifier Identifier { get; set; }
    }
}
