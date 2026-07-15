using System;
using JetBrains.Annotations;

namespace Systems.SimpleSaving.Abstract
{
    /// <summary>Optional default save-file type for an <see cref="ISaveData"/> implementation.</summary>
    public interface IHasDefaultSaveFile
    {
        [CanBeNull] public Type DefaultSaveFileType { get; }
    }
}
