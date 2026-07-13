using Systems.SimpleCore.Operations;
using Systems.SimpleProgression.Abstract;
using Systems.SimpleProgression.Operations;
using UnityEngine;

namespace Systems.SimpleProgression.Components
{
    /// <summary>
    ///     Base component that stores and modifies a GameObject's experience.
    /// </summary>
    public abstract class ExperienceControllerBase : MonoBehaviour, IWithExperience
    {
        /// <summary>
        ///     Current experience value.
        /// </summary>
        public ulong Experience { get; private set; }

        /// <summary>
        ///     Returns whether this controller has at least the requested amount of experience.
        /// </summary>
        public bool HasExperience(ulong experienceAmount)
        {
            return Experience >= experienceAmount;
        }

        /// <summary>
        ///     Adds experience and notifies derived controllers and callbacks.
        /// </summary>
        public OperationResult AddExperience(ulong experienceAmount)
        {
            if (experienceAmount == 0) return ProgressionOperations.InvalidExperienceAmount();
            if (ulong.MaxValue - Experience < experienceAmount)
                return ProgressionOperations.ExperienceOverflow();

            ulong previousExperience = Experience;
            Experience += experienceAmount;
            NotifyExperienceChanged(previousExperience, Experience);
            OnExperienceAdded(experienceAmount);
            OnExperienceAdded();
            return ProgressionOperations.ExperienceAdded();
        }

        /// <summary>
        ///     Removes experience when the controller has enough experience.
        /// </summary>
        public OperationResult TakeExperience(ulong experienceAmount)
        {
            if (experienceAmount == 0) return ProgressionOperations.InvalidExperienceAmount();
            if (Experience < experienceAmount) return ProgressionOperations.NotEnoughExperience();

            ulong previousExperience = Experience;
            Experience -= experienceAmount;
            NotifyExperienceChanged(previousExperience, Experience);
            OnExperienceTaken(experienceAmount);
            OnExperienceTaken();
            return ProgressionOperations.ExperienceTaken();
        }

        /// <summary>
        ///     Alias for <see cref="AddExperience"/> for callers that prefer Try-style names.
        /// </summary>
        public OperationResult TryAddExperience(ulong experienceAmount)
        {
            return AddExperience(experienceAmount);
        }

        /// <summary>
        ///     Alias for <see cref="TakeExperience"/> for callers that prefer Try-style names.
        /// </summary>
        public OperationResult TryTakeExperience(ulong experienceAmount)
        {
            return TakeExperience(experienceAmount);
        }

        /// <summary>
        ///     Called after the experience value changes.
        /// </summary>
        protected virtual void OnExperienceChanged(ulong previousExperience, ulong newExperience) { }

        /// <summary>
        ///     Compatibility overload for integrations that only need a change notification.
        /// </summary>
        protected virtual void OnExperienceChanged() { }

        /// <summary>
        ///     Called after experience is added successfully.
        /// </summary>
        protected virtual void OnExperienceAdded(ulong experienceAmount) { }

        /// <summary>
        ///     Compatibility overload for integrations that only need an added notification.
        /// </summary>
        protected virtual void OnExperienceAdded() { }

        /// <summary>
        ///     Called after experience is taken successfully.
        /// </summary>
        protected virtual void OnExperienceTaken(ulong experienceAmount) { }

        /// <summary>
        ///     Compatibility overload for integrations that only need a taken notification.
        /// </summary>
        protected virtual void OnExperienceTaken() { }

        /// <summary>
        ///     Internal extension point used by level controllers before public experience callbacks.
        /// </summary>
        protected internal virtual void OnExperienceChangedInternal(
            ulong previousExperience,
            ulong newExperience)
        {
        }

        private void NotifyExperienceChanged(ulong previousExperience, ulong newExperience)
        {
            OnExperienceChangedInternal(previousExperience, newExperience);
            OnExperienceChanged(previousExperience, newExperience);
            OnExperienceChanged();
        }
    }
}
