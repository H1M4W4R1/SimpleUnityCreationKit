using JetBrains.Annotations;
using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleRelations.Examples
{
    /// <summary>Runtime controller for the SimpleRelations example scene.</summary>
    [DisallowMultipleComponent]
    public sealed class ExampleRelationsScene : MonoBehaviour
    {
        [SerializeField] private ExampleRelationActor _source;
        [SerializeField] private ExampleRelationActor _target;
        [SerializeField] private int _changeAmount = 10;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private bool _runExampleOnStart;

        [CanBeNull] private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Start()
        {
            if (_createRuntimeUI) CreateRuntimeUI();

            if (_runExampleOnStart)
                RunExample();
            else
                RefreshStatus("Ready. Relations are one-way: Source can change independently of Target.");
        }

        [ContextMenu("Run Relations Example")]
        public void RunExample()
        {
            if (!HasActors()) return;

            RelationChangeContext<ExampleTrustRelation> trustContext = new(_source, _target, _changeAmount);
            RelationChangeContext<ExampleFearRelation> fearContext = new(_source, _target, _changeAmount / 2);
            OperationResult trustResult = RelationAPI.Change<ExampleTrustRelation>(in trustContext);
            OperationResult fearResult = RelationAPI.Change<ExampleFearRelation>(in fearContext);
            _lastResult = ExampleRuntimePanel.FormatResult(fearResult);
            Debug.Log("[SimpleRelations] Trust result: " + trustResult + ", fear result: " + fearResult + ".");
            RefreshStatus("Ran trust and fear changes from Source toward Target.");
        }

        private void IncreaseTrust()
        {
            if (!HasActors()) return;

            RelationChangeContext<ExampleTrustRelation> context = new(_source, _target, _changeAmount);
            OperationResult result = RelationAPI.Change<ExampleTrustRelation>(in context);
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            RefreshStatus("Increased Source trust in Target.");
        }

        private void DecreaseTrust()
        {
            if (!HasActors()) return;

            RelationChangeContext<ExampleTrustRelation> context = new(_source, _target, -_changeAmount);
            OperationResult result = RelationAPI.Change<ExampleTrustRelation>(in context);
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            RefreshStatus("Decreased Source trust in Target.");
        }

        private void IncreaseFear()
        {
            if (!HasActors()) return;

            RelationChangeContext<ExampleFearRelation> context = new(_source, _target, _changeAmount);
            OperationResult result = RelationAPI.Change<ExampleFearRelation>(in context);
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            RefreshStatus("Increased Source fear of Target.");
        }

        private void SetTrust()
        {
            if (!HasActors()) return;

            RelationSetContext<ExampleTrustRelation> context = new(_source, _target, 50);
            OperationResult result = RelationAPI.Set<ExampleTrustRelation>(in context);
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            RefreshStatus("Set Source trust in Target to 50.");
        }

        private bool HasActors()
        {
            if (_source && _target) return true;

            Debug.LogWarning("[SimpleRelations] Assign source and target relation actors on the scene controller.");
            RefreshStatus("Example relation actors are not assigned.");
            return false;
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleRelations Example",
                "Track independent trust and fear values from Source to Target. Target's reverse values stay unchanged.");

            _panel.AddSection("Source -> Target");
            Button trustIncreaseButton = _panel.AddButton("Increase Trust");
            trustIncreaseButton.onClick.AddListener(IncreaseTrust);

            Button trustDecreaseButton = _panel.AddButton("Decrease Trust");
            trustDecreaseButton.onClick.AddListener(DecreaseTrust);

            Button trustSetButton = _panel.AddButton("Set Trust To 50");
            trustSetButton.onClick.AddListener(SetTrust);

            Button fearButton = _panel.AddButton("Increase Fear");
            fearButton.onClick.AddListener(IncreaseFear);

            Button runAllButton = _panel.AddButton("Run Full Example");
            runAllButton.onClick.AddListener(RunExample);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null)) return;

            RelationQueryContext<ExampleTrustRelation> sourceTrustContext = new(_source, _target);
            RelationQueryContext<ExampleFearRelation> sourceFearContext = new(_source, _target);
            RelationQueryContext<ExampleTrustRelation> targetTrustContext = new(_target, _source);
            int sourceTrust = RelationAPI.GetValue<ExampleTrustRelation>(in sourceTrustContext);
            int sourceFear = RelationAPI.GetValue<ExampleFearRelation>(in sourceFearContext);
            int targetTrust = RelationAPI.GetValue<ExampleTrustRelation>(in targetTrustContext);

            _panel.SetStatus(
                message +
                "\nSource -> Target trust: " + sourceTrust +
                " | fear: " + sourceFear +
                "\nTarget -> Source trust: " + targetTrust +
                "\nSource serialized entries: " + (_source ? _source.Relations.Count : 0) +
                "\nLast result: " + _lastResult);
        }
    }
}
