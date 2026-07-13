using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Core;
using Systems.SimpleSettings.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Systems.SimpleSettings.Examples.Scripts
{
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class ExampleSettingsScene : MonoBehaviour
    {
        [SerializeField] private SettingsManager _manager;
        [SerializeField] private bool _createRuntimeUI = true;
        [SerializeField] private int _previewDifficulty = 4;
        [SerializeField] private int _appliedDifficulty = 3;

        private ExampleGameplaySettingsGroup _gameplayGroup;
        private ExampleDifficultySetting _difficultySetting;
        private ExampleHintsSetting _hintsSetting;
        private Slider _difficultySlider;
        private Toggle _hintsToggle;
        private TextMeshProUGUI _statusText;
        private TextMeshProUGUI _difficultyValueText;
        private bool _isSyncingUI;

        private void Awake()
        {
            if (ReferenceEquals(_manager, null))
                _manager = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);

            if (!_manager)
            {
                GameObject managerObject = new GameObject("Settings Manager");
                _manager = managerObject.AddComponent<SettingsManager>();
            }

            _gameplayGroup = new ExampleGameplaySettingsGroup();
            _manager.RegisterGroup(_gameplayGroup);
            _difficultySetting = _gameplayGroup.Difficulty;
            _hintsSetting = _gameplayGroup.Hints;
        }

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            AttachSettingRefreshEvents();
            RunExample();
            SyncUIFromSettings();
        }

        private void OnDestroy()
        {
            DetachSettingRefreshEvents();
        }

        [ContextMenu("Run Settings Example")]
        public void RunExample()
        {
            ExampleDifficultySetting difficulty = SettingsAPI.GetSetting<ExampleDifficultySetting>();
            if (ReferenceEquals(difficulty, null))
            {
                Debug.LogWarning("[SimpleSettings] Example difficulty setting is not registered.");
                return;
            }

            difficulty.Set(_previewDifficulty);
            bool undoResult = SettingsAPI.TryUndo(ExampleGameplaySettingsGroup.GROUP_ID);
            difficulty.Set(_appliedDifficulty);
            SettingsAPI.Apply(ExampleGameplaySettingsGroup.GROUP_ID);
            RefreshStatus();

            Debug.Log("[SimpleSettings] Undo result: " + undoResult + ", current difficulty: " + difficulty.CurrentValue + ", applied difficulty: " + difficulty.AppliedValue);
        }

        private void CreateRuntimeUI()
        {
            EnsureEventSystem();

            GameObject canvasObject = new GameObject("SimpleSettings Example UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Transform canvasTransform = canvasObject.transform;
            RectTransform panel = CreateRect("Settings Panel", canvasTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 430f));
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color32(24, 31, 42, 235);

            CreateText("Title", panel, "SimpleSettings Example", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(-64f, 46f), 32, FontStyles.Bold, TextAlignmentOptions.Center, new Color32(245, 247, 250, 255));
            CreateText("Subtitle", panel, "Preview values, undo changes, then apply or revert the group.", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(-80f, 34f), 18, FontStyles.Normal, TextAlignmentOptions.Center, new Color32(181, 191, 205, 255));

            CreateText("Difficulty Label", panel, "Difficulty", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(52f, -136f), new Vector2(180f, 32f), 22, FontStyles.Bold, TextAlignmentOptions.Left, new Color32(245, 247, 250, 255));
            _difficultyValueText = CreateText("Difficulty Value", panel, string.Empty, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-52f, -136f), new Vector2(120f, 32f), 22, FontStyles.Bold, TextAlignmentOptions.Right, new Color32(110, 203, 255, 255));
            _difficultySlider = CreateSlider(panel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -184f), new Vector2(-104f, 34f));
            _difficultySlider.onValueChanged.AddListener(OnDifficultySliderChanged);

            CreateText("Hints Label", panel, "Hints", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(52f, -236f), new Vector2(180f, 32f), 22, FontStyles.Bold, TextAlignmentOptions.Left, new Color32(245, 247, 250, 255));
            _hintsToggle = CreateToggle(panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-52f, -232f), new Vector2(120f, 42f));
            _hintsToggle.onValueChanged.AddListener(OnHintsToggleChanged);

            RectTransform buttonRow = CreateRect("Actions", panel, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 82f), new Vector2(-80f, 48f));
            Button applyButton = CreateButton("Apply", buttonRow, new Vector2(0f, 0.5f), new Vector2(0.25f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(-10f, 44f));
            applyButton.onClick.AddListener(ApplyExampleSettings);
            Button revertButton = CreateButton("Revert", buttonRow, new Vector2(0.25f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(-10f, 44f));
            revertButton.onClick.AddListener(RevertExampleSettings);
            Button undoButton = CreateButton("Undo", buttonRow, new Vector2(0.5f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(-10f, 44f));
            undoButton.onClick.AddListener(UndoExampleSettings);
            Button resetButton = CreateButton("Reset", buttonRow, new Vector2(0.75f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(-10f, 44f));
            resetButton.onClick.AddListener(ResetExampleSettings);

            _statusText = CreateText("Status", panel, string.Empty, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(-80f, 34f), 17, FontStyles.Normal, TextAlignmentOptions.Center, new Color32(210, 218, 230, 255));
        }

        private void EnsureEventSystem()
        {
            EventSystem eventSystem = FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (!ReferenceEquals(eventSystem, null))
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private void AttachSettingRefreshEvents()
        {
            if (!ReferenceEquals(_difficultySetting, null))
            {
                _difficultySetting.OnValueChanged += OnSettingChanged;
                _difficultySetting.OnApplied += RefreshStatus;
            }

            if (!ReferenceEquals(_hintsSetting, null))
            {
                _hintsSetting.OnValueChanged += OnSettingChanged;
                _hintsSetting.OnApplied += RefreshStatus;
            }
        }

        private void DetachSettingRefreshEvents()
        {
            if (!ReferenceEquals(_difficultySetting, null))
            {
                _difficultySetting.OnValueChanged -= OnSettingChanged;
                _difficultySetting.OnApplied -= RefreshStatus;
            }

            if (!ReferenceEquals(_hintsSetting, null))
            {
                _hintsSetting.OnValueChanged -= OnSettingChanged;
                _hintsSetting.OnApplied -= RefreshStatus;
            }
        }

        private void OnSettingChanged()
        {
            SyncUIFromSettings();
            RefreshStatus();
        }

        private void OnDifficultySliderChanged(float value)
        {
            if (_isSyncingUI || ReferenceEquals(_difficultySetting, null))
            {
                return;
            }

            _difficultySetting.Set(Mathf.RoundToInt(value));
        }

        private void OnHintsToggleChanged(bool value)
        {
            if (_isSyncingUI || ReferenceEquals(_hintsSetting, null))
            {
                return;
            }

            _hintsSetting.Set(value);
        }

        private void ApplyExampleSettings()
        {
            SettingsAPI.Apply(ExampleGameplaySettingsGroup.GROUP_ID);
            RefreshStatus();
        }

        private void RevertExampleSettings()
        {
            SettingsAPI.Revert(ExampleGameplaySettingsGroup.GROUP_ID);
            SyncUIFromSettings();
            RefreshStatus();
        }

        private void UndoExampleSettings()
        {
            SettingsAPI.TryUndo(ExampleGameplaySettingsGroup.GROUP_ID);
            SyncUIFromSettings();
            RefreshStatus();
        }

        private void ResetExampleSettings()
        {
            SettingsAPI.ResetToDefaults(ExampleGameplaySettingsGroup.GROUP_ID);
            SyncUIFromSettings();
            RefreshStatus();
        }

        private void SyncUIFromSettings()
        {
            _isSyncingUI = true;

            if (!ReferenceEquals(_difficultySlider, null) && !ReferenceEquals(_difficultySetting, null))
            {
                _difficultySlider.SetValueWithoutNotify(_difficultySetting.CurrentValue);
            }

            if (!ReferenceEquals(_hintsToggle, null) && !ReferenceEquals(_hintsSetting, null))
            {
                _hintsToggle.SetIsOnWithoutNotify(_hintsSetting.CurrentValue);
            }

            _isSyncingUI = false;
        }

        private void RefreshStatus()
        {
            if (!ReferenceEquals(_difficultyValueText, null) && !ReferenceEquals(_difficultySetting, null))
            {
                _difficultyValueText.text = _difficultySetting.CurrentValue.ToString();
            }

            if (ReferenceEquals(_statusText, null) || ReferenceEquals(_difficultySetting, null) || ReferenceEquals(_hintsSetting, null))
            {
                return;
            }

            string dirtyState = _gameplayGroup.IsDirty ? "Pending changes" : "Applied";
            string hintsState = _hintsSetting.CurrentValue ? "on" : "off";
            _statusText.text = dirtyState + " | Applied difficulty " + _difficultySetting.AppliedValue + " | Hints " + hintsState;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);

            RectTransform rect = rectObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return rect;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = alignment;
            label.color = color;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static Slider CreateSlider(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform root = CreateRect("Difficulty Slider", parent, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta);
            Slider slider = root.gameObject.AddComponent<Slider>();
            slider.minValue = 1f;
            slider.maxValue = 5f;
            slider.wholeNumbers = true;

            RectTransform background = CreateRect("Background", root, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 10f));
            Image backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.color = new Color32(65, 75, 89, 255);

            RectTransform fillArea = CreateRect("Fill Area", root, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-24f, 10f));
            RectTransform fill = CreateRect("Fill", fillArea, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = new Color32(78, 171, 255, 255);

            RectTransform handleArea = CreateRect("Handle Slide Area", root, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-24f, 0f));
            RectTransform handle = CreateRect("Handle", handleArea, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 28f));
            Image handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = new Color32(246, 248, 252, 255);

            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            return slider;
        }

        private static Toggle CreateToggle(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform root = CreateRect("Hints Toggle", parent, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta);
            Toggle toggle = root.gameObject.AddComponent<Toggle>();

            RectTransform background = CreateRect("Background", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(42f, 42f));
            Image backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.color = new Color32(65, 75, 89, 255);

            RectTransform checkmark = CreateRect("Checkmark", background, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f));
            Image checkmarkImage = checkmark.gameObject.AddComponent<Image>();
            checkmarkImage.color = new Color32(104, 222, 160, 255);

            CreateText("Value", root, "Enabled", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(54f, 0f), new Vector2(-54f, 0f), 18, FontStyles.Normal, TextAlignmentOptions.Left, new Color32(210, 218, 230, 255));

            toggle.targetGraphic = backgroundImage;
            toggle.graphic = checkmarkImage;
            return toggle;
        }

        private static Button CreateButton(string label, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform root = CreateRect(label + " Button", parent, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta);
            Image image = root.gameObject.AddComponent<Image>();
            image.color = new Color32(46, 63, 82, 255);

            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            CreateText("Label", root, label, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 18, FontStyles.Bold, TextAlignmentOptions.Center, new Color32(245, 247, 250, 255));
            return button;
        }
    }

    public sealed class ExampleDifficultySetting : Setting<int>, ISliderSetting
    {
        public ExampleDifficultySetting() : base(2)
        {
        }

        public float MinValue => 1f;
        public float MaxValue => 5f;
        public float Step => 1f;

        protected override void OnApplyInternal(int value)
        {
            Debug.Log("[SimpleSettings] Difficulty applied: " + value);
        }

        protected override void OnCurrentValueChanged(int value)
        {
            Debug.Log("[SimpleSettings] Difficulty preview: " + value);
        }
    }

    public sealed class ExampleHintsSetting : Setting<bool>, IToggleSetting
    {
        public ExampleHintsSetting() : base(true)
        {
        }

        protected override void OnApplyInternal(bool value)
        {
            Debug.Log("[SimpleSettings] Hints enabled: " + value);
        }
    }

    public sealed class ExampleGameplaySettingsGroup : SettingGroupBase
    {
        public const string GROUP_ID = "example-gameplay";

        private readonly ExampleDifficultySetting _difficulty = new ExampleDifficultySetting();
        private readonly ExampleHintsSetting _hints = new ExampleHintsSetting();
        private readonly ISetting[] _settings;

        public ExampleGameplaySettingsGroup()
        {
            _settings = new ISetting[] { _difficulty, _hints };
            RegisterSettings(_settings);
        }

        public ExampleDifficultySetting Difficulty => _difficulty;
        public ExampleHintsSetting Hints => _hints;
        public override string GroupId => GROUP_ID;
        public override string SaveFileName => "example-gameplay-settings";

        protected override IEnumerable<ISetting> GetSettings()
        {
            return _settings;
        }
    }
}
