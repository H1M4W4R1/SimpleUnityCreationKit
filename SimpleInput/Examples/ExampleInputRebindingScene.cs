using System;
using JetBrains.Annotations;
using Systems.SimpleCore.Examples;
using Systems.SimpleInput.Data;
using Systems.SimpleInput.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Systems.SimpleInput.Examples
{
    /// <summary>
    ///     Builds an interactive keyboard-rebinding panel without requiring SimpleSettings.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExampleInputRebindingScene : MonoBehaviour
    {
        private const int JUMP_ACTION_INDEX = 0;
        private const int INTERACT_ACTION_INDEX = 1;
        private const int SPRINT_ACTION_INDEX = 2;
        private const int CROUCH_ACTION_INDEX = 3;

        private static bool _inputApiInitialized;

        [CanBeNull] private InputActionAsset _inputAsset;
        [CanBeNull] private InputActionReference[] _actionReferences;
        [CanBeNull] private Text[] _bindingTexts;
        [CanBeNull] private Text[] _changeButtonTexts;
        [CanBeNull] private ExampleRuntimePanel _panel;
        private int _activeActionIndex = -1;

        private void Awake()
        {
            InitializeInputApi();
            CreateInputActions();
        }

        private void Start()
        {
            CreateRuntimeUI();
            RefreshBindings();
            SetStatus("Choose an action, then press a keyboard key. Escape cancels a change.");
        }

        private void OnDestroy()
        {
            if (!ReferenceEquals(_actionReferences, null))
            {
                for (int actionIndex = 0; actionIndex < _actionReferences.Length; actionIndex++)
                {
                    InputActionReference actionReference = _actionReferences[actionIndex];
                    if (!ReferenceEquals(actionReference, null)) Destroy(actionReference);
                }
            }

            if (!ReferenceEquals(_inputAsset, null)) Destroy(_inputAsset);
        }

        private static void InitializeInputApi()
        {
            if (_inputApiInitialized) return;

            InputAPI.Initialize();
            _inputApiInitialized = true;
        }

        private void CreateInputActions()
        {
            _inputAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            _inputAsset.name = "SimpleInput Rebinding Example Actions";

            InputActionMap gameplayMap = new InputActionMap("Gameplay");
            InputAction jumpAction = gameplayMap.AddAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            InputAction interactAction = gameplayMap.AddAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/e");
            InputAction sprintAction = gameplayMap.AddAction("Sprint", InputActionType.Button);
            sprintAction.AddBinding("<Keyboard>/leftShift");
            InputAction crouchAction = gameplayMap.AddAction("Crouch", InputActionType.Button);
            crouchAction.AddBinding("<Keyboard>/c");
            _inputAsset.AddActionMap(gameplayMap);

            _actionReferences = new InputActionReference[]
            {
                InputActionReference.Create(jumpAction),
                InputActionReference.Create(interactAction),
                InputActionReference.Create(sprintAction),
                InputActionReference.Create(crouchAction)
            };
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleInput Key Rebinding",
                "This scene uses SimpleInput directly. Select Change, press any keyboard key, or press Escape to cancel.");
            _panel.AddSection("Keyboard Bindings");

            _bindingTexts = new Text[4];
            _changeButtonTexts = new Text[4];

            AddBindingControl(JUMP_ACTION_INDEX, "Jump", ChangeJumpKey);
            AddBindingControl(INTERACT_ACTION_INDEX, "Interact", ChangeInteractKey);
            AddBindingControl(SPRINT_ACTION_INDEX, "Sprint", ChangeSprintKey);
            AddBindingControl(CROUCH_ACTION_INDEX, "Crouch", ChangeCrouchKey);

            _panel.AddSection("Defaults");
            Button resetButton = _panel.AddButton("Reset All Keys to Defaults");
            resetButton.onClick.AddListener(ResetAllKeys);
        }

        private void AddBindingControl(int actionIndex, string actionName, UnityEngine.Events.UnityAction changeAction)
        {
            Text bindingText = _panel.AddBodyText(actionName + " Key: ");
            _bindingTexts[actionIndex] = bindingText;

            Button changeButton = _panel.AddButton("Change " + actionName + " Key");
            changeButton.onClick.AddListener(changeAction);
            _changeButtonTexts[actionIndex] = changeButton.GetComponentInChildren<Text>();
        }

        private void ChangeJumpKey() => StartRebind(JUMP_ACTION_INDEX);

        private void ChangeInteractKey() => StartRebind(INTERACT_ACTION_INDEX);

        private void ChangeSprintKey() => StartRebind(SPRINT_ACTION_INDEX);

        private void ChangeCrouchKey() => StartRebind(CROUCH_ACTION_INDEX);

        private void StartRebind(int actionIndex)
        {
            if (!IsValidActionIndex(actionIndex)) return;

            if (_activeActionIndex >= 0)
            {
                SetStatus("A key change is already in progress. Press a key or Escape first.");
                return;
            }

            InputActionReference actionReference = _actionReferences[actionIndex];
            bool started = actionReference.Rebind(
                InputDeviceType.Keyboard,
                OnBindingChangeStarted,
                OnBindingChangeCancelled,
                OnBindingChangeCompleted,
                OnBindingDuplicateFound);
            if (!started)
            {
                SetStatus("Could not start changing " + actionReference.action.name + " key.");
                return;
            }

            _activeActionIndex = actionIndex;
            SetChangeButtonText(actionIndex, "Press any key...");
            SetStatus("Changing " + actionReference.action.name + " key. Press any keyboard key, or Escape to cancel.");
        }

        private void ResetAllKeys()
        {
            if (_activeActionIndex >= 0)
            {
                SetStatus("Finish or cancel the current key change before resetting defaults.");
                return;
            }

            bool allReset = true;
            for (int actionIndex = 0; actionIndex < _actionReferences.Length; actionIndex++)
            {
                bool reset = _actionReferences[actionIndex].ResetToDefault(0, InputDeviceType.Keyboard);
                allReset &= reset;
            }

            RefreshBindings();
            SetStatus(allReset ? "All keys reset to their defaults." : "Some keys could not be reset because of a duplicate.");
        }

        private void OnBindingChangeStarted(BindingChangeInfo bindingChangeInfo)
        {
            int actionIndex = GetActionIndex(bindingChangeInfo.action);
            if (actionIndex < 0) return;

            _activeActionIndex = actionIndex;
            SetChangeButtonText(actionIndex, "Press any key...");
        }

        private void OnBindingChangeCancelled(BindingChangeInfo bindingChangeInfo)
        {
            int actionIndex = GetActionIndex(bindingChangeInfo.action);
            if (actionIndex < 0) return;

            FinishRebind(actionIndex);
            SetStatus("Change cancelled. " + bindingChangeInfo.action.name + " remains " + GetBindingName(actionIndex) + ".");
        }

        private void OnBindingChangeCompleted(BindingChangeInfo bindingChangeInfo)
        {
            int actionIndex = GetActionIndex(bindingChangeInfo.action);
            if (actionIndex < 0) return;

            FinishRebind(actionIndex);
            string bindingName = GetBindingName(actionIndex);
            SetStatus("Changed " + bindingChangeInfo.action.name + " to " + bindingName + ".");
        }

        private void OnBindingDuplicateFound(BindingChangeInfo bindingChangeInfo)
        {
            int actionIndex = GetActionIndex(bindingChangeInfo.action);
            if (actionIndex < 0) return;

            FinishRebind(actionIndex);
            SetStatus(GetBindingName(actionIndex) + " is already assigned. " + bindingChangeInfo.action.name + " was not changed.");
        }

        private void FinishRebind(int actionIndex)
        {
            _activeActionIndex = -1;
            RefreshBindings();
            SetChangeButtonText(actionIndex, "Change " + _actionReferences[actionIndex].action.name + " Key");
        }

        private void RefreshBindings()
        {
            if (ReferenceEquals(_bindingTexts, null)) return;

            for (int actionIndex = 0; actionIndex < _bindingTexts.Length; actionIndex++)
            {
                Text bindingText = _bindingTexts[actionIndex];
                if (ReferenceEquals(bindingText, null) || !bindingText) continue;

                InputActionReference actionReference = _actionReferences[actionIndex];
                bindingText.text = actionReference.action.name + " Key: " + GetBindingName(actionIndex);
            }
        }

        [NotNull] private string GetBindingName(int actionIndex)
        {
            InputActionReference actionReference = _actionReferences[actionIndex];
            string bindingName = actionReference.GetBindingDisplayName(
                true,
                false,
                InputDeviceType.Keyboard);
            return string.IsNullOrEmpty(bindingName) ? "Unassigned" : bindingName;
        }

        private int GetActionIndex([NotNull] InputAction action)
        {
            for (int actionIndex = 0; actionIndex < _actionReferences.Length; actionIndex++)
            {
                if (ReferenceEquals(_actionReferences[actionIndex].action, action)) return actionIndex;
            }

            return -1;
        }

        private bool IsValidActionIndex(int actionIndex)
        {
            return !ReferenceEquals(_actionReferences, null) &&
                   actionIndex >= 0 &&
                   actionIndex < _actionReferences.Length;
        }

        private void SetChangeButtonText(int actionIndex, string value)
        {
            if (ReferenceEquals(_changeButtonTexts, null)) return;
            if (actionIndex < 0 || actionIndex >= _changeButtonTexts.Length) return;

            Text changeButtonText = _changeButtonTexts[actionIndex];
            if (!ReferenceEquals(changeButtonText, null) && changeButtonText)
                changeButtonText.text = value;
        }

        private void SetStatus(string value)
        {
            if (ReferenceEquals(_panel, null) || !_panel) return;
            _panel.SetStatus(value);
        }
    }
}
