using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Components;
using UnityEngine;

namespace Systems.SimpleRelations.Data
{
    /// <summary>
    ///     Serialized state for one outgoing relationship. Its target is stored as a strongly typed
    ///     relation component; arbitrary Unity objects are not valid.
    /// </summary>
    [System.Serializable]
    public sealed class RelationEntry : IRelation
    {
        [SerializeField] private RelationTypeBase _relationType;
        [SerializeField] private RelationComponentBase _target;
        [SerializeField] private int _value;

        /// <summary>Configured type of this relationship.</summary>
        public RelationTypeBase RelationType => _relationType;

        /// <summary>Relation component targeted by this entry.</summary>
        public IRelatable Target
        {
            get
            {
                return !ReferenceEquals(_target, null) && _target ? _target : null;
            }
        }

        /// <inheritdoc />
        public int Value => _value;

        internal RelationEntry(RelationTypeBase relationType, IRelatable target, int value)
        {
            _relationType = relationType;
            _target = target as RelationComponentBase;
            _value = value;
        }

        internal bool Matches(RelationTypeBase relationType, IRelatable target)
        {
            if (!ReferenceEquals(_relationType, relationType)) return false;

            return target is RelationComponentBase componentTarget && ReferenceEquals(_target, componentTarget);
        }

        internal static bool IsSupportedRelatable(IRelatable relatable)
        {
            return relatable is RelationComponentBase componentTarget &&
                   !ReferenceEquals(componentTarget, null) && componentTarget;
        }

        internal void SetValue(int value)
        {
            _value = value;
        }
    }
}
