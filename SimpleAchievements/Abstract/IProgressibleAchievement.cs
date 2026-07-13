namespace Systems.SimpleAchievements.Abstract
{
    /// <summary>
    ///     Implemented by achievements whose completion is driven by explicit gameplay progress notifications
    ///     rather than registry polling.
    /// </summary>
    /// <remarks>
    ///     <see cref="UpdateProgress"/> is called by <see cref="Utility.AchievementAPI.NotifyProgress"/> while
    ///     the achievement is still locked. Return <c>true</c> after applying the notification when the
    ///     achievement is ready to unlock.
    /// </remarks>
    public interface IProgressibleAchievement
    {
        /// <summary>
        ///     Applies one gameplay progress notification.
        /// </summary>
        /// <returns><c>true</c> when the achievement should unlock after this update.</returns>
        bool UpdateProgress();
    }
}
