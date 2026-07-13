using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data.Context;
using UnityEngine;

namespace Systems.SimpleFactions.Examples
{
    /// <summary>
    ///     Minimal faction example. The sealed class is auto-created as a ScriptableObject asset
    ///     in <c>Assets/Generated/Factions/</c> and registered in
    ///     <c>FactionDatabase</c> automatically via the <c>AutoCreate</c> attribute on
    ///     <see cref="FactionBase"/>.
    /// </summary>
    /// <remarks>
    ///     This example demonstrates how to override typed event callbacks. Replace
    ///     <see cref="ExampleFactionHolder"/> with the MonoBehaviour or interface that represents
    ///     your game objects.
    /// </remarks>
    public sealed class ExampleFaction : FactionBase<ExampleFactionHolder>
    {
        protected internal override void OnJoined(
            in JoinFactionContext<ExampleFactionHolder> context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.member, null)) return;
            Debug.Log($"[ExampleFaction] {context.member.name} joined {name}.");
        }

        protected internal override void OnLeft(
            in LeaveFactionContext<ExampleFactionHolder> context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.member, null)) return;
            Debug.Log($"[ExampleFaction] {context.member.name} left {name}.");
        }

        protected internal override void OnReputationChanged(
            in ReputationChangeContext<ExampleFactionHolder> context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.member, null)) return;
            long newRep = context.previousReputation + context.amountRequested;
            Debug.Log($"[ExampleFaction] {context.member.name} reputation: {context.previousReputation} → {newRep}.");
        }

        protected internal override void OnLevelChanged(
            in FactionLevelChangeContext<ExampleFactionHolder> context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.member, null)) return;
            string levelName = ReferenceEquals(context.newLevel, null) ? "none" : context.newLevel.name;
            Debug.Log($"[ExampleFaction] {context.member.name} level changed to: {levelName}.");
        }
    }
}
