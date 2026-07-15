using JetBrains.Annotations;

namespace Systems.SimpleSaving.Abstract.Transitions
{
    /// <summary>Indicates a save file type can be converted from a newer version.</summary>
    public interface IDowngradableSaveFile<out TToFile, in TFromFile>
        where TToFile : SaveFileBase
        where TFromFile : SaveFileBase
    {
        TToFile GetDowngradedVersion([NotNull] TFromFile originalFile);
    }
}
