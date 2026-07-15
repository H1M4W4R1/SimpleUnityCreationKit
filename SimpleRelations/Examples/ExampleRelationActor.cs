using System.Collections.Generic;
using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using UnityEngine;

namespace Systems.SimpleRelations.Examples
{
    /// <summary>Minimal relation owner used by the SimpleRelations example scene.</summary>
    public sealed class ExampleRelationActor : SimpleBehaviour, IRelatable, IIdentifiable<Snowflake128>
    {
        [SerializeField] private List<RelationEntry> _relationEntries = new();
        [SerializeField, HideInInspector] private Snowflake128 _identifier;

        /// <inheritdoc />
        public Snowflake128 Identifier
        {
            get => _identifier;
            set => _identifier = value;
        }

        List<RelationEntry> IRelatable.RelationEntries => _relationEntries;

    }
}
