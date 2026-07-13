using JetBrains.Annotations;
using Systems.SimpleAchievements.Data.Databases;
using Systems.SimpleAchievements.Operations;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleAchievements.Utility;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using UnityEngine;

namespace Systems.SimpleAchievements.Abstract
{
    /// <summary>
    ///     Base ScriptableObject for all achievements. Subclass to define either a manually triggered
    ///     achievement (override nothing, call <see cref="AchievementAPI.Unlock"/> explicitly) or a
    ///     condition-monitored achievement (set <see cref="IsConditional"/> to <c>true</c> and override
    ///     <see cref="EvaluateCondition"/>).
    /// </summary>
    /// <remarks>
    ///     Concrete subclasses inherit the <see cref="AutoCreateAttribute"/> and are automatically
    ///     registered in <see cref="AchievementDatabase"/> via the Addressables label
    ///     <see cref="AchievementDatabase.LABEL"/>.
    /// </remarks>
    [AutoCreate("Achievements", AchievementDatabase.LABEL)]
    public abstract class AchievementData : ScriptableObject
    {
        [SerializeField] private string _platformId;
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;

        /// <summary>
        ///     Identifier used by external platform SDKs (Steam, Epic, etc.).
        ///     Must match the achievement ID configured on each platform exactly,
        ///     e.g. <c>"ACH_WIN_ONE_GAME"</c>.
        /// </summary>
        [NotNull] public string PlatformId => _platformId;

        /// <summary>Display name shown to the player.</summary>
        [NotNull] public string DisplayName => _displayName;

        /// <summary>Description shown to the player.</summary>
        [NotNull] public string Description => _description;

        /// <summary>
        ///     When <c>true</c> the registry polls <see cref="EvaluateCondition"/> every tick.
        ///     Override and return <c>true</c> to enable automatic condition monitoring.
        ///     Defaults to <c>false</c> for manually triggered achievements.
        /// </summary>
        public virtual bool IsConditional => false;

        /// <summary>
        ///     Evaluates whether this achievement's unlock condition is currently satisfied.
        ///     Called by the registry each tick when <see cref="IsConditional"/> is <c>true</c>.
        ///     Override to implement the condition logic.
        /// </summary>
        /// <returns><c>true</c> when the achievement should unlock.</returns>
        protected virtual bool EvaluateCondition() => false;

        /// <summary>
        ///     Validates whether the achievement may be unlocked through
        ///     <see cref="AchievementAPI.Unlock"/>. Override to add gameplay-specific gates.
        /// </summary>
        /// <param name="context">Unlock context supplied by the caller.</param>
        /// <returns>An operation result describing whether the unlock is permitted.</returns>
        protected virtual OperationResult CanUnlock(in AchievementUnlockContext context)
        {
            if (string.IsNullOrWhiteSpace(_platformId)) return AchievementOperations.InvalidAchievement();
            if (!IsConditional) return AchievementOperations.Permitted();
            if (context.ForceUnlock) return AchievementOperations.Permitted();
            return EvaluateCondition()
                ? AchievementOperations.Permitted()
                : AchievementOperations.ConditionNotMet();
        }

        /// <summary>
        ///     Called once immediately after the unlock state is recorded in the registry.
        ///     Override for one-time side effects such as analytics events, VFX, or UI notifications.
        ///     Not called when restoring state from a save file.
        /// </summary>
        protected virtual void OnUnlocked() { }

        // Internal surface: only AchievementRegistry may invoke these.
        internal OperationResult CanUnlockInternal(in AchievementUnlockContext context) =>
            CanUnlock(in context);

        internal bool CheckCondition() =>
            !string.IsNullOrWhiteSpace(_platformId) && EvaluateCondition();

        internal void NotifyUnlocked() => OnUnlocked();
    }
}
