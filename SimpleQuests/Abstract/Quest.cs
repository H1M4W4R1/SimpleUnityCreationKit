using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleQuests.Data;
using Systems.SimpleQuests.Operations;
using UnityEngine;

namespace Systems.SimpleQuests.Abstract
{
    /// <summary>
    ///     Basic quest that consists of multiple tasks/objectives to complete
    /// </summary>
    [AutoCreate("Quests", QuestDatabase.LABEL)]
    public abstract class Quest : ScriptableObject
    {
        /// <summary>
        ///     Create instance of quest
        /// </summary>
        /// <remarks>
        ///     You should apply objectives to the instance and return it.
        /// </remarks>
        [NotNull] public virtual QuestInstance Create() => new(this);

        /// <summary>
        ///     Checks if quest can be started
        /// </summary>
        protected internal virtual OperationResult CanBeStarted() =>
            QuestOperations.Permitted();
        
        /// <summary>
        ///     Event that is called when quest is started
        /// </summary>
        protected internal virtual void OnQuestStarted(QuestInstance instance) { }
        
        /// <summary>
        ///     Event that is called when quest start fails
        /// </summary>
        protected internal virtual void OnQuestStartFailed(OperationResult reason) { }
        
        /// <summary>
        ///     Event that is called when quest is completed
        /// </summary>
        protected internal virtual void OnQuestCompleted(QuestInstance instance) { }
        
        /// <summary>
        ///     Event that is called when quest is failed
        /// </summary>
        protected internal virtual void OnQuestFailed(QuestInstance instance) { }
    }
}