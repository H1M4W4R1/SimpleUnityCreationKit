namespace Systems.SimpleIntegration.Abstract.Features
{
    /// <summary>
    ///     Contract implemented by integrations that can unlock external platform achievements.
    /// </summary>
    public interface IAchievementPlatform
    {
        /// <summary>Unlocks the achievement configured with <paramref name="achievementId"/>.</summary>
        void UnlockAchievement(string achievementId);
    }
}
