using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using Systems.SimpleEntities.Components;
using Systems.SimpleEntities.Examples.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleEntities.Examples.Status
{
    [RequireComponent(typeof(AliveEntityBase))]
    public sealed class ExampleBurningStatusApplier : MonoBehaviour
    {
        [SerializeField] private bool _createRuntimeUI = true;

        private AliveEntityBase _entity;
        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Awake()
        {
            _entity = GetComponent<AliveEntityBase>();
        }

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            RefreshStatus("Ready. Apply statuses or damage the example entity.");
        }

        [ContextMenu("Set on flame")]
        private void SetOnFlame()
        {
            _lastResult = ExampleRuntimePanel.FormatResult(_entity.ApplyStatus<BurningStatusExample>());
            RefreshStatus("Apply burning status attempted.");
        }

        [ContextMenu("Remove from flame")]
        private void RemoveFromFlame()
        {
            _lastResult = ExampleRuntimePanel.FormatResult(_entity.RemoveStatus<BurningStatusExample>());
            RefreshStatus("Remove burning status attempted.");
        }

        [ContextMenu("Check if is on flame")]
        private void CheckIfIsOnFlame()
        {
            bool isOnFlame = _entity.HasStatus<BurningStatusExample>();
            Debug.Log(isOnFlame ? _entity.name + " is on flame!" : _entity.name + " is not on flame!");
            RefreshStatus("Burning status check: " + isOnFlame);
        }

        [ContextMenu("Deal fire damage")]
        private void DealFireDamage()
        {
            ExampleEntityBase exampleEntity = _entity as ExampleEntityBase;
            if (ReferenceEquals(exampleEntity, null) || !exampleEntity)
            {
                RefreshStatus("Target is not an ExampleEntityBase.");
                return;
            }

            exampleEntity.DealFireDamage();
            RefreshStatus("Fire damage applied.");
        }

        [ContextMenu("Deal cold damage")]
        private void DealColdDamage()
        {
            ExampleEntityBase exampleEntity = _entity as ExampleEntityBase;
            if (ReferenceEquals(exampleEntity, null) || !exampleEntity)
            {
                RefreshStatus("Target is not an ExampleEntityBase.");
                return;
            }

            exampleEntity.DealColdDamage();
            RefreshStatus("Cold damage applied.");
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleEntities Example",
                "Navigate status application, status removal, status checks, and elemental damage cases.");

            _panel.AddSection("Status");
            Button applyButton = _panel.AddButton("Apply Burning");
            applyButton.onClick.AddListener(SetOnFlame);

            Button removeButton = _panel.AddButton("Remove Burning");
            removeButton.onClick.AddListener(RemoveFromFlame);

            Button checkButton = _panel.AddButton("Check Burning");
            checkButton.onClick.AddListener(CheckIfIsOnFlame);

            _panel.AddSection("Damage");
            Button fireButton = _panel.AddButton("Deal Fire Damage");
            fireButton.onClick.AddListener(DealFireDamage);

            Button coldButton = _panel.AddButton("Deal Cold Damage");
            coldButton.onClick.AddListener(DealColdDamage);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            bool burning = _entity.HasStatus<BurningStatusExample>();
            _panel.SetStatus(
                message +
                "\nHealth: " + _entity.CurrentHealth + "/" + _entity.MaxHealth +
                "\nBurning: " + burning +
                "\nLast result: " + _lastResult);
        }
    }
}
