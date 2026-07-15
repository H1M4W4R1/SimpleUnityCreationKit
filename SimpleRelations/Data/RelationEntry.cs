using JetBrains.Annotations;
using Systems.SimpleRelations.Abstract;
using UnityEngine;

namespace Systems.SimpleRelations.Data
{
    /// <summary>
    ///     Serialized state for one outgoing relationship. Targets must be Unity objects that implement
    ///     <see cref="IRelatable"/> so Unity can persist the object reference.
    /// </summary>
    [System.Serializable]
    public sealed class RelationEntry : IRelation
    {
        [SerializeField] private RelationTypeBase _relationType;
        [SerializeField] private Object _target;
        [SerializeField] private int _value;

        /// <summary>Configured type of this relationship.</summary>
        public RelationTypeBase RelationType => _relationType;

        /// <summary>Relatable Unity object targeted by this entry.</summary>
        [CanBeNull] public IRelatable Target
        {
            get
            {
                if (ReferenceEquals(_target, null) || !_target) return null;
                return _target as IRelatable;
            }
        }

        /// <inheritdoc />
        public int Value => _value;

        internal RelationEntry(RelationTypeBase relationType, IRelatable target, int value)
        {
            _relationType = relationType;
            _target = target as Object;
            _value = value;
        }

        internal bool Matches(RelationTypeBase relationType, IRelatable target)
        {
            if (!ReferenceEquals(_relationType, relationType)) return false;

            Object targetObject = target as Object;
            return !ReferenceEquals(targetObject, null) && ReferenceEquals(_target, targetObject);
        }

        internal static bool IsSupportedRelatable(IRelatable relatable)
        {
            Object relatableObject = relatable as Object;
            return !ReferenceEquals(relatableObject, null) && relatableObject;
        }

        internal void SetValue(int value)
        {
            _value = value;
        }
    }
}
