using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using Systems.SimplePermissions.Components;
using Systems.SimplePermissions.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimplePermissions.Examples
{
    /// <summary>
    ///     Builds the SimpleCore runtime panel that exercises permission overrides and requirements.
    /// </summary>
    public sealed class PermissionExampleController : MonoBehaviour
    {
        [SerializeField] private PermissionStorage _permissionStorage;
        [SerializeField] private int _currentLevel = 1;

        private Text _permissionStateText;
        private Text _requirementStateText;
        private ExampleRuntimePanel _runtimePanel;

        private void Awake()
        {
            FindStorage();
        }

        private void Start()
        {
            if (ReferenceEquals(_permissionStorage, null) || !_permissionStorage) return;

            _runtimePanel = ExampleRuntimePanel.Create(
                "SimplePermissions",
                "Explicitly grant, deny, or revoke Build. Adjust the level to evaluate the generated requirement.");
            _runtimePanel.AddSection("Build permission");
            _permissionStateText = _runtimePanel.AddBodyText(string.Empty);
            Button grantButton = _runtimePanel.AddButton("Grant");
            grantButton.onClick.AddListener(GrantPermission);
            Button denyButton = _runtimePanel.AddButton("Deny");
            denyButton.onClick.AddListener(DenyPermission);
            Button revokeButton = _runtimePanel.AddButton("Revoke");
            revokeButton.onClick.AddListener(RevokePermission);

            _runtimePanel.AddSection("Minimum level requirement");
            _requirementStateText = _runtimePanel.AddBodyText(string.Empty);
            Button decreaseLevelButton = _runtimePanel.AddButton("Decrease level");
            decreaseLevelButton.onClick.AddListener(DecreaseLevel);
            Button increaseLevelButton = _runtimePanel.AddButton("Increase level");
            increaseLevelButton.onClick.AddListener(IncreaseLevel);
            RefreshDisplay();
        }

        private void OnValidate()
        {
            FindStorage();
        }

        private void GrantPermission()
        {
            OperationResult result = _permissionStorage.TryGrant<ExampleBuildPermission>();
            SetOperationStatus("Grant", in result);
        }

        private void DenyPermission()
        {
            OperationResult result = _permissionStorage.TryDeny<ExampleBuildPermission>();
            SetOperationStatus("Deny", in result);
        }

        private void RevokePermission()
        {
            OperationResult result = _permissionStorage.TryRevoke<ExampleBuildPermission>();
            SetOperationStatus("Revoke", in result);
        }

        private void DecreaseLevel()
        {
            _currentLevel = Mathf.Max(0, _currentLevel - 1);
            RefreshDisplay();
        }

        private void IncreaseLevel()
        {
            _currentLevel++;
            RefreshDisplay();
        }

        private void SetOperationStatus(string operation, in OperationResult result)
        {
            _runtimePanel.SetStatus(operation + ": " + ExampleRuntimePanel.FormatResult(in result));
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (!ReferenceEquals(_permissionStateText, null))
            {
                bool hasPermission = _permissionStorage.HasPermission<ExampleBuildPermission>();
                _permissionStateText.text = "Build is currently " + (hasPermission ? "allowed." : "denied.");
            }

            if (!ReferenceEquals(_requirementStateText, null))
            {
                bool isRequirementMet = RequirementAPI.IsMet<ExampleMinimumLevelRequirement, int>(_currentLevel);
                _requirementStateText.text = "Level " + _currentLevel + " / minimum 3: " +
                                             (isRequirementMet ? "requirement met." : "requirement not met.");
            }
        }

        private void FindStorage()
        {
            if (!ReferenceEquals(_permissionStorage, null)) return;

            PermissionStorage permissionStorage;
            if (!TryGetComponent(out permissionStorage)) return;
            _permissionStorage = permissionStorage;
        }
    }
}
