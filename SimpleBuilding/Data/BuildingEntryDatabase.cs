using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using System.Runtime.CompilerServices;
#endif
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleCore.Storage.Databases;

#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("SimpleBuilding.Tests")]
#endif

namespace Systems.SimpleBuilding.Data
{
    /// <summary>
    ///     Addressable database containing every generated building entry.
    /// </summary>
    public sealed class BuildingEntryDatabase : AddressableDatabase<BuildingEntryDatabase, BuildingEntryBase>
    {
        public const string LABEL = "SimpleBuilding.Buildings";
        [NotNull] protected override string AddressableLabel => LABEL;
    }
}
