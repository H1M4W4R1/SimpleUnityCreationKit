using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Utility;
using UnityEngine;

namespace Systems.SimpleFactions.Examples
{
    /// <summary>
    ///     Example membership component. Attach to a <c>GameObject</c> alongside
    ///     <see cref="ExampleFactionHolder"/> to enable faction tracking for that object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use <see cref="FactionAPI"/> to perform operations from external code:
    ///         <code>
    ///         FactionAPI.Join&lt;ExampleFaction, ExampleFactionHolder&gt;(membership);
    ///         FactionAPI.ChangeReputation&lt;ExampleFaction, ExampleFactionHolder&gt;(membership, 200);
    ///         </code>
    ///     </para>
    ///     <para>
    ///         Override <see cref="FactionMembershipBase{THolder}.GetHolder"/> if the holder component is on a different
    ///         <c>GameObject</c> or is not auto-discoverable via <c>GetComponent</c>.
    ///     </para>
    /// </remarks>
    public sealed class ExampleFactionMembership : FactionMembershipBase<ExampleFactionHolder>
    {
        // GetHolder() is inherited — default returns GetComponent<ExampleFactionHolder>()
        // which finds the sibling ExampleFactionHolder on the same GameObject.

        // Override member-level checks to add custom conditions:
        protected override OperationResult CanJoinFaction<TFaction>(
            in JoinFactionContext<ExampleFactionHolder> context)
        {
            // Example: allow join unconditionally (default behaviour).
            return base.CanJoinFaction<TFaction>(context);
        }

        // Override member-level events to add per-component reactions:
        protected override void OnJoinedFaction<TFaction>(
            in JoinFactionContext<ExampleFactionHolder> context,
            in OperationResult result)
        {
            // Delegate to the faction config first, then add custom logic.
            base.OnJoinedFaction<TFaction>(context, result);
            Debug.Log($"[ExampleFactionMembership] Joined faction on {gameObject.name}.");
        }
    }
}
