using System.Collections.Generic;
using Systems.SimpleRelations.Abstract;
using UnityEngine;

namespace Systems.SimpleRelations.Data
{
    /// <summary>Serialized entry collection owned by a relatable component.</summary>
    [System.Serializable]
    public sealed class RelationStorage
    {
        [SerializeField] private List<RelationEntry> _entries = new();

        /// <summary>Serialized outgoing entries in insertion order.</summary>
        public IReadOnlyList<RelationEntry> Entries => _entries;

        internal int GetValue(RelationTypeBase relationType, IRelatable target)
        {
            if (ReferenceEquals(relationType, null) || !relationType) return 0;
            if (!TryGet(relationType, target, out RelationEntry relation))
                return relationType.InitialValue;

            return relation.Value;
        }

        internal bool TryGet(RelationTypeBase relationType, IRelatable target, out RelationEntry relation)
        {
            relation = null;
            if (ReferenceEquals(relationType, null) || !relationType)
                return false;
            if (!RelationEntry.IsSupportedRelatable(target))
                return false;

            for (int index = 0; index < _entries.Count; index++)
            {
                RelationEntry current = _entries[index];
                if (ReferenceEquals(current, null)) continue;
                if (!current.Matches(relationType, target)) continue;

                relation = current;
                return true;
            }

            return false;
        }

        internal void SetValue(RelationTypeBase relationType, IRelatable target, int previousValue, int newValue)
        {
            if (TryGet(relationType, target, out RelationEntry relation))
            {
                relation.SetValue(newValue);
                return;
            }

            RelationEntry newRelation = new RelationEntry(relationType, target, previousValue);
            newRelation.SetValue(newValue);
            _entries.Add(newRelation);
        }
    }
}
