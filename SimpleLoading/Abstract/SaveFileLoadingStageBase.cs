using JetBrains.Annotations;
using Systems.SimpleCore.Saving.Abstract;
using Systems.SimpleLoading.Data;

namespace Systems.SimpleLoading.Abstract
{
    /// <summary>Typed stage base for sequences whose user data is a specific save-file type.</summary>
    public abstract class SaveFileLoadingStageBase<TSaveFile> : LoadingStageBase
        where TSaveFile : SaveFileBase
    {
        /// <summary>Gets the typed save file supplied through <see cref="LoadingContext.userData"/>.</summary>
        protected static bool TryGetSaveFile(in LoadingContext context, [CanBeNull] out TSaveFile saveFile)
        {
            saveFile = context.userData as TSaveFile;
            return !ReferenceEquals(saveFile, null);
        }
    }
}
