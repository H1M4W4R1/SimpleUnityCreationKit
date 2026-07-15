using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data.Context;
using UnityEngine;

namespace Systems.SimpleFactions.Examples
{
    /// <summary>Minimal auto-created faction that logs membership callbacks.</summary>
    public sealed class ExampleFaction : FactionBase<ExampleFactionHolder>
    {
        protected internal override void OnJoined(
            in JoinFactionContext<ExampleFactionHolder> context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.member, null)) return;
            Debug.Log("[ExampleFaction] " + context.member.name + " joined " + name + ".");
        }

        protected internal override void OnLeft(
            in LeaveFactionContext<ExampleFactionHolder> context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.member, null)) return;
            Debug.Log("[ExampleFaction] " + context.member.name + " left " + name + ".");
        }
    }
}
