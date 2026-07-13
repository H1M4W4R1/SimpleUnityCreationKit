using Systems.SimpleAchievements.Abstract;
using UnityEngine;

namespace Systems.SimpleAchievements.Examples.Achievements
{
    /// <summary>
    ///     Example of a condition-monitored achievement.
    ///     The registry polls <see cref="AchievementData.EvaluateCondition"/> every tick and
    ///     unlocks the achievement automatically once the counter reaches <c>_targetCount</c>.
    ///     Call <see cref="IncrementCount"/> from game code to drive the counter.
    /// </summary>
    [CreateAssetMenu(
        menuName = "SimpleAchievements/Examples/Conditional Achievement",
        fileName = "ExampleConditionalAchievement")]
    public sealed class ExampleConditionalAchievement : AchievementData
    {
        [SerializeField] private int _targetCount = 10;

        // Static counter for demonstration only.
        // In production, read from a stat or gameplay system instead.
        private static int _currentCount;

        public static int CurrentCount => _currentCount;
        public int TargetCount => _targetCount;

        /// <summary>Increments the internal counter used by the example condition.</summary>
        public static void IncrementCount() => _currentCount++;

        /// <summary>Resets the internal counter (e.g. on session start).</summary>
        public static void ResetCount() => _currentCount = 0;

        /// <inheritdoc />
        public override bool IsConditional => true;

        /// <inheritdoc />
        protected override bool EvaluateCondition() => _currentCount >= _targetCount;

        /// <inheritdoc />
        protected override void OnUnlocked() =>
            Debug.Log($"[Achievements] '{DisplayName}' condition met at count {_currentCount}.");
    }
}
