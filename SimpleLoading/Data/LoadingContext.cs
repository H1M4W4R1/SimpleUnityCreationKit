using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleLoading.Data
{
    /// <summary>Caller-owned data made available to every stage in one loading request.</summary>
    public readonly ref struct LoadingContext
    {
        /// <summary>The request being processed.</summary>
        public readonly LoadingHandle handle;

        /// <summary>Optional object being loaded for, commonly a player or save owner.</summary>
        [CanBeNull] public readonly Object target;

        /// <summary>Optional non-Unity data such as a save file or network response.</summary>
        [CanBeNull] public readonly object userData;

        internal LoadingContext(LoadingHandle handle, [CanBeNull] Object target, [CanBeNull] object userData)
        {
            this.handle = handle;
            this.target = target;
            this.userData = userData;
        }
    }
}
