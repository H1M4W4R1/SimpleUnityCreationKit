using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Abstract.Markers;
using Systems.SimpleQuests.Data;
using Systems.SimpleQuests.Data.Enums;
using UnityEngine;

namespace Systems.SimpleQuests.Objectives
{
    /// <summary>
    ///     Objective consisting of multiple objectives to complete that
    ///     are activated at same time
    /// </summary>
    [UsedImplicitly] public sealed class CombinedQuestObjective : QuestObjective, IWithObjectives<CombinedQuestObjective>
    {
        /// <summary>
        ///     List of objectives that need to be completed
        /// </summary>
        private readonly List<QuestObjective> _objectives = new();
        
        /// <summary>
        ///     Access to list of objectives that need to be completed
        /// </summary>
        public IReadOnlyList<QuestObjective> Objectives => _objectives;

        /// <summary>
        ///     Adds an objective to the list
        /// </summary>
        [NotNull] public CombinedQuestObjective WithObjective([CanBeNull] QuestObjective objective)
        {
            if (ReferenceEquals(objective, null))
            {
                Debug.LogError("Trying to add null objective to combined objective");
                return this;
            }
            _objectives.Add(objective);
            return this;
        }

        /// <summary>
        ///     Activates all child objectives simultaneously when the combined objective starts
        /// </summary>
        protected internal override void OnQuestObjectiveStarted(QuestInstance quest)
        {
            base.OnQuestObjectiveStarted(quest);

            for (int i = 0; i < _objectives.Count; i++)
            {
                QuestObjective objective = _objectives[i];
                objective.State = QuestState.InProgress;
                objective.OnQuestObjectiveStarted(quest);
            }
        }

        /// <summary>
        ///     Checks if all required objectives are completed
        /// </summary>
        public override bool ShouldBeComplete()
        {
            for (int i = 0; i < _objectives.Count; i++)
            {
                QuestObjective objective = _objectives[i];
                if (!objective.IsRequired) continue;
                if (objective.State != QuestState.Completed) return false;
            }

            return true;
        }

        /// <summary>
        ///     Checks if any required objective is failed
        /// </summary>
        public override bool ShouldBeFailed()
        {
            for (int i = 0; i < _objectives.Count; i++)
            {
                QuestObjective objective = _objectives[i];
                if (!objective.IsRequired) continue;
                if (objective.State == QuestState.Failed) return true;
            }

            return false;
        }

        /// <summary>
        ///     No-op: child objectives of a CombinedQuestObjective are all activated simultaneously
        ///     when the parent starts. Sequential activation is handled by QuestInstance, not here.
        /// </summary>
        void IWithObjectives.AfterQuestIterationComplete()
        {
        }
    }
}