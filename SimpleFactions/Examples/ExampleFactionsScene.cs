using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data;
using Systems.SimpleFactions.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleFactions.Examples
{
    /// <summary>Small membership-only example. Model reputation and diplomacy with SimpleRelations.</summary>
    [DisallowMultipleComponent]
    public sealed class ExampleFactionsScene : MonoBehaviour
    {
        [SerializeField] private ExampleFactionMembership _membership;
        [SerializeField] private bool _createRuntimeUI = true;

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
            if (!_createRuntimeUI) return;

            _panel = ExampleRuntimePanel.Create(
                "SimpleFactions Example",
                "Join and leave a faction. Use SimpleRelations for reputation and diplomacy.");
            Button joinButton = _panel.AddButton("Join Faction");
            joinButton.onClick.AddListener(JoinFaction);
            Button leaveButton = _panel.AddButton("Leave Faction");
            leaveButton.onClick.AddListener(LeaveFaction);
            RefreshStatus("Ready.");
        }

        private void JoinFaction()
        {
            if (!TryGetFaction(out ExampleFaction faction)) return;

            OperationResult result = FactionAPI.Join<ExampleFaction, ExampleFactionHolder>(_membership);
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            RefreshStatus("Join attempted.");
        }

        private void LeaveFaction()
        {
            if (!TryGetFaction(out ExampleFaction faction)) return;

            OperationResult result = FactionAPI.Leave<ExampleFaction, ExampleFactionHolder>(_membership);
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            RefreshStatus("Leave attempted.");
        }

        private bool TryGetFaction(out ExampleFaction faction)
        {
            faction = FactionDatabase.GetExact<ExampleFaction>();
            if (!ReferenceEquals(faction, null) && faction && _membership) return true;

            Debug.LogWarning("[SimpleFactions] ExampleFaction or its membership component is unavailable.");
            return false;
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null)) return;

            bool isMember = _membership && _membership.IsMemberOf<ExampleFaction>();
            _panel.SetStatus(message + "\nMember: " + isMember + "\nLast result: " + _lastResult);
        }
    }
}
