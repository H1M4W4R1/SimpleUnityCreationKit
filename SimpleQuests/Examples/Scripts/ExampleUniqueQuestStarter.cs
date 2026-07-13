using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using Systems.SimpleQuests.Data.Enums;
using Systems.SimpleQuests.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleQuests.Examples.Scripts
{
    /// <summary>
    ///     Demonstrates unique quest behaviour through runtime controls and context-menu actions.
    /// </summary>
    public sealed class ExampleUniqueQuestStarter : MonoBehaviour
    {
        private static readonly Vector2 PanelPosition = new Vector2(584f, 0f);

        [SerializeField] private bool _createRuntimeUI = true;

        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            RefreshStatus("Ready. Start the unique quest and try duplicate cases.");
        }

        [ContextMenu("Start Unique Quest")]
        private void StartUniqueQuest()
        {
            OperationResult result = QuestAPI.TryStartQuest<ExampleUniqueQuest>(out _);
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            Debug.Log(result ? "Unique quest started successfully." : "Unique quest start rejected: " + result);
            RefreshStatus("Unique quest start attempted.");
        }

        [ContextMenu("Start Unique Quest Twice")]
        private void StartUniqueQuestTwice()
        {
            OperationResult firstResult = QuestAPI.TryStartQuest<ExampleUniqueQuest>(out _);
            OperationResult secondResult = QuestAPI.TryStartQuest<ExampleUniqueQuest>(out _);
            _lastResult = ExampleRuntimePanel.FormatResult(secondResult);
            Debug.Log("[SimpleQuests] Unique first start: " + firstResult + ", second start: " + secondResult);
            RefreshStatus("Started unique quest twice to show duplicate rejection.");
        }

        [ContextMenu("Start Unique Quest Ignoring Running")]
        private void StartUniqueQuestIgnoringRunning()
        {
            StartQuestFlags flags = StartQuestFlags.AllowStartUniqueIfRunning;
            _lastResult = ExampleRuntimePanel.FormatResult(QuestAPI.TryStartQuest<ExampleUniqueQuest>(out _, flags));
            RefreshStatus("Unique quest start attempted with running override.");
        }

        [ContextMenu("Check Unique Quest State")]
        private void CheckState()
        {
            bool completed = QuestAPI.IsQuestCompleted<ExampleUniqueQuest>();
            bool failed = QuestAPI.IsQuestFailed<ExampleUniqueQuest>();
            Debug.Log("ExampleUniqueQuest completed: " + completed + ", failed: " + failed);
            RefreshStatus("Checked unique quest state.");
        }

        [ContextMenu("Complete Unique Quest")]
        private void CompleteUniqueQuest()
        {
            bool completed = QuestAPI.CompleteQuest<ExampleUniqueQuest>();
            RefreshStatus("Complete unique quest result: " + completed);
        }

        [ContextMenu("Clear Unique Quest State")]
        private void ClearState()
        {
            QuestAPI.ClearAllQuests();
            RefreshStatus("Cleared unique quest state.");
        }

        private void OnDestroy()
        {
            QuestAPI.ClearAllQuests();
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleQuests Unique Example",
                "Navigate unique quest duplicate rejection, override flags, completion, and state checks.",
                PanelPosition);

            _panel.AddSection("Unique Quest");
            Button startButton = _panel.AddButton("Start Unique Quest");
            startButton.onClick.AddListener(StartUniqueQuest);

            Button twiceButton = _panel.AddButton("Start Twice");
            twiceButton.onClick.AddListener(StartUniqueQuestTwice);

            Button overrideButton = _panel.AddButton("Start With Running Override");
            overrideButton.onClick.AddListener(StartUniqueQuestIgnoringRunning);

            Button completeButton = _panel.AddButton("Complete Unique Quest");
            completeButton.onClick.AddListener(CompleteUniqueQuest);

            Button checkButton = _panel.AddButton("Check State");
            checkButton.onClick.AddListener(CheckState);

            Button clearButton = _panel.AddButton("Clear State");
            clearButton.onClick.AddListener(ClearState);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            bool active = !ReferenceEquals(QuestAPI.GetFirstActiveQuestOfType<ExampleUniqueQuest>(), null);
            bool completed = QuestAPI.IsQuestCompleted<ExampleUniqueQuest>();
            bool failed = QuestAPI.IsQuestFailed<ExampleUniqueQuest>();
            _panel.SetStatus(
                message +
                "\nActive: " + active +
                " | Completed: " + completed +
                " | Failed: " + failed +
                "\nLast result: " + _lastResult);
        }
    }
}
