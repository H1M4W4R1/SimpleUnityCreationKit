using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleRelations.Abstract;

namespace Systems.SimpleRelations.Data
{
    /// <summary>Runtime database of identified objects that implement <see cref="IRelatable"/>.</summary>
    public sealed class RelatableObjectDatabase : RuntimeDatabase<RelatableObjectDatabase, IRelatable>
    {
#if UNITY_INCLUDE_TESTS
        internal static void ClearForTests()
        {
            Clear();
        }
#endif
    }
}
