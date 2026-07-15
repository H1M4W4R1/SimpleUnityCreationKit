using System.Collections.Generic;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using UnityEngine;

namespace Systems.SimpleRelations.Examples
{
    /// <summary>Minimal relation owner used by the SimpleRelations example scene.</summary>
    public sealed class ExampleRelationActor : MonoBehaviour, IRelatable
    {
        [SerializeField] private List<RelationEntry> _relationEntries = new();

        List<RelationEntry> IRelatable.RelationEntries => _relationEntries;
    }
}
