using Systems.SimpleCore.Operations;
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Abstract.Markers;
using Systems.SimpleQuests.Data;
using UnityEngine;

namespace Systems.SimpleQuests.Examples.Scripts
{
    /// <summary>
    ///     Example quest that implements <see cref="IUniqueQuest"/>.
    ///     Only one active instance is allowed at a time — subsequent <c>TryStartQuest</c>
    ///     calls will fail with <c>QuestAlreadyStarted</c> until this instance finishes.
    /// </summary>
    public sealed class ExampleUniqueQuest : Quest, IUniqueQuest
    {
        public override QuestInstance Create()
        {
            return base.Create()
                .WithObjective(new ExampleKeyObjective(KeyCode.U));
        }

        protected internal override void OnQuestStarted(QuestInstance instance)
        {
            base.OnQuestStarted(instance);
            Debug.Log($"Unique quest {name} started. Press U to complete it.");
        }

        protected internal override void OnQuestStartFailed(OperationResult reason)
        {
            base.OnQuestStartFailed(reason);
            Debug.Log($"Unique quest {name} could not be started (already active).");
        }

        protected internal override void OnQuestCompleted(QuestInstance instance)
        {
            base.OnQuestCompleted(instance);
            Debug.Log($"Unique quest {name} completed.");
        }

        protected internal override void OnQuestFailed(QuestInstance instance)
        {
            base.OnQuestFailed(instance);
            Debug.Log($"Unique quest {name} failed.");
        }
    }
}
