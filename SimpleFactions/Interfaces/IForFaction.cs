using Systems.SimpleFactions.Abstract;

namespace Systems.SimpleFactions.Interfaces
{
    /// <summary>
    ///     Marker interface. Concrete <see cref="ReputationLevelBase"/> types that implement
    ///     <see cref="IForFaction{TFaction}"/> are automatically discovered by
    ///     <c>FactionLevelAssigner</c> on script reload and asset import, and assigned to the
    ///     target faction's level list — sorted ascending by
    ///     <see cref="ReputationLevelBase.PromotionThreshold"/>.
    /// </summary>
    /// <typeparam name="TFaction">
    ///     The concrete <see cref="FactionBase"/> subclass this level belongs to.
    /// </typeparam>
    /// <remarks>
    ///     Levels that do <b>not</b> implement this interface are never auto-assigned.
    ///     They can still be added to a faction manually via the Inspector.
    /// </remarks>
    public interface IForFaction<TFaction> where TFaction : FactionBase { }
}
