namespace Systems.SimpleRelations.Data
{
    /// <summary>Read-only view shared by relation entries.</summary>
    public interface IRelation
    {
        /// <summary>Current numeric value of the relation.</summary>
        int Value { get; }
    }
}
