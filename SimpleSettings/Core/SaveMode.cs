namespace Systems.SimpleSettings.Core
{
    /// <summary>
    ///     Controls how setting groups are persisted to disk.
    /// </summary>
    public enum SaveMode
    {
        /// <summary>
        ///     All groups are merged into one JSON file
        ///     (named by <c>SettingsManager._sharedFileName</c>).
        /// </summary>
        SingleFile,

        /// <summary>
        ///     Each group is saved to its own file
        ///     (named by <c>SettingGroupBase.SaveFileName</c>).
        /// </summary>
        PerGroup,
    }
}
