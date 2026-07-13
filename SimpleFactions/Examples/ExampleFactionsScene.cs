using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Examples;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleFactions.Examples
{
    [DisallowMultipleComponent]
    public sealed class ExampleFactionsScene : MonoBehaviour
    {
        [SerializeField] private ExampleFactionMembership _membership;
        [SerializeField] private long _reputationGain = 125L;
        [SerializeField] private ReputationLevelBase _manualLevel;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private bool _runExampleOnStart;

        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Awake()
        {
            if (!_membership && !TryGetComponent(out _membership))
                _membership = gameObject.AddComponent<ExampleFactionMembership>();

            if (!TryGetComponent(out ExampleFactionHolder holder))
                holder = gameObject.AddComponent<ExampleFactionHolder>();
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
                RefreshStatus("Ready. Join a faction or change reputation.");
            }
        }

        [ContextMenu("Run Factions Example")]
        public void RunExample()
        {
            if (!_membership)
            {
                Debug.LogWarning("[SimpleFactions] Example membership component is not assigned.");
                RefreshStatus("Example membership component is not assigned.");
                return;
            }

            ExampleFaction faction = FactionDatabase.GetExact<ExampleFaction>();
            if (ReferenceEquals(faction, null))
            {
                Debug.LogWarning("[SimpleFactions] ExampleFaction was not found in the faction database. Let the auto-create/addressables setup generate it before running faction operations.");
                RefreshStatus("ExampleFaction was not found in the faction database.");
                return;
            }

            OperationResult joinResult = FactionAPI.Join<ExampleFaction, ExampleFactionHolder>(_membership);
            OperationResult reputationResult = FactionAPI.ChangeReputation<ExampleFaction, ExampleFactionHolder>(
                _membership,
                _reputationGain);
            _lastResult = ExampleRuntimePanel.FormatResult(reputationResult);

            ReputationLevelBase currentLevel =
                FactionAPI.GetLevel<ExampleFaction, ExampleFactionHolder>(_membership);
            string currentLevelName = ReferenceEquals(currentLevel, null) ? "none" : currentLevel.name;

            Debug.Log("[SimpleFactions] Join result: " + joinResult +
                      ", reputation result: " + reputationResult +
                      ", reputation: " + _membership.GetReputation<ExampleFaction>() +
                      ", current level: " + currentLevelName);

            if (!_manualLevel) return;

            OperationResult levelResult =
                FactionAPI.AssignLevel<ExampleFaction, ExampleFactionHolder>(_membership, _manualLevel);
            _lastResult = ExampleRuntimePanel.FormatResult(levelResult);
            Debug.Log("[SimpleFactions] Manual level assignment result: " + levelResult);
            RefreshStatus("Ran join, reputation, and manual level flow.");
        }

        private void JoinFaction()
        {
            if (!ValidateExampleFaction()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(FactionAPI.Join<ExampleFaction, ExampleFactionHolder>(_membership));
            RefreshStatus("Join attempted.");
        }

        private void LeaveFaction()
        {
            if (!ValidateMembership()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(FactionAPI.Leave<ExampleFaction, ExampleFactionHolder>(_membership));
            RefreshStatus("Leave attempted.");
        }

        private void GainReputation()
        {
            if (!ValidateExampleFaction()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(FactionAPI.ChangeReputation<ExampleFaction, ExampleFactionHolder>(_membership, _reputationGain));
            RefreshStatus("Added " + _reputationGain + " reputation.");
        }

        private void LoseReputation()
        {
            if (!ValidateExampleFaction()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(FactionAPI.ChangeReputation<ExampleFaction, ExampleFactionHolder>(_membership, -_reputationGain));
            RefreshStatus("Removed " + _reputationGain + " reputation.");
        }

        private void AssignManualLevel()
        {
            if (!ValidateMembership()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(FactionAPI.AssignLevel<ExampleFaction, ExampleFactionHolder>(_membership, _manualLevel));
            RefreshStatus("Manual level assignment attempted.");
        }

        private void ClearManualLevel()
        {
            if (!ValidateMembership()) return;
            _lastResult = ExampleRuntimePanel.FormatResult(FactionAPI.AssignLevel<ExampleFaction, ExampleFactionHolder>(_membership, null));
            RefreshStatus("Manual level clear attempted.");
        }

        private bool ValidateExampleFaction()
        {
            if (!ValidateMembership())
            {
                return false;
            }

            ExampleFaction faction = FactionDatabase.GetExact<ExampleFaction>();
            if (!ReferenceEquals(faction, null))
            {
                return true;
            }

            Debug.LogWarning("[SimpleFactions] ExampleFaction was not found in the faction database. Let the auto-create/addressables setup generate it before running faction operations.");
            RefreshStatus("ExampleFaction was not found in the faction database.");
            return false;
        }

        private bool ValidateMembership()
        {
            if (_membership)
            {
                return true;
            }

            Debug.LogWarning("[SimpleFactions] Example membership component is not assigned.");
            RefreshStatus("Example membership component is not assigned.");
            return false;
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleFactions Example",
                "Navigate membership, reputation changes, automatic level checks, and manual level overrides.");

            _panel.AddSection("Membership");
            Button joinButton = _panel.AddButton("Join Faction");
            joinButton.onClick.AddListener(JoinFaction);

            Button leaveButton = _panel.AddButton("Leave Faction");
            leaveButton.onClick.AddListener(LeaveFaction);

            _panel.AddSection("Reputation");
            Button gainButton = _panel.AddButton("Gain Reputation");
            gainButton.onClick.AddListener(GainReputation);

            Button loseButton = _panel.AddButton("Lose Reputation");
            loseButton.onClick.AddListener(LoseReputation);

            Button assignButton = _panel.AddButton("Assign Manual Level");
            assignButton.onClick.AddListener(AssignManualLevel);

            Button clearButton = _panel.AddButton("Clear Manual Level");
            clearButton.onClick.AddListener(ClearManualLevel);

            Button runAllButton = _panel.AddButton("Run Full Example");
            runAllButton.onClick.AddListener(RunExample);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            string levelName = "none";
            long reputation = 0L;
            if (_membership)
            {
                reputation = _membership.GetReputation<ExampleFaction>();
                ReputationLevelBase level = FactionAPI.GetLevel<ExampleFaction, ExampleFactionHolder>(_membership);
                levelName = ReferenceEquals(level, null) ? "none" : level.name;
            }

            _panel.SetStatus(
                message +
                "\nReputation: " + reputation +
                "\nCurrent level: " + levelName +
                "\nLast result: " + _lastResult);
        }
    }
}
