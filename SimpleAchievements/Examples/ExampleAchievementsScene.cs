using Systems.SimpleAchievements.Abstract;
using Systems.SimpleAchievements.Examples.Achievements;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleAchievements.Utility;
using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleAchievements.Examples
{
    [DisallowMultipleComponent]
    public sealed class ExampleAchievementsScene : MonoBehaviour
    {
        [SerializeField] private AchievementData _manualAchievement;
        [SerializeField] private ExampleConditionalAchievement _conditionalAchievement;
        [SerializeField] private int _conditionIncrements = 3;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private bool _runExampleOnStart;

        private ExampleRuntimePanel _panel;
        private string _lastManualResult = "none";
        private string _lastConditionalResult = "none";

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            ExampleConditionalAchievement.ResetCount();

            if (_runExampleOnStart)
            {
                RunExample();
            }
            else
            {
                RefreshStatus("Ready. Pick a case to run.");
            }
        }

        [ContextMenu("Run Achievements Example")]
        public void RunExample()
        {
            UnlockManualAchievement();
            UnlockConditionalAchievement();
            RefreshStatus("Ran manual and conditional unlock flow.");
        }

        private void UnlockManualAchievement()
        {
            if (ReferenceEquals(_manualAchievement, null) || !_manualAchievement)
            {
                Debug.LogWarning("[SimpleAchievements] Manual achievement asset is not assigned.");
                RefreshStatus("Manual achievement asset is not assigned.");
                return;
            }

            AchievementUnlockContext context = new AchievementUnlockContext(_manualAchievement);
            OperationResult result = AchievementAPI.Unlock(in context);
            _lastManualResult = ExampleRuntimePanel.FormatResult(result);
            Debug.Log("[SimpleAchievements] Manual unlock result: " + result);
            RefreshStatus("Manual unlock attempted.");
        }

        private void UnlockConditionalAchievement()
        {
            if (ReferenceEquals(_conditionalAchievement, null) || !_conditionalAchievement)
            {
                Debug.LogWarning("[SimpleAchievements] Conditional achievement asset is not assigned.");
                RefreshStatus("Conditional achievement asset is not assigned.");
                return;
            }

            ExampleConditionalAchievement.ResetCount();

            AchievementUnlockContext blockedContext = new AchievementUnlockContext(_conditionalAchievement);
            OperationResult blockedResult = AchievementAPI.Unlock(in blockedContext);
            Debug.Log("[SimpleAchievements] Conditional unlock before progress: " + blockedResult);

            for (int incrementIndex = 0; incrementIndex < _conditionIncrements; incrementIndex++)
            {
                ExampleConditionalAchievement.IncrementCount();
            }

            AchievementUnlockContext readyContext = new AchievementUnlockContext(_conditionalAchievement);
            OperationResult readyResult = AchievementAPI.Unlock(in readyContext);
            _lastConditionalResult = ExampleRuntimePanel.FormatResult(readyResult);
            Debug.Log("[SimpleAchievements] Conditional unlock after progress: " + readyResult);
        }

        private void IncrementConditionalProgress()
        {
            ExampleConditionalAchievement.IncrementCount();
            RefreshStatus("Conditional progress incremented.");
        }

        private void ResetConditionalProgress()
        {
            ExampleConditionalAchievement.ResetCount();
            RefreshStatus("Conditional progress reset.");
        }

        private void TryUnlockConditionalNow()
        {
            if (ReferenceEquals(_conditionalAchievement, null) || !_conditionalAchievement)
            {
                Debug.LogWarning("[SimpleAchievements] Conditional achievement asset is not assigned.");
                RefreshStatus("Conditional achievement asset is not assigned.");
                return;
            }

            AchievementUnlockContext context = new AchievementUnlockContext(_conditionalAchievement);
            OperationResult result = AchievementAPI.Unlock(in context);
            _lastConditionalResult = ExampleRuntimePanel.FormatResult(result);
            Debug.Log("[SimpleAchievements] Conditional unlock result: " + result);
            RefreshStatus("Conditional unlock attempted.");
        }

        private void ForceUnlockConditional()
        {
            if (ReferenceEquals(_conditionalAchievement, null) || !_conditionalAchievement)
            {
                Debug.LogWarning("[SimpleAchievements] Conditional achievement asset is not assigned.");
                RefreshStatus("Conditional achievement asset is not assigned.");
                return;
            }

            AchievementUnlockContext context = new AchievementUnlockContext(_conditionalAchievement, true);
            OperationResult result = AchievementAPI.Unlock(in context);
            _lastConditionalResult = ExampleRuntimePanel.FormatResult(result);
            Debug.Log("[SimpleAchievements] Conditional force unlock result: " + result);
            RefreshStatus("Conditional force unlock attempted.");
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleAchievements Example",
                "Navigate manual, conditional, blocked, and forced achievement unlock cases.");

            _panel.AddSection("Manual");
            Button manualButton = _panel.AddButton("Unlock Manual Achievement");
            manualButton.onClick.AddListener(UnlockManualAchievement);

            _panel.AddSection("Conditional");
            Button incrementButton = _panel.AddButton("Increment Progress");
            incrementButton.onClick.AddListener(IncrementConditionalProgress);

            Button tryConditionalButton = _panel.AddButton("Try Conditional Unlock");
            tryConditionalButton.onClick.AddListener(TryUnlockConditionalNow);

            Button forceButton = _panel.AddButton("Force Conditional Unlock");
            forceButton.onClick.AddListener(ForceUnlockConditional);

            Button resetButton = _panel.AddButton("Reset Conditional Progress");
            resetButton.onClick.AddListener(ResetConditionalProgress);

            Button runAllButton = _panel.AddButton("Run Full Example");
            runAllButton.onClick.AddListener(RunExample);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            string conditionalTarget = ReferenceEquals(_conditionalAchievement, null)
                ? "?"
                : _conditionalAchievement.TargetCount.ToString();
            bool manualUnlocked = AchievementAPI.IsUnlocked(_manualAchievement);
            bool conditionalUnlocked = AchievementAPI.IsUnlocked(_conditionalAchievement);
            _panel.SetStatus(
                message +
                "\nManual: " + manualUnlocked +
                " (" + _lastManualResult + ")" +
                "\nConditional: " + conditionalUnlocked +
                " progress " + ExampleConditionalAchievement.CurrentCount + "/" + conditionalTarget +
                " (" + _lastConditionalResult + ")");
        }
    }
}
