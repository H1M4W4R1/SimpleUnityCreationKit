using Systems.SimpleRelations.Components;
using Systems.SimpleRelations.Data;
using UnityEngine;

namespace Systems.SimpleRelations.Examples
{
    /// <summary>Minimal relation owner used by the SimpleRelations example scene.</summary>
    public sealed class ExampleRelationActor : RelationComponentBase
    {
        /// <inheritdoc />
        protected override void OnRelationChanged(in RelationChangeContext context)
        {
            RelationComponentBase target = context.target as RelationComponentBase;
            string targetName = !ReferenceEquals(target, null) && target ? target.gameObject.name : "unknown";
            Debug.Log(
                "[SimpleRelations] " + gameObject.name + " changed " + context.relationType.name +
                " toward " + targetName + ": " + context.previousValue +
                " -> " + context.newValue + ".");
        }
    }
}
