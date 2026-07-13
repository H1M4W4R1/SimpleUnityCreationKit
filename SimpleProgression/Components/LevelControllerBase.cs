using Systems.SimpleCore.Operations;
using Systems.SimpleProgression.Abstract;
using Systems.SimpleProgression.Operations;

namespace Systems.SimpleProgression.Components
{
    /// <summary>
    ///     Experience controller that derives a level from an overridable experience curve.
    /// </summary>
    public abstract class LevelControllerBase : ExperienceControllerBase, IWithLevel
    {
        /// <summary>
        ///     Current level. Levels start at zero and increase as thresholds are reached.
        /// </summary>
        protected int CurrentLevel { get; private set; }

        /// <summary>
        ///     Gets the maximum level, or -1 when the controller has no maximum.
        /// </summary>
        public virtual int GetMaxLevel()
        {
            return -1;
        }

        /// <summary>
        ///     Gets the current level derived from <see cref="Experience"/>.
        /// </summary>
        public int GetCurrentLevel()
        {
            return CurrentLevel;
        }

        /// <summary>
        ///     Increases the level by reaching the corresponding experience threshold.
        /// </summary>
        public OperationResult IncreaseLevel(int levelAmount)
        {
            if (levelAmount <= 0) return ProgressionOperations.InvalidLevelAmount();

            int maxLevel = GetMaxLevel();
            if (maxLevel >= 0 && CurrentLevel > maxLevel - levelAmount)
                return ProgressionOperations.MaxLevelReached();

            int targetLevel = CurrentLevel + levelAmount;
            ulong targetExperience = GetExperienceRequiredForLevel(targetLevel);
            if (targetExperience < Experience)
            {
                UpdateLevelFromExperience(Experience);
                return CurrentLevel >= targetLevel
                    ? ProgressionOperations.LevelIncreased()
                    : ProgressionOperations.InvalidLevelCurve();
            }

            ulong experienceToAdd = targetExperience - Experience;
            if (experienceToAdd == 0)
            {
                UpdateLevelFromExperience(targetExperience);
                return CurrentLevel >= targetLevel
                    ? ProgressionOperations.LevelIncreased()
                    : ProgressionOperations.InvalidLevelCurve();
            }

            OperationResult result = AddExperience(experienceToAdd);
            return result ? ProgressionOperations.LevelIncreased() : result;
        }

        /// <summary>
        ///     Alias for <see cref="IncreaseLevel"/>.
        /// </summary>
        public OperationResult AddLevel(int levelAmount)
        {
            return IncreaseLevel(levelAmount);
        }

        /// <summary>
        ///     Returns the experience required to reach a level.
        ///     The default curve requires one experience point per level.
        /// </summary>
        protected virtual ulong GetExperienceForLevel(int level)
        {
            return level <= 0 ? 0UL : (ulong)level;
        }

        /// <summary>
        ///     Extension point for non-linear progression curves.
        /// </summary>
        protected virtual ulong GetExperienceRequiredForLevel(int level)
        {
            return GetExperienceForLevel(level);
        }

        /// <summary>
        ///     Called once for every level crossed by an experience change.
        /// </summary>
        protected virtual void OnLevelIncreased(int newLevel) { }

        /// <summary>
        ///     Compatibility overload for integrations that do not need the new level value.
        /// </summary>
        protected virtual void OnLevelIncreased() { }

        /// <summary>
        ///     Called once after the level changes for an experience operation.
        /// </summary>
        protected virtual void OnLevelChanged(int previousLevel, int newLevel) { }

        /// <summary>
        ///     Called when a finite maximum level is reached.
        /// </summary>
        protected virtual void OnMaxLevelReached() { }

        protected internal override void OnExperienceChangedInternal(
            ulong previousExperience,
            ulong newExperience)
        {
            UpdateLevelFromExperience(newExperience);
        }

        private void UpdateLevelFromExperience(ulong experience)
        {
            int previousLevel = CurrentLevel;
            int newLevel = CalculateLevel(experience);
            if (newLevel == previousLevel) return;

            CurrentLevel = newLevel;
            if (newLevel > previousLevel)
            {
                for (int level = previousLevel + 1; level <= newLevel; level++)
                {
                    OnLevelIncreased(level);
                    OnLevelIncreased();
                }
            }

            OnLevelChanged(previousLevel, newLevel);
            int maxLevel = GetMaxLevel();
            if (maxLevel >= 0 && newLevel == maxLevel) OnMaxLevelReached();
        }

        private int CalculateLevel(ulong experience)
        {
            int maxLevel = GetMaxLevel();
            if (maxLevel == 0) return 0;

            int upperBound = maxLevel >= 0 ? maxLevel : 1;
            if (maxLevel < 0)
            {
                while (upperBound < int.MaxValue &&
                       GetExperienceRequiredForLevel(upperBound) <= experience)
                {
                    if (upperBound > int.MaxValue / 2)
                    {
                        upperBound = int.MaxValue;
                        break;
                    }

                    upperBound *= 2;
                }
            }

            int lowerBound = 0;
            while (lowerBound < upperBound)
            {
                int midpoint = lowerBound + (upperBound - lowerBound + 1) / 2;
                if (GetExperienceRequiredForLevel(midpoint) <= experience)
                    lowerBound = midpoint;
                else
                    upperBound = midpoint - 1;
            }

            return lowerBound;
        }
    }
}
