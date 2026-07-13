using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Data;
using UnityEngine;

namespace Systems.SimpleQuests.Examples.Scripts
{
    public sealed class ExampleQuest : Quest
    {
        public override QuestInstance Create()
        {
            return base.Create()
                .WithObjective(new ExampleKeyObjective(KeyCode.A))
                .WithObjective(new ExampleKeyObjective(KeyCode.B))
                .WithObjective(new ExampleKeyObjective(KeyCode.C));
        }

        protected internal override void OnQuestStarted(QuestInstance instance)
        {
            base.OnQuestStarted(instance);
            Debug.Log($"Quest {name} has been started");
        }

        protected internal override void OnQuestCompleted(QuestInstance instance)
        {
            base.OnQuestCompleted(instance);
            Debug.Log($"Quest {name} has been completed");
        }

        protected internal override void OnQuestFailed(QuestInstance instance)
        {
            base.OnQuestFailed(instance);
            Debug.Log($"Quest {name} has been failed");
        }
    }
}