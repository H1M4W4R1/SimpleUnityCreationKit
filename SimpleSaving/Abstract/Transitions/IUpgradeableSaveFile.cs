using JetBrains.Annotations;

namespace Systems.SimpleSaving.Abstract.Transitions
{
    /// <summary>Indicates a save file type can be converted to a newer version.</summary>
    public interface IUpgradeableSaveFile<out TToFile, in TFromFile>
        where TToFile : SaveFileBase
        where TFromFile : SaveFileBase
    {
        TToFile GetUpgradedVersion([NotNull] TFromFile originalFile);
    }
}
