using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Data;
using UnityEngine;

namespace Systems.SimpleQuests.Examples.Scripts
{
    public sealed class ExampleKeyObjective : QuestObjective
    {
        private readonly KeyCode _key;
        
        public ExampleKeyObjective(KeyCode key)
        {
            _key = key;
        }
        
        public override bool ShouldBeComplete()
        {
            return Input.GetKey(_key);
        }

        protected internal override void OnQuestObjectiveStarted(QuestInstance quest)
        {
            base.OnQuestObjectiveStarted(quest);
            Debug.Log($"Objective to press key {_key} has been started. Press key now");
        }

        protected internal override void OnQuestObjectiveCompleted(QuestInstance quest)
        {
            base.OnQuestObjectiveCompleted(quest);
            Debug.Log($"Objective to press key {_key} has been completed");
        }

        protected internal override void OnQuestObjectiveFailed(QuestInstance quest)
        {
            base.OnQuestObjectiveFailed(quest);
            Debug.Log($"Objective to press key {_key} has been failed");
        }

        protected internal override void OnQuestObjectiveTick(QuestInstance questInstance, float deltaTime)
        {
            base.OnQuestObjectiveTick(questInstance, deltaTime);
            Debug.Log($"Objective to press key {_key} has been ticked");
        }
    }
}