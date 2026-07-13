using Systems.SimpleCore.Examples;
using Systems.SimpleQuests.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleQuests.Examples.Scripts
{
    public sealed class ExampleQuestStarter : MonoBehaviour
    {
        private static readonly Vector2 PanelPosition = new Vector2(32f, 0f);

        [SerializeField] private bool _createRuntimeUI = true;

        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            RefreshStatus("Ready. Start, complete, fail, or clear the normal quest.");
        }

        [ContextMenu("Start Quest")]
        private void StartQuest()
        {
            _lastResult = ExampleRuntimePanel.FormatResult(QuestAPI.TryStartQuest<ExampleQuest>(out _));
            RefreshStatus("Start quest attempted.");
        }

        [ContextMenu("Complete Quest")]
        private void CompleteQuest()
        {
            bool completed = QuestAPI.CompleteQuest<ExampleQuest>();
            RefreshStatus("Complete quest result: " + completed);
        }

        [ContextMenu("Fail Quest")]
        private void FailQuest()
        {
            bool failed = QuestAPI.FailQuest<ExampleQuest>();
            RefreshStatus("Fail quest result: " + failed);
        }

        [ContextMenu("Clear Quests")]
        private void ClearQuests()
        {
            QuestAPI.ClearAllQuests();
            RefreshStatus("Cleared quest state.");
        }

        private void OnDestroy()
        {
            QuestAPI.ClearAllQuests();
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleQuests Example",
                "Navigate normal quest start, forced completion, forced failure, and state clearing.",
                PanelPosition);

            _panel.AddSection("Normal Quest");
            Button startButton = _panel.AddButton("Start Quest");
            startButton.onClick.AddListener(StartQuest);

            Button completeButton = _panel.AddButton("Complete Quest");
            completeButton.onClick.AddListener(CompleteQuest);

            Button failButton = _panel.AddButton("Fail Quest");
            failButton.onClick.AddListener(FailQuest);

            Button clearButton = _panel.AddButton("Clear Quests");
            clearButton.onClick.AddListener(ClearQuests);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            bool active = !ReferenceEquals(QuestAPI.GetFirstActiveQuestOfType<ExampleQuest>(), null);
            bool completed = QuestAPI.IsQuestCompleted<ExampleQuest>();
            bool failed = QuestAPI.IsQuestFailed<ExampleQuest>();
            _panel.SetStatus(
                message +
                "\nActive: " + active +
                " | Completed: " + completed +
                " | Failed: " + failed +
                "\nLast result: " + _lastResult);
        }
    }
}
