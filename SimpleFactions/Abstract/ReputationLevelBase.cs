using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Data.Context;
using Systems.SimpleFactions.Interfaces;
using Systems.SimpleFactions.Operations;
using UnityEngine;

namespace Systems.SimpleFactions.Abstract
{
    /// <summary>
    ///     Defines a reputation tier within a faction. Concrete sealed subclasses are
    ///     auto-created in <c>Assets/Generated/ReputationLevels/</c> and registered in
    ///     <see cref="ReputationLevelDatabase"/> via the <c>AutoCreate</c> attribute.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implement <see cref="IForFaction{TFaction}"/> on your concrete subclass to have it
    ///         automatically assigned to its faction's level list on script reload. Without that
    ///         interface the level must be added manually in the Inspector.
    ///     </para>
    ///     <para>
    ///         Levels in <see cref="FactionBase.Levels"/> are ordered by
    ///         <see cref="PromotionThreshold"/> ascending (index 0 = lowest rank). The ordering is
    ///         maintained automatically by the editor postprocessor and by
    ///         <see cref="FactionBase.AssignLevel"/>.
    ///     </para>
    ///     <para>
    ///         <b>Automatic promotion</b> and <b>automatic demotion</b> are independent flags, so
    ///         a level can be manually granted by a game event (e.g. a king promotes a player to
    ///         knight) while still being automatically revoked if reputation falls below
    ///         <see cref="DemotionThreshold"/>.
    ///     </para>
    /// </remarks>
    [AutoCreate("ReputationLevels", ReputationLevelDatabase.LABEL)]
    public abstract class ReputationLevelBase : ScriptableObject
    {
        [SerializeField] private bool _automaticPromotion;
        [SerializeField] private long _promotionThreshold;
        [SerializeField] private bool _automaticDemotion;
        [SerializeField] private long _demotionThreshold;

        /// <summary>
        ///     When <c>true</c>, this level is automatically granted when a member's reputation
        ///     reaches or exceeds <see cref="PromotionThreshold"/>.
        /// </summary>
        public bool AutomaticPromotion => _automaticPromotion;

        /// <summary>
        ///     Reputation value at or above which automatic promotion to this level is triggered.
        ///     Only evaluated when <see cref="AutomaticPromotion"/> is <c>true</c>.
        /// </summary>
        public long PromotionThreshold => _promotionThreshold;

        /// <summary>
        ///     When <c>true</c>, this level is automatically revoked when a member's reputation
        ///     falls below <see cref="DemotionThreshold"/>.
        ///     This flag is evaluated independently of <see cref="AutomaticPromotion"/>, so a
        ///     manually granted level can still be automatically removed.
        /// </summary>
        public bool AutomaticDemotion => _automaticDemotion;

        /// <summary>
        ///     Reputation value below which automatic demotion from this level is triggered.
        ///     Only evaluated when <see cref="AutomaticDemotion"/> is <c>true</c>.
        /// </summary>
        public long DemotionThreshold => _demotionThreshold;

        #region Checks

        /// <summary>
        ///     Determines whether a member may be promoted <b>to</b> this level.
        ///     Called during automatic promotion before the level is assigned.
        ///     Override to add prerequisites or cooldowns.
        /// </summary>
        protected internal virtual OperationResult CanPromoteTo(in FactionLevelChangeContext context)
            => FactionOperations.Permitted();

        /// <summary>
        ///     Determines whether a member may be demoted <b>from</b> this level.
        ///     Called when this is the <em>current</em> level and an automatic demotion is triggered.
        ///     Override to protect a level from automatic removal (e.g., a quest lock).
        /// </summary>
        protected internal virtual OperationResult CanDemoteFrom(in FactionLevelChangeContext context)
            => FactionOperations.Permitted();

        /// <summary>
        ///     Determines whether a member may be demoted <b>to</b> this level.
        ///     Called when this is the <em>target</em> level of an automatic demotion.
        ///     Override to skip this level as a demotion target.
        /// </summary>
        protected internal virtual OperationResult CanDemoteTo(in FactionLevelChangeContext context)
            => FactionOperations.Permitted();

        #endregion

        #region Events

        /// <summary>
        ///     Called on the <b>new</b> level when a member's level increases to this rank
        ///     (promotion). Not called on demotion.
        /// </summary>
        protected internal virtual void OnLevelAchieved(in FactionLevelChangeContext context, in OperationResult result) { }

        /// <summary>
        ///     Called on the <b>new</b> level when the level transition is a promotion
        ///     (new index &gt; previous index).
        /// </summary>
        protected internal virtual void OnLevelIncreased(in FactionLevelChangeContext context, in OperationResult result) { }

        /// <summary>
        ///     Called on the <b>new</b> level when the level transition is a demotion
        ///     (new index &lt; previous index).
        /// </summary>
        protected internal virtual void OnLevelDecreased(in FactionLevelChangeContext context, in OperationResult result) { }

        /// <summary>
        ///     Called on the <b>new</b> level whenever the active reputation level changes,
        ///     regardless of direction.
        /// </summary>
        protected internal virtual void OnLevelChanged(in FactionLevelChangeContext context, in OperationResult result) { }

        #endregion
    }
}
