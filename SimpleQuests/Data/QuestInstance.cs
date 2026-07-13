using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Abstract.Markers;
using Systems.SimpleQuests.Data.Enums;
using UnityEngine;

namespace Systems.SimpleQuests.Data
{
    /// <summary>
    ///     Instance of quest created from quest object
    /// </summary>
    public sealed class QuestInstance : IWithObjectives<QuestInstance>
    {
        /// <summary>
        ///     Reference to the quest object this quest is based upon
        /// </summary>
        [field: SerializeReference] [NotNull] private readonly Quest _quest;

        /// <summary>
        ///     Quest object this quest is based upon
        /// </summary>
        [NotNull] public Quest Quest => _quest;

        /// <summary>
        ///     List of all objectives
        /// </summary>
        private readonly List<QuestObjective> _objectives = new List<QuestObjective>();

        /// <summary>
        ///     List of all instance objectives
        /// </summary>
        public IReadOnlyList<QuestObjective> Objectives => _objectives;

        /// <summary>
        ///     State of the quest
        /// </summary>
        public QuestState State { get; private set; } = QuestState.Inactive;

        /// <summary>
        ///     Returns true if this quest instance is completed
        /// </summary>
        public bool IsCompleted => State == QuestState.Completed;

        /// <summary>
        ///     Returns true if this quest instance is failed
        /// </summary>
        public bool IsFailed => State == QuestState.Failed;

        /// <summary>
        ///     Adds an objective to the list
        /// </summary>
        [NotNull] public QuestInstance WithObjective([CanBeNull] QuestObjective objective)
        {
            if (ReferenceEquals(objective, null))
            {
                Debug.LogError($"Tried to add null objective to quest {Quest.name}");
                return this;
            }
            _objectives.Add(objective);
            return this;
        }

        /// <summary>
        ///     Starts the quest
        /// </summary>
        public void Start()
        {
            if (State != QuestState.Inactive)
            {
                Debug.LogWarning($"Attempted to start quest {Quest.name} but it is already in state {State}");
                return;
            }

            State = QuestState.InProgress;
            _quest.OnQuestStarted(this);

            ActivateFirstInactiveObjectiveIfNoneAreInProgress();
        }

        /// <summary>
        ///     Forces the quest to finish by completing all required objectives.
        /// </summary>
        internal void ForceFinish()
        {
            if (State is QuestState.Completed or QuestState.Failed) return;

            for (int i = 0; i < _objectives.Count; i++)
            {
                QuestObjective objective = _objectives[i];
                if (!objective.IsRequired) continue;
                if (objective.State is QuestState.Completed or QuestState.Failed) continue;
                objective.State = QuestState.Completed;
                objective.OnQuestObjectiveCompleted(this);
            }

            State = QuestState.Completed;
            _quest.OnQuestCompleted(this);
        }

        /// <summary>
        ///     Forces the quest to fail by failing all required objectives.
        /// </summary>
        internal void ForceFail()
        {
            if (State is QuestState.Completed or QuestState.Failed) return;

            for (int i = 0; i < _objectives.Count; i++)
            {
                QuestObjective objective = _objectives[i];
                if (!objective.IsRequired) continue;
                if (objective.State is QuestState.Completed or QuestState.Failed) continue;
                objective.State = QuestState.Failed;
                objective.OnQuestObjectiveFailed(this);
            }

            State = QuestState.Failed;
            _quest.OnQuestFailed(this);
        }
        
        /// <summary>
        ///     Handle quest failure or completion states
        /// </summary>
        void IWithObjectives.AfterQuestIterationComplete()
        {
            if (_objectives.Count == 0) return;

            if (IsAnyRequiredObjectiveFailed())
            {
                State = QuestState.Failed;
                _quest.OnQuestFailed(this);
                return;
            }

            if (AreRequiredObjectivesComplete())
            {
                State = QuestState.Completed;
                _quest.OnQuestCompleted(this);
                return;
            }

            ActivateFirstInactiveObjectiveIfNoneAreInProgress();
        }

        /// <summary>
        ///     Checks if all required objectives are completed
        /// </summary>
        private bool AreRequiredObjectivesComplete()
        {
            for (int i = 0; i < Objectives.Count; i++)
            {
                QuestObjective objective = Objectives[i];
                if (!objective.IsRequired) continue;
                if (objective.State != QuestState.Completed) return false;
            }

            return true;
        }

        /// <summary>
        ///     Checks if any required objective is failed
        /// </summary>
        private bool IsAnyRequiredObjectiveFailed()
        {
            for (int i = 0; i < Objectives.Count; i++)
            {
                QuestObjective objective = Objectives[i];
                if (!objective.IsRequired) continue;
                if (objective.State == QuestState.Failed) return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets the first objective of the specified type
        /// </summary>
        [CanBeNull] public TQuestObjective GetObjective<TQuestObjective>() where TQuestObjective : QuestObjective
        {
            for (int i = 0; i < _objectives.Count; i++)
            {
                if (_objectives[i] is TQuestObjective typed) return typed;
            }
            return null;
        }

        /// <summary>
        ///     Gets all objectives of the specified type
        /// </summary>
        public ROListAccess<TQuestObjective> GetObjectives<TQuestObjective>() where TQuestObjective : QuestObjective
        {
            RWListAccess<TQuestObjective> list = RWListAccess<TQuestObjective>.Create();
            List<TQuestObjective> refList = list.List;

            for (int i = 0; i < _objectives.Count; i++)
            {
                if (_objectives[i] is TQuestObjective typed) refList.Add(typed);
            }

            return list.ToReadOnly();
        }

        /// <summary>
        ///     Tries to complete the first in-progress objective of the specified type
        /// </summary>
        public bool TryCompleteObjective<TQuestObjective>() where TQuestObjective : QuestObjective
        {
            for (int i = 0; i < _objectives.Count; i++)
            {
                if (_objectives[i] is not TQuestObjective objective) continue;
                if (objective.State != QuestState.InProgress) continue;
                objective.State = QuestState.Completed;
                objective.OnQuestObjectiveCompleted(this);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Tries to fail the first in-progress objective of the specified type
        /// </summary>
        public bool TryFailObjective<TQuestObjective>() where TQuestObjective : QuestObjective
        {
            for (int i = 0; i < _objectives.Count; i++)
            {
                if (_objectives[i] is not TQuestObjective objective) continue;
                if (objective.State != QuestState.InProgress) continue;
                objective.State = QuestState.Failed;
                objective.OnQuestObjectiveFailed(this);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Tries to complete a specific objective instance
        /// </summary>
        public bool TryCompleteObjective([NotNull] QuestObjective objective)
        {
            if (objective.State != QuestState.InProgress) return false;

            for (int i = 0; i < _objectives.Count; i++)
            {
                if (!ReferenceEquals(_objectives[i], objective)) continue;
                objective.State = QuestState.Completed;
                objective.OnQuestObjectiveCompleted(this);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Tries to fail a specific objective instance
        /// </summary>
        public bool TryFailObjective([NotNull] QuestObjective objective)
        {
            if (objective.State != QuestState.InProgress) return false;

            for (int i = 0; i < _objectives.Count; i++)
            {
                if (!ReferenceEquals(_objectives[i], objective)) continue;
                objective.State = QuestState.Failed;
                objective.OnQuestObjectiveFailed(this);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Creates a new quest instance from a quest object
        /// </summary>
        [NotNull] public static QuestInstance FromQuest([NotNull] Quest fromQuest)
        {
            return fromQuest.Create();
        }

        internal QuestInstance([NotNull] Quest fromQuest)
        {
            _quest = fromQuest;
        }

        internal void Tick(float deltaTime)
        {
            IWithObjectives self = this;
            self.TickCompletionStatusCheck(this, deltaTime);
        }

        /// <summary>
        ///     Activates the first inactive objective
        /// </summary>
        private void ActivateFirstInactiveObjectiveIfNoneAreInProgress()
        {
            // Do not activate if any required objective is in progress
            for (int i = 0; i < Objectives.Count; i++)
            {
                QuestObjective objective = Objectives[i];
                if (!objective.IsRequired) continue;
                if (objective.State == QuestState.InProgress) return;
            }

            for (int i = 0; i < Objectives.Count; i++)
            {
                QuestObjective objective = Objectives[i];
                if (objective.State != QuestState.Inactive) continue;
                objective.State = QuestState.InProgress;
                objective.OnQuestObjectiveStarted(this);
                
                // Check if required, if not then continue until first required objective is met
                // this is intended to activate the first required objective and all preceding optional objectives
                if (!objective.IsRequired) continue;
                return;
            }
        }
    }
}
