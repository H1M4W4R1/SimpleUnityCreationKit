using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Examples;
using Systems.SimpleCrafting.Abstract;
using Systems.SimpleCrafting.Data.Runtime;
using Systems.SimpleCrafting.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleCrafting.Examples
{
    [DisallowMultipleComponent]
    public sealed class ExampleCraftingScene : MonoBehaviour, IExampleCraftingLevelProvider
    {
        [SerializeField] private CraftingRecipeBase _instantRecipe;
        [SerializeField] private CraftingRecipeBase _timedRecipe;
        [SerializeField] private CraftingRecipeBase _blockedRecipe;
        [SerializeField] private ExampleWorkbenchStation _station;
        [SerializeField] private int _craftingLevel = 1;
        [SerializeField] private float _timedAdvanceSeconds = 5f;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private bool _runExampleOnStart;

        private ExampleRuntimePanel _panel;
        private CraftingInstance _currentTimedInstance;
        private string _lastResult = "none";

        public int CraftingLevel => _craftingLevel;

        private void Awake()
        {
            if (!_station)
                _station = FindAnyObjectByType<ExampleWorkbenchStation>(FindObjectsInactive.Include);
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
                RefreshStatus("Ready. Choose a crafting case.");
            }
        }

        [ContextMenu("Run Crafting Example")]
        public void RunExample()
        {
            RunImmediateRecipe("Instant", _instantRecipe);
            RunImmediateRecipe("Blocked", _blockedRecipe);
            RunTimedRecipe();
            RefreshStatus("Ran instant, blocked, and timed crafting flow.");
        }

        private void RunImmediateRecipe(string label, CraftingRecipeBase recipe)
        {
            if (ReferenceEquals(recipe, null) || !recipe)
            {
                Debug.LogWarning("[SimpleCrafting] " + label + " recipe is not assigned.");
                RefreshStatus(label + " recipe is not assigned.");
                return;
            }

            OperationResult result = CraftingAPI.TryStartCrafting(
                recipe,
                out CraftingInstance instance,
                _station,
                this);

            _lastResult = ExampleRuntimePanel.FormatResult(result);
            Debug.Log("[SimpleCrafting] " + label + " recipe start result: " + result);
            if (!ReferenceEquals(instance, null))
                Debug.Log("[SimpleCrafting] " + label + " recipe created timed instance unexpectedly.");
        }

        private void RunTimedRecipe()
        {
            if (ReferenceEquals(_timedRecipe, null) || !_timedRecipe)
            {
                Debug.LogWarning("[SimpleCrafting] Timed recipe is not assigned.");
                RefreshStatus("Timed recipe is not assigned.");
                return;
            }

            OperationResult startResult = CraftingAPI.TryStartCrafting(
                _timedRecipe,
                out CraftingInstance instance,
                _station,
                this);

            _lastResult = ExampleRuntimePanel.FormatResult(startResult);
            Debug.Log("[SimpleCrafting] Timed recipe start result: " + startResult);
            if (ReferenceEquals(instance, null)) return;

            _currentTimedInstance = instance;

            OperationResult advanceResult = CraftingAPI.AdvanceCrafting(instance, _timedAdvanceSeconds);
            _lastResult = ExampleRuntimePanel.FormatResult(advanceResult);
            Debug.Log("[SimpleCrafting] Timed recipe advance result: " + advanceResult);

            if (!instance.IsReadyToComplete) return;

            OperationResult completeResult = CraftingAPI.TryCompleteCrafting(instance);
            _lastResult = ExampleRuntimePanel.FormatResult(completeResult);
            Debug.Log("[SimpleCrafting] Timed recipe completion result: " + completeResult);
            RefreshStatus("Timed recipe completed by full flow.");
        }

        private void StartInstantRecipe()
        {
            RunImmediateRecipe("Instant", _instantRecipe);
            RefreshStatus("Instant recipe attempted.");
        }

        private void StartBlockedRecipe()
        {
            RunImmediateRecipe("Blocked", _blockedRecipe);
            RefreshStatus("Blocked recipe attempted.");
        }

        private void StartTimedRecipeOnly()
        {
            if (ReferenceEquals(_timedRecipe, null) || !_timedRecipe)
            {
                Debug.LogWarning("[SimpleCrafting] Timed recipe is not assigned.");
                RefreshStatus("Timed recipe is not assigned.");
                return;
            }

            _lastResult = ExampleRuntimePanel.FormatResult(CraftingAPI.TryStartCrafting(
                _timedRecipe,
                out _currentTimedInstance,
                _station,
                this));

            Debug.Log("[SimpleCrafting] Timed recipe start result: " + _lastResult);
            RefreshStatus("Timed recipe start attempted.");
        }

        private void AdvanceTimedRecipe()
        {
            if (ReferenceEquals(_currentTimedInstance, null))
            {
                RefreshStatus("Start a timed recipe before advancing.");
                return;
            }

            _lastResult = ExampleRuntimePanel.FormatResult(CraftingAPI.AdvanceCrafting(_currentTimedInstance, _timedAdvanceSeconds));
            Debug.Log("[SimpleCrafting] Timed recipe advance result: " + _lastResult);
            RefreshStatus("Advanced timed recipe by " + _timedAdvanceSeconds + " seconds.");
        }

        private void CompleteTimedRecipe()
        {
            if (ReferenceEquals(_currentTimedInstance, null))
            {
                RefreshStatus("Start a timed recipe before completing.");
                return;
            }

            _lastResult = ExampleRuntimePanel.FormatResult(CraftingAPI.TryCompleteCrafting(_currentTimedInstance));
            Debug.Log("[SimpleCrafting] Timed recipe completion result: " + _lastResult);
            RefreshStatus("Timed recipe completion attempted.");
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleCrafting Example",
                "Navigate instant, blocked, and timed crafting cases with manual progress controls.");

            _panel.AddSection("Recipes");
            Button instantButton = _panel.AddButton("Craft Instant Recipe");
            instantButton.onClick.AddListener(StartInstantRecipe);

            Button blockedButton = _panel.AddButton("Try Blocked Recipe");
            blockedButton.onClick.AddListener(StartBlockedRecipe);

            _panel.AddSection("Timed Recipe");
            Button timedButton = _panel.AddButton("Start Timed Recipe");
            timedButton.onClick.AddListener(StartTimedRecipeOnly);

            Button advanceButton = _panel.AddButton("Advance Timed Recipe");
            advanceButton.onClick.AddListener(AdvanceTimedRecipe);

            Button completeButton = _panel.AddButton("Complete Timed Recipe");
            completeButton.onClick.AddListener(CompleteTimedRecipe);

            Button runAllButton = _panel.AddButton("Run Full Example");
            runAllButton.onClick.AddListener(RunExample);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            string timedState = ReferenceEquals(_currentTimedInstance, null)
                ? "none"
                : _currentTimedInstance.State + " " + _currentTimedInstance.ElapsedSeconds + "/" + _currentTimedInstance.DurationSeconds + "s";

            _panel.SetStatus(
                message +
                "\nCrafting level: " + _craftingLevel +
                "\nTimed instance: " + timedState +
                "\nLast result: " + _lastResult);
        }
    }
}
