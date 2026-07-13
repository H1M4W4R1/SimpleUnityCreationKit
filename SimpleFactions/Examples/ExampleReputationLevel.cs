using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Interfaces;
using UnityEngine;

namespace Systems.SimpleFactions.Examples
{
    /// <summary>
    ///     Example reputation level that is automatically assigned to <see cref="ExampleFaction"/>
    ///     via <see cref="IForFaction{TFaction}"/>. The <c>FactionLevelAssigner</c> editor
    ///     postprocessor discovers this type on script reload and wires it into
    ///     <c>ExampleFaction._levels</c>.
    /// </summary>
    /// <remarks>
    ///     Configure <c>AutomaticPromotion</c>, <c>PromotionThreshold</c>,
    ///     <c>AutomaticDemotion</c>, and <c>DemotionThreshold</c> in the Inspector on the
    ///     generated asset at <c>Assets/Generated/ReputationLevels/ExampleReputationLevel.asset</c>.
    /// </remarks>
    public sealed class ExampleReputationLevel : ReputationLevelBase, IForFaction<ExampleFaction>
    {
        protected internal override void OnLevelAchieved(
            in FactionLevelChangeContext context,
            in OperationResult result)
        {
            Debug.Log($"[ExampleReputationLevel] Level achieved: {name}.");
        }

        protected internal override void OnLevelIncreased(
            in FactionLevelChangeContext context,
            in OperationResult result)
        {
            Debug.Log($"[ExampleReputationLevel] Promoted to: {name}.");
        }

        protected internal override void OnLevelDecreased(
            in FactionLevelChangeContext context,
            in OperationResult result)
        {
            Debug.Log($"[ExampleReputationLevel] Demoted to: {name}.");
        }

        protected internal override void OnLevelChanged(
            in FactionLevelChangeContext context,
            in OperationResult result)
        {
            Debug.Log($"[ExampleReputationLevel] Active level changed to: {name}.");
        }
    }
}
