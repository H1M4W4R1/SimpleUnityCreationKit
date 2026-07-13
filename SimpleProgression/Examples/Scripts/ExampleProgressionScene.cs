using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Examples;
using Systems.SimpleProgression.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleProgression.Examples.Scripts
{
    [DisallowMultipleComponent]
    public sealed class ExampleProgressionScene : MonoBehaviour
    {
        [SerializeField] private ulong _questRewardExperience = 125UL;
        [SerializeField] private int _bonusLevels = 1;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private bool _runExampleOnStart;

        private ExampleProgressionController _controller;
        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Awake()
        {
            if (!TryGetComponent(out _controller))
                _controller = gameObject.AddComponent<ExampleProgressionController>();
        }

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            if (_runExampleOnStart)
            {
                RunExample();
            }
            else
            {
                RefreshStatus("Ready. Add experience or levels.");
            }
        }

        [ContextMenu("Run Progression Example")]
        public void RunExample()
        {
            OperationResult experienceResult = ProgressionAPI.AddExperience(gameObject, _questRewardExperience);
            OperationResult levelResult = ProgressionAPI.IncreaseLevel(gameObject, _bonusLevels);
            _lastResult = ExampleRuntimePanel.FormatResult(levelResult);
            Debug.Log("[SimpleProgression] Experience result: " + experienceResult + ", level result: " + levelResult + ", level: " + _controller.CurrentExampleLevel + ", experience: " + _controller.Experience);
            RefreshStatus("Ran reward experience and bonus level flow.");
        }

        private void AddQuestRewardExperience()
        {
            _lastResult = ExampleRuntimePanel.FormatResult(ProgressionAPI.AddExperience(gameObject, _questRewardExperience));
            RefreshStatus("Added " + _questRewardExperience + " experience.");
        }

        private void AddBonusLevel()
        {
            _lastResult = ExampleRuntimePanel.FormatResult(ProgressionAPI.IncreaseLevel(gameObject, _bonusLevels));
            RefreshStatus("Added " + _bonusLevels + " bonus level(s).");
        }

        private void SpendRewardExperience()
        {
            _lastResult = ExampleRuntimePanel.FormatResult(_controller.TryTakeExperience(_questRewardExperience));
            RefreshStatus("Tried to spend " + _questRewardExperience + " experience.");
        }

        private void IncreaseToMaxLevel()
        {
            int remainingLevels = _controller.GetMaxLevel() - _controller.CurrentExampleLevel;
            if (remainingLevels <= 0)
            {
                _lastResult = ExampleRuntimePanel.FormatResult(ProgressionAPI.IncreaseLevel(gameObject, 1));
                RefreshStatus("Tried to increase beyond max level.");
                return;
            }

            _lastResult = ExampleRuntimePanel.FormatResult(ProgressionAPI.IncreaseLevel(gameObject, remainingLevels));
            RefreshStatus("Increased toward max level.");
        }

        private void ResetProgression()
        {
            if (_controller.Experience > 0UL)
            {
                _lastResult = ExampleRuntimePanel.FormatResult(_controller.TryTakeExperience(_controller.Experience));
            }

            RefreshStatus("Progression reset through experience removal.");
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleProgression Example",
                "Explore experience rewards, level jumps, spending experience, and max-level guards.");

            _panel.AddSection("Progression");
            Button experienceButton = _panel.AddButton("Add Quest Reward XP");
            experienceButton.onClick.AddListener(AddQuestRewardExperience);

            Button levelButton = _panel.AddButton("Add Bonus Level");
            levelButton.onClick.AddListener(AddBonusLevel);

            Button spendButton = _panel.AddButton("Spend Reward XP");
            spendButton.onClick.AddListener(SpendRewardExperience);

            Button maxButton = _panel.AddButton("Increase To Max");
            maxButton.onClick.AddListener(IncreaseToMaxLevel);

            Button resetButton = _panel.AddButton("Reset Progression");
            resetButton.onClick.AddListener(ResetProgression);

            Button runAllButton = _panel.AddButton("Run Full Example");
            runAllButton.onClick.AddListener(RunExample);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            _panel.SetStatus(
                message +
                "\nLevel: " + _controller.CurrentExampleLevel + "/" + _controller.GetMaxLevel() +
                "\nExperience: " + _controller.Experience +
                "\nLast result: " + _lastResult);
        }
    }
}
