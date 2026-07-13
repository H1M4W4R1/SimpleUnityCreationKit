using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleBuilding.Operations;
using Systems.SimpleBuilding.Utility;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using UnityEngine;

namespace Systems.SimpleBuilding.Components
{
    /// <summary>
    ///     Projects a ray to a build surface, drives an optional ghost, and submits placement or demolition requests.
    /// </summary>
    public abstract class BuildingRaycasterBase : MonoBehaviour
    {
        [SerializeField] private LayerMask _raycastMask = Physics.DefaultRaycastLayers;
        [SerializeField] [Min(0.01f)] private float _maxRaycastDistance = 100f;
        [SerializeField] private bool _snapToGrid;
        [SerializeField] [Min(0.01f)] private float _gridSize = 1f;
        [SerializeField] [Min(1f)] private float _rotationIncrementDegrees = 90f;
        [SerializeField] [CanBeNull] private Transform _buildingParent;
        [SerializeField] [CanBeNull] private BuildingGhostPreview _ghostPreview;

        private readonly List<BuildingSlot> _placementSlots = new List<BuildingSlot>();
        [CanBeNull] private BuildingEntryBase _selectedEntry;
        private float _rotationDegrees;
        private RaycastHit _lastHit;
        private bool _hasRaycastHit;

        [CanBeNull] public BuildingEntryBase SelectedEntry => _selectedEntry;
        public float RotationDegrees => _rotationDegrees;
        public bool HasPlacementHit => _hasRaycastHit;

        protected virtual void Update()
        {
            RefreshPlacementPreview();
        }

        /// <summary>
        ///     Selects an entry and begins rendering its ghost where this controller's ray hits.
        /// </summary>
        public OperationResult Select(
            [CanBeNull] BuildingEntryBase entry,
            [CanBeNull] IBuildingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            OperationResult result = BuildingAPI.CanSelect(entry, user, this, actionSource);
            if (!result) return result;

            _selectedEntry = entry;
            _rotationDegrees = 0f;
            RefreshPlacementPreview(user, actionSource);
            return BuildingOperations.Permitted();
        }

        /// <summary>
        ///     Clears the selected entry and removes the transient ghost.
        /// </summary>
        public void ClearSelection()
        {
            _selectedEntry = null;
            _placementSlots.Clear();
            _hasRaycastHit = false;
            if (!ReferenceEquals(_ghostPreview, null) && _ghostPreview) _ghostPreview.Hide();
        }

        /// <summary>
        ///     Rotates the selected building by the configured increment. Negative steps rotate counter-clockwise.
        /// </summary>
        public void Rotate(int steps = 1)
        {
            _rotationDegrees = Mathf.Repeat(
                _rotationDegrees + _rotationIncrementDegrees * steps,
                360f);
            RefreshPlacementPreview();
        }

        /// <summary>
        ///     Sets the selected building's local-yaw angle in degrees.
        /// </summary>
        public void SetRotation(float rotationDegrees)
        {
            _rotationDegrees = Mathf.Repeat(rotationDegrees, 360f);
            RefreshPlacementPreview();
        }

        /// <summary>
        ///     Builds the selected entry at the current raycast result.
        /// </summary>
        public OperationResult TryBuild(
            [CanBeNull] out BuildingBase building,
            [CanBeNull] IBuildingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            building = null;
            if (ReferenceEquals(_selectedEntry, null) || !_selectedEntry)
                return BuildingOperations.EntryIsNull();
            if (!RefreshPlacementPreview(user, actionSource)) return BuildingOperations.RaycastNotHit();

            BuildingPlacementContext context = new BuildingPlacementContext(
                _selectedEntry,
                GetPlacementPosition(_lastHit.point),
                GetPlacementRotation(),
                user,
                this,
                _buildingParent,
                _placementSlots,
                actionSource);
            OperationResult result = BuildingAPI.TryBuild(in context, out building);
            if (result && !ReferenceEquals(_ghostPreview, null) && _ghostPreview)
                _ghostPreview.Hide();
            return result;
        }

        /// <summary>
        ///     Demolishes the first <see cref="BuildingBase"/> found on the object hit by this controller's ray.
        /// </summary>
        public OperationResult TryDemolishTarget(
            [CanBeNull] IBuildingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            if (!TryGetRaycastHit(out RaycastHit hit)) return BuildingOperations.RaycastNotHit();

            BuildingBase building = hit.collider.GetComponentInParent<BuildingBase>();
            if (ReferenceEquals(building, null)) return BuildingOperations.BuildingIsNull();
            return BuildingAPI.TryDemolish(building, user, this, actionSource);
        }

        protected abstract bool TryGetRay(out Ray ray);

        /// <summary>
        ///     Supplies candidate slots for the current hit. Override for grids or multi-slot footprints.
        /// </summary>
        protected virtual void CollectPlacementSlots(in RaycastHit hit, List<BuildingSlot> slots)
        {
            BuildingSlot slot = hit.collider.GetComponentInParent<BuildingSlot>();
            if (!ReferenceEquals(slot, null) && slot) slots.Add(slot);
        }

        protected virtual Vector3 GetPlacementPosition(Vector3 hitPosition)
        {
            if (!_snapToGrid) return hitPosition;

            float gridSize = Mathf.Max(0.01f, _gridSize);
            return new Vector3(
                Mathf.Round(hitPosition.x / gridSize) * gridSize,
                Mathf.Round(hitPosition.y / gridSize) * gridSize,
                Mathf.Round(hitPosition.z / gridSize) * gridSize);
        }

        protected virtual Quaternion GetPlacementRotation()
            => Quaternion.Euler(0f, _rotationDegrees, 0f);

        private bool RefreshPlacementPreview(
            [CanBeNull] IBuildingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            if (ReferenceEquals(_selectedEntry, null) || !_selectedEntry)
            {
                _hasRaycastHit = false;
                if (!ReferenceEquals(_ghostPreview, null) && _ghostPreview) _ghostPreview.Hide();
                return false;
            }

            if (!TryGetRaycastHit(out _lastHit))
            {
                _hasRaycastHit = false;
                if (!ReferenceEquals(_ghostPreview, null) && _ghostPreview) _ghostPreview.Hide();
                return false;
            }

            _hasRaycastHit = true;
            _placementSlots.Clear();
            CollectPlacementSlots(in _lastHit, _placementSlots);
            Vector3 position = GetPlacementPosition(_lastHit.point);
            Quaternion rotation = GetPlacementRotation();
            OperationResult result = BuildingAPI.CanBuild(
                _selectedEntry,
                position,
                rotation,
                user,
                this,
                _buildingParent,
                _placementSlots,
                actionSource);

            if (!ReferenceEquals(_ghostPreview, null) && _ghostPreview)
                _ghostPreview.Show(_selectedEntry, position, rotation, result);

            return true;
        }

        private bool TryGetRaycastHit(out RaycastHit hit)
        {
            hit = default;
            if (!TryGetRay(out Ray ray)) return false;
            return Physics.Raycast(ray, out hit, _maxRaycastDistance, _raycastMask, QueryTriggerInteraction.Ignore);
        }

        protected virtual void OnDestroy()
        {
            _placementSlots.Clear();
        }
    }
}
