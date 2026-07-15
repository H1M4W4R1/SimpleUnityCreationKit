using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using UnityEngine;

namespace Systems.SimpleRelations.Examples
{
    /// <summary>Shared change callback used by the relation types in the example scene.</summary>
    public abstract class ExampleRelationTypeBase : RelationTypeBase
    {
        /// <inheritdoc />
        protected override void OnRelationChanged(in RelationChangeContext context)
        {
            Component source = context.source as Component;
            Component target = context.target as Component;
            string sourceName = !ReferenceEquals(source, null) && source ? source.gameObject.name : "unknown";
            string targetName = !ReferenceEquals(target, null) && target ? target.gameObject.name : "unknown";
            Debug.Log(
                "[SimpleRelations] " + sourceName + " changed " + name +
                " toward " + targetName + ": " + context.previousValue +
                " -> " + context.newValue + ".");
        }
    }

    /// <summary>Example relation type with neutral trust starting at zero.</summary>
    public sealed class ExampleTrustRelation : ExampleRelationTypeBase { }
}
