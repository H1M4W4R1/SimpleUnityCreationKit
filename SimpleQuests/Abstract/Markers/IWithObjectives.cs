using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleQuests.Data;
using Systems.SimpleQuests.Data.Enums;

namespace Systems.SimpleQuests.Abstract.Markers
{
    /// <summary>
    ///     Represents an object that has objectives
    /// </summary>
    public interface IWithObjectives<out TSelf> : IWithObjectives
        where TSelf : IWithObjectives<TSelf>
    {
        /// <summary>
        ///     Adds an objective to the list
        /// </summary>
        [UsedImplicitly] public TSelf WithObjective([NotNull] QuestObjective objective);
    }

    public interface IWithObjectives
    {
        /// <summary>
        ///     State of this object
        /// </summary>
        public QuestState State { get; }

        /// <summary>
        ///     List of all objectives
        /// </summary>
        public IReadOnlyList<QuestObjective> Objectives { get; }

        /// <summary>
        ///     Method that is executed after the iteration of objectives is complete
        ///     used to execute additional events on quest instance such as completion status.
        /// </summary>
        protected void AfterQuestIterationComplete();

        protected internal void TickCompletionStatusCheck([NotNull] QuestInstance questInstance, float deltaTime)
        {
            // Only handle objectives that are in progress
            if (State != QuestState.InProgress) return;

            // Handle optional objectives
            for (int i = 0; i < Objectives.Count; i++)
            {
                QuestObjective objective = Objectives[i];
                if (objective.IsRequired) continue;

                VerifyObjectiveCompletionStatus(questInstance, objective, deltaTime);
            }

            // Handle required objectives
            for (int i = 0; i < Objectives.Count; i++)
            {
                QuestObjective objective = Objectives[i];
                if (!objective.IsRequired) continue;

                VerifyObjectiveCompletionStatus(questInstance, objective, deltaTime);
            }

            // Iteration was completed
            AfterQuestIterationComplete();
        }

        /// <summary>
        ///     Handles the completion status of an objective of specified quest instance
        /// </summary>
        private void VerifyObjectiveCompletionStatus(
            [NotNull] QuestInstance questInstance,
            [NotNull] QuestObjective objective,
            float deltaTime)
        {
            // Ensure objective is in progress
            if (objective.State != QuestState.InProgress) return;
            
            // Handle objective tick
            objective.OnQuestObjectiveTick(questInstance, deltaTime);
            
            // Handle cascade of objectives
            if (objective is IWithObjectives withObjectives)
                withObjectives.TickCompletionStatusCheck(questInstance, deltaTime);

            // Handle failure first - failure takes priority over completion
            if (objective.ShouldBeFailed())
            {
                objective.State = QuestState.Failed;
                objective.OnQuestObjectiveFailed(questInstance);
            }
            else if (objective.ShouldBeComplete())
            {
                objective.State = QuestState.Completed;
                objective.OnQuestObjectiveCompleted(questInstance);
            }
        }
    }
}