using System;
using JetBrains.Annotations;
using Systems.SimpleQuests.Data;
using Systems.SimpleQuests.Data.Enums;

namespace Systems.SimpleQuests.Abstract
{
    /// <summary>
    ///     Objective of a quest - simple thing to complete
    /// </summary>
    [Serializable] public abstract class QuestObjective
    {
        /// <summary>
        ///     State of this objective
        /// </summary>
        public QuestState State { get; internal set; } = QuestState.Inactive;

        /// <summary>
        ///     Checks if the objective is required to complete the quest
        /// </summary>
        public virtual bool IsRequired => true;

        /// <summary>
        ///     Check if the objective is completed
        /// </summary>
        public abstract bool ShouldBeComplete();

        /// <summary>
        ///     Checks if the objective is failed
        /// </summary>
        public virtual bool ShouldBeFailed() => false;

        /// <summary>
        ///     Event that is called when objective is started
        /// </summary>
        protected internal virtual void OnQuestObjectiveStarted([NotNull] QuestInstance quest)
        {
        }

        /// <summary>
        ///     Event that is called when objective is completed
        /// </summary>
        protected internal virtual void OnQuestObjectiveCompleted([NotNull] QuestInstance quest)
        {
        }

        /// <summary>
        ///     Event that is called when objective is failed
        /// </summary>
        protected internal virtual void OnQuestObjectiveFailed([NotNull] QuestInstance quest)
        {
        }

        /// <summary>
        ///     Event that is called every in-game tick
        /// </summary>
        protected internal virtual void OnQuestObjectiveTick(QuestInstance questInstance, float deltaTime)
        {
        }
    }
}