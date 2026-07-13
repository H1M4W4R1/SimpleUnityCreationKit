using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Systems.SimpleBuilding.Examples
{
    /// <summary>
    ///     Interactive controls for the Building Playground scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExampleBuildingScene : MonoBehaviour, IBuildingUser
    {
        [SerializeField] [CanBeNull] private ExampleBuildingEntry _freeBuildingEntry;
        [SerializeField] [CanBeNull] private ExampleBuildingEntry _slotBuildingEntry;
        [SerializeField] [CanBeNull] private PointerBuildingRaycaster _raycaster;
        [CanBeNull] private ExampleRuntimePanel _panel;
        private string _lastResult = "Ready";

        public void Configure(
            [NotNull] ExampleBuildingEntry freeBuildingEntry,
            [NotNull] ExampleBuildingEntry slotBuildingEntry,
            [NotNull] PointerBuildingRaycaster raycaster)
        {
            _freeBuildingEntry = freeBuildingEntry;
            _slotBuildingEntry = slotBuildingEntry;
            _raycaster = raycaster;
        }

        private void Start()
        {
            CreateRuntimeUI();
            SelectFreeBuilding();
        }

        private void Update()
        {
            EventSystem eventSystem = EventSystem.current;
            if (!ReferenceEquals(eventSystem, null) && eventSystem.IsPointerOverGameObject()) return;
            if (Input.GetMouseButtonDown(0)) TryBuild();
            if (Input.GetMouseButtonDown(1)) TryDemolish();
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleBuilding Playground",
                "Choose a building and rotate it here. Left-click the world to build; right-click a completed building to demolish.");
            _panel.AddSection("Building Type");
            Button freeBuildingButton = _panel.AddButton("Select Free Building");
            freeBuildingButton.onClick.AddListener(SelectFreeBuilding);
            Button slotBuildingButton = _panel.AddButton("Select One-Slot Building");
            slotBuildingButton.onClick.AddListener(SelectSlotBuilding);
            _panel.AddSection("Rotation");
            Button rotateLeftButton = _panel.AddButton("Rotate Left");
            rotateLeftButton.onClick.AddListener(RotateLeft);
            Button rotateRightButton = _panel.AddButton("Rotate Right");
            rotateRightButton.onClick.AddListener(RotateRight);
            _panel.AddBodyText("The cylinder snaps to raised slot tiles. The cube uses free placement.");
            RefreshStatus();
        }

        private void SelectFreeBuilding()
        {
            if (ReferenceEquals(_raycaster, null) || !_raycaster) return;
            if (ReferenceEquals(_freeBuildingEntry, null) || !_freeBuildingEntry) return;
            SetResult(_raycaster.Select(_freeBuildingEntry, this));
        }

        private void SelectSlotBuilding()
        {
            if (ReferenceEquals(_raycaster, null) || !_raycaster) return;
            if (ReferenceEquals(_slotBuildingEntry, null) || !_slotBuildingEntry) return;
            SetResult(_raycaster.Select(_slotBuildingEntry, this));
        }

        private void RotateLeft() => Rotate(-1);

        private void RotateRight() => Rotate(1);

        private void Rotate(int steps)
        {
            if (ReferenceEquals(_raycaster, null) || !_raycaster) return;
            _raycaster.Rotate(steps);
            _lastResult = "Rotation: " + _raycaster.RotationDegrees + " degrees";
            RefreshStatus();
        }

        private void TryBuild()
        {
            if (ReferenceEquals(_raycaster, null) || !_raycaster) return;
            SetResult(_raycaster.TryBuild(out BuildingBase building, this));
        }

        private void TryDemolish()
        {
            if (ReferenceEquals(_raycaster, null) || !_raycaster) return;
            SetResult(_raycaster.TryDemolishTarget(this));
        }

        private void SetResult(in OperationResult result)
        {
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (ReferenceEquals(_panel, null) || !_panel) return;

            string selectedEntryName = ReferenceEquals(_raycaster, null) || !_raycaster ||
                                       ReferenceEquals(_raycaster.SelectedEntry, null)
                ? "none"
                : _raycaster.SelectedEntry.name;
            _panel.SetStatus(
                "Selected: " + selectedEntryName +
                "\nRotation: " + (ReferenceEquals(_raycaster, null) || !_raycaster ? 0f : _raycaster.RotationDegrees) +
                " degrees\nLast result: " + _lastResult);
        }
    }
}
