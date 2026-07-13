using Systems.SimpleAchievements.Abstract;
using UnityEngine;

namespace Systems.SimpleAchievements.Examples.Achievements
{
    /// <summary>
    ///     Example of a manually triggered achievement.
    ///     <see cref="AchievementData.IsConditional"/> is <c>false</c> (default) so the registry
    ///     never polls it. Unlock it explicitly by calling:
    ///     <code>
    ///     AchievementAPI.Unlock(new AchievementUnlockContext(myAchievementAsset));
    ///     </code>
    /// </summary>
    [CreateAssetMenu(
        menuName = "SimpleAchievements/Examples/Manual Achievement",
        fileName = "ExampleManualAchievement")]
    public sealed class ExampleManualAchievement : AchievementData
    {
        /// <inheritdoc />
        protected override void OnUnlocked() =>
            Debug.Log($"[Achievements] '{DisplayName}' manually unlocked.");
    }
}
