using System;
using JetBrains.Annotations;
using Systems.SimpleSaving.Abstract;
using UnityEngine;

namespace Systems.SimpleBuilding.Data.SaveFiles
{
    /// <summary>
    ///     Serializable snapshot of API-placed buildings in one loaded world.
    /// </summary>
    [Serializable]
    public sealed class BuildingSaveFile : SaveFileBase
    {
        [NotNull] public SavedBuilding[] Buildings = Array.Empty<SavedBuilding>();

        /// <summary>
        ///     Serializable placement and slot-reservation state for one building.
        /// </summary>
        [Serializable]
        public sealed class SavedBuilding
        {
            [NotNull] public string EntryIdentifier = string.Empty;
            public Vector3 Position;
            public Quaternion Rotation = Quaternion.identity;
            public Vector3 LocalScale = Vector3.one;
            [NotNull] public string[] SlotIdentifiers = Array.Empty<string>();
        }
    }
}
