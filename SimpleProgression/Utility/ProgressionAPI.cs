using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleProgression.Components;
using Systems.SimpleProgression.Operations;
using UnityEngine;

namespace Systems.SimpleProgression.Utility
{
    /// <summary>
    ///     Static facade for modifying progression components attached to GameObjects.
    /// </summary>
    public static class ProgressionAPI
    {
        /// <summary>
        ///     Adds experience to the first <see cref="ExperienceControllerBase"/> on a GameObject.
        /// </summary>
        public static OperationResult AddExperience(
            [CanBeNull] GameObject gameObject,
            ulong experienceAmount)
        {
            if (!IsValidGameObject(gameObject)) return ProgressionOperations.InvalidGameObject();

            if (!gameObject.TryGetComponent<ExperienceControllerBase>(
                    out ExperienceControllerBase controller))
                return ProgressionOperations.ExperienceControllerNotFound();

            return controller.AddExperience(experienceAmount);
        }

        /// <summary>
        ///     Alias for <see cref="AddExperience"/>.
        /// </summary>
        public static OperationResult IncreaseExperience(
            [CanBeNull] GameObject gameObject,
            ulong experienceAmount)
        {
            return AddExperience(gameObject, experienceAmount);
        }

        /// <summary>
        ///     Takes experience from the first <see cref="ExperienceControllerBase"/> on a GameObject.
        /// </summary>
        public static OperationResult TakeExperience(
            [CanBeNull] GameObject gameObject,
            ulong experienceAmount)
        {
            if (!IsValidGameObject(gameObject)) return ProgressionOperations.InvalidGameObject();

            if (!gameObject.TryGetComponent<ExperienceControllerBase>(
                    out ExperienceControllerBase controller))
                return ProgressionOperations.ExperienceControllerNotFound();

            return controller.TakeExperience(experienceAmount);
        }

        /// <summary>
        ///     Increases the level of the first <see cref="LevelControllerBase"/> on a GameObject.
        /// </summary>
        public static OperationResult IncreaseLevel(
            [CanBeNull] GameObject gameObject,
            int levelAmount)
        {
            if (!IsValidGameObject(gameObject)) return ProgressionOperations.InvalidGameObject();

            if (!gameObject.TryGetComponent<LevelControllerBase>(
                    out LevelControllerBase controller))
                return ProgressionOperations.LevelControllerNotFound();

            return controller.IncreaseLevel(levelAmount);
        }

        /// <summary>
        ///     Alias for <see cref="IncreaseLevel"/>.
        /// </summary>
        public static OperationResult AddLevel(
            [CanBeNull] GameObject gameObject,
            int levelAmount)
        {
            return IncreaseLevel(gameObject, levelAmount);
        }

        private static bool IsValidGameObject([CanBeNull] GameObject gameObject)
        {
            return !ReferenceEquals(gameObject, null) && gameObject;
        }
    }
}
