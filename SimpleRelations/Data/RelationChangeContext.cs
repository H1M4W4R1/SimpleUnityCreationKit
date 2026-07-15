using Systems.SimpleRelations.Abstract;

namespace Systems.SimpleRelations.Data
{
    /// <summary>Resolved request to add a signed amount to an outgoing relation.</summary>
    public readonly ref struct RelationChangeContext
    {
        /// <summary>Resolved relation type asset.</summary>
        public readonly RelationTypeBase relationType;

        /// <summary>Object receiving the one-way relationship.</summary>
        public readonly IRelatable target;

        /// <summary>Signed amount requested by the caller.</summary>
        public readonly int amountRequested;

        /// <summary>Value before the operation. It is valid for component callbacks only.</summary>
        public readonly int previousValue;

        /// <summary>Value after the operation. It is valid for component callbacks only.</summary>
        public readonly int newValue;

        /// <summary>Creates a resolved change request for direct component use.</summary>
        public RelationChangeContext(RelationTypeBase relationType, IRelatable target, int amountRequested)
        {
            this.relationType = relationType;
            this.target = target;
            this.amountRequested = amountRequested;
            previousValue = 0;
            newValue = 0;
        }

        internal RelationChangeContext(
            RelationTypeBase relationType,
            IRelatable target,
            int amountRequested,
            int previousValue,
            int newValue)
        {
            this.relationType = relationType;
            this.target = target;
            this.amountRequested = amountRequested;
            this.previousValue = previousValue;
            this.newValue = newValue;
        }
    }

    /// <summary>Resolved request to assign an exact outgoing relation value.</summary>
    public readonly ref struct RelationSetContext
    {
        /// <summary>Resolved relation type asset.</summary>
        public readonly RelationTypeBase relationType;

        /// <summary>Object receiving the one-way relationship.</summary>
        public readonly IRelatable target;

        /// <summary>Value requested by the caller and stored after a successful operation.</summary>
        public readonly int value;

        /// <summary>Value before the operation. It is valid for component callbacks only.</summary>
        public readonly int previousValue;

        /// <summary>Creates a resolved set request for direct component use.</summary>
        public RelationSetContext(RelationTypeBase relationType, IRelatable target, int value)
        {
            this.relationType = relationType;
            this.target = target;
            this.value = value;
            previousValue = 0;
        }

        internal RelationSetContext(
            RelationTypeBase relationType,
            IRelatable target,
            int value,
            int previousValue)
        {
            this.relationType = relationType;
            this.target = target;
            this.value = value;
            this.previousValue = previousValue;
        }
    }

    /// <summary>Caller-owned typed request for a relation change through <c>RelationAPI</c>.</summary>
    /// <typeparam name="TRelationType">Configured relation type resolved by the static API.</typeparam>
    public readonly ref struct RelationChangeContext<TRelationType>
        where TRelationType : RelationTypeBase, new()
    {
        /// <summary>Relatable that owns the outgoing relationship.</summary>
        public readonly IRelatable source;

        /// <summary>Relatable that receives the one-way relationship.</summary>
        public readonly IRelatable target;

        /// <summary>Signed amount to add.</summary>
        public readonly int amount;

        public RelationChangeContext(IRelatable source, IRelatable target, int amount)
        {
            this.source = source;
            this.target = target;
            this.amount = amount;
        }
    }

    /// <summary>Caller-owned typed request for assigning a relation value through <c>RelationAPI</c>.</summary>
    /// <typeparam name="TRelationType">Configured relation type resolved by the static API.</typeparam>
    public readonly ref struct RelationSetContext<TRelationType>
        where TRelationType : RelationTypeBase, new()
    {
        /// <summary>Relatable that owns the outgoing relationship.</summary>
        public readonly IRelatable source;

        /// <summary>Relatable that receives the one-way relationship.</summary>
        public readonly IRelatable target;

        /// <summary>Exact value to assign.</summary>
        public readonly int value;

        public RelationSetContext(IRelatable source, IRelatable target, int value)
        {
            this.source = source;
            this.target = target;
            this.value = value;
        }
    }

    /// <summary>Caller-owned typed request for querying a relation through <c>RelationAPI</c>.</summary>
    /// <typeparam name="TRelationType">Configured relation type resolved by the static API.</typeparam>
    public readonly ref struct RelationQueryContext<TRelationType>
        where TRelationType : RelationTypeBase, new()
    {
        /// <summary>Relatable whose outgoing value is queried.</summary>
        public readonly IRelatable source;

        /// <summary>Relatable that receives the one-way relationship.</summary>
        public readonly IRelatable target;

        public RelationQueryContext(IRelatable source, IRelatable target)
        {
            this.source = source;
            this.target = target;
        }
    }
}
