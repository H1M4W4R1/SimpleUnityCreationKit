using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleUI.Components.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Buttons;
using Systems.SimpleUI.Components.Canvases;
using Systems.SimpleUI.Components.Features.Drag;
using Systems.SimpleUI.Components.Features.Positioning;
using Systems.SimpleUI.Components.InputFields;
using Systems.SimpleUI.Components.Lists;
using Systems.SimpleUI.Components.Progress;
using Systems.SimpleUI.Components.Scrolling;
using Systems.SimpleUI.Components.Selectors.Abstract;
using Systems.SimpleUI.Components.Selectors.Implementations.Dropdown;
using Systems.SimpleUI.Components.Selectors.Tabs;
using Systems.SimpleUI.Components.Sliders;
using Systems.SimpleUI.Components.Text;
using Systems.SimpleUI.Components.Toggles;
using Systems.SimpleUI.Components.Tooltips;
using Systems.SimpleUI.Components.Windows;
using Systems.SimpleUI.Context.Abstract;
using Systems.SimpleUI.Context.Lists;
using Systems.SimpleUI.Context.Selectors;
using Systems.SimpleUI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Systems.SimpleUI.Tests
{
    internal static class SimpleUITestFixtures
    {
        internal static void ResetScene()
        {
            UserInterface.ClearTestState();

            GameObject[] gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            for (int index = 0; index < gameObjects.Length; index++)
            {
                GameObject gameObject = gameObjects[index];
                if (!gameObject) continue;
                Object.DestroyImmediate(gameObject);
            }
        }

        internal static GameObject CreateWindowCanvas()
        {
            return CreateRootCanvas<UIWindowCanvas>("Window Canvas");
        }

        internal static GameObject CreatePopupCanvas()
        {
            return CreateRootCanvas<UIPopupCanvas>("Popup Canvas");
        }

        internal static TCanvasType CreateCanvasComponent<TCanvasType>(string name)
            where TCanvasType : UIRootCanvasBase
        {
            GameObject root = CreateRootCanvas<TCanvasType>(name);
            return root.GetComponent<TCanvasType>();
        }

        internal static TWindowType CreateWindowPrefab<TWindowType>(string name)
            where TWindowType : UIWindowBase
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            TWindowType window = gameObject.AddComponent<TWindowType>();
            InitializeRecursively(gameObject);
            gameObject.SetActive(false);
            return window;
        }

        internal static TComponent CreateUIComponent<TComponent>(string name, Transform parent)
            where TComponent : Component
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            TComponent component = gameObject.AddComponent<TComponent>();
            InitializeRecursively(gameObject);
            return component;
        }

        internal static TWindowType GetOpenWindow<TWindowType>()
            where TWindowType : UIWindowBase
        {
            for (int index = 0; index < UserInterface.OpenWindows.Count; index++)
            {
                if (UserInterface.OpenWindows[index] is TWindowType window) return window;
            }

            Assert.Fail($"Open window of type {typeof(TWindowType).Name} was not found.");
            return null;
        }

        internal static void ValidateRecursively(GameObject root)
        {
            InitializeRecursively(root);
        }

        internal static void InitializeRecursively(GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponents<MonoBehaviour>();
            for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
            {
                MonoBehaviour behaviour = behaviours[behaviourIndex];
                if (!behaviour) continue;

                if (behaviour is UIObjectBase uiObject)
                {
                    uiObject.InitializeForTests();
                    continue;
                }

                if (behaviour is TestDragFeature dragFeature)
                {
                    dragFeature.InitializeForTests();
                    continue;
                }

                if (behaviour is TestSlotFeature slotFeature)
                {
                    slotFeature.InitializeForTests();
                }
            }

            Transform transform = root.transform;
            for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                InitializeRecursively(transform.GetChild(childIndex).gameObject);
            }
        }

        internal static PointerEventData CreatePointerEvent(Vector2 position)
        {
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (!eventSystem)
            {
                GameObject eventSystemObject = new GameObject("Event System", typeof(EventSystem),
                    typeof(StandaloneInputModule));
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }

            PointerEventData eventData = new PointerEventData(eventSystem)
            {
                position = position
            };
            return eventData;
        }

        private static GameObject CreateRootCanvas<TCanvasType>(string name)
            where TCanvasType : UIRootCanvasBase
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            RectTransform rectTransform = root.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(800f, 600f);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<TCanvasType>();
            return root;
        }
    }

    internal class TestWindowBase : UIWindowBase
    {
        internal bool Openable = true;
        internal bool Closable = true;
        internal int OpenedCount;
        internal int ClosedCount;

        public override bool CanBeOpened => Openable;
        public override bool CanBeClosed => Closable;
        internal int SortingOrder => GetComponent<Canvas>().sortingOrder;
        internal int DependentCount => Dependents.Count;
        internal object ExposedContext => WindowContext;

        protected internal override void OnWindowOpened()
        {
            OpenedCount++;
        }

        protected internal override void OnWindowClosed()
        {
            ClosedCount++;
        }
    }

    internal sealed class TestWindow : TestWindowBase
    {
    }

    internal sealed class TestSecondWindow : TestWindowBase
    {
    }

    internal sealed class TestThirdWindow : TestWindowBase
    {
    }

    internal sealed class TestNoMultipleDifferentContextWindow : TestWindowBase
    {
        public override bool AllowMultipleInstancesWithDifferentContext => false;
    }

    internal sealed class TestSameContextAllowedWindow : TestWindowBase
    {
        public override bool AllowMultipleInstancesWithSameContext => true;
    }

    internal sealed class TestPopup : UIPopupBase
    {
        internal int OpenedCount;
        internal int ClosedCount;

        protected internal override void OnWindowOpened()
        {
            OpenedCount++;
        }

        protected internal override void OnWindowClosed()
        {
            ClosedCount++;
            base.OnWindowClosed();
        }
    }

    internal sealed class TestSecondPopup : UIPopupBase
    {
        internal int OpenedCount;
        internal int ClosedCount;

        protected internal override void OnWindowOpened()
        {
            OpenedCount++;
        }

        protected internal override void OnWindowClosed()
        {
            ClosedCount++;
            base.OnWindowClosed();
        }
    }

    internal sealed class TestStringProvider : ContextProviderBase<string>
    {
        internal string Value = string.Empty;

        public override string GetContext()
        {
            return Value;
        }
    }

    internal sealed class TestTextObject : UITextObject
    {
        internal string RenderedText => TextReference.text;
    }

    internal sealed class TestLocalTextObject : UITextObject, IWithLocalContext<string>
    {
        internal string Value = string.Empty;
        internal string RenderedText => TextReference.text;

        public bool TryGetContext(out string context)
        {
            context = Value;
            return true;
        }
    }

    internal sealed class TestVisibilityObject : UIObjectBase
    {
        internal void HideForTests()
        {
            Hide();
        }

        internal void ShowForTests()
        {
            Show();
        }
    }

    internal sealed class TestProgressObject : UIProgressBase, IWithLocalContext<float>
    {
        internal float Value;

        public bool TryGetContext(out float context)
        {
            context = Value;
            return true;
        }
    }

    internal sealed class TestButton : UIButtonBase
    {
        internal int ClickCount;

        protected override void OnClick()
        {
            ClickCount++;
        }
    }

    internal sealed class TestSlider : UISliderBase
    {
        internal float LastValue;
        internal int ChangeCount;

        protected override void OnSliderValueChanged(float newValue)
        {
            LastValue = newValue;
            ChangeCount++;
        }
    }

    internal sealed class TestScrollbar : UIScrollbar
    {
        internal float LastValue;
        internal int ChangeCount;

        protected override void OnScrollbarValueChanged(float value)
        {
            LastValue = value;
            ChangeCount++;
        }
    }

    internal sealed class TestInputField : UIInputFieldBase
    {
        internal string SelectedText;
        internal string ChangedText;
        internal string SubmittedText;
        internal string EndEditedText;
        internal string DeselectedText;
        internal string SelectionText;
        internal string EndSelectionText;
        internal int SelectionFrom;
        internal int SelectionTo;

        protected override void OnFieldSelected(string text)
        {
            SelectedText = text;
        }

        protected override void OnFieldValueChanged(string newText)
        {
            ChangedText = newText;
        }

        protected override void OnFieldSubmitted(string withText)
        {
            SubmittedText = withText;
        }

        protected override void OnFieldEndEdited(string currentText)
        {
            EndEditedText = currentText;
        }

        protected override void OnFieldDeselected(string text)
        {
            DeselectedText = text;
        }

        protected override void OnTextSelected(string text, int from, int to)
        {
            SelectionText = text;
            SelectionFrom = from;
            SelectionTo = to;
        }

        protected override void OnTextDeselected(string text, int from, int to)
        {
            EndSelectionText = text;
        }
    }

    internal sealed class TestToggle : UIToggleBase
    {
        internal bool LastValue;
        internal int ChangeCount;

        protected override void OnToggleValueChanged(bool newValue)
        {
            LastValue = newValue;
            ChangeCount++;
        }
    }

    internal sealed class TestToggleGroup : UIToggleGroupBase
    {
        internal int LastChangedIndex = -1;
        internal bool LastChangedValue;

        protected override void OnToggleValueChanged(int toggleIndex, bool newValue)
        {
            LastChangedIndex = toggleIndex;
            LastChangedValue = newValue;
        }

        internal void RefreshForTests()
        {
            RefreshToggleArray();
        }
    }

    internal sealed class TestListContext : ListContext<string>
    {
        internal TestListContext(IReadOnlyList<string> data) : base(data)
        {
        }
    }

    internal sealed class TestSelectableContext : SelectableContext<string>
    {
        internal int LastOldIndex = -1;
        internal int LastNewIndex = -1;

        internal TestSelectableContext(IReadOnlyList<string> data, int defaultIndex = -1) : base(data, defaultIndex)
        {
        }

        public override void OnSelectionChanged(int oldIndex, int newIndex)
        {
            LastOldIndex = oldIndex;
            LastNewIndex = newIndex;
        }
    }

    internal sealed class TestSelector : UISelectorBase<string>, IWithLocalContext<SelectableContext<string>>
    {
        internal TestSelectableContext ProvidedContext;
        internal int LastFrom = -1;
        internal int LastTo = -1;

        public bool TryGetContext(out SelectableContext<string> context)
        {
            context = ProvidedContext;
            return !ReferenceEquals(ProvidedContext, null);
        }

        protected override void OnSelectedIndexChanged(int from, int to)
        {
            base.OnSelectedIndexChanged(from, to);
            LastFrom = from;
            LastTo = to;
        }
    }

    internal sealed class TestDropdownSelector : UIDropdownSelectorBase<string>, IWithLocalContext<SelectableContext<string>>
    {
        internal TestSelectableContext ProvidedContext;

        public bool TryGetContext(out SelectableContext<string> context)
        {
            context = ProvidedContext;
            return !ReferenceEquals(ProvidedContext, null);
        }

        protected override string GetOptionLabel(string obj)
        {
            return $"Option {obj}";
        }

        internal void CompleteLateSetupForTests()
        {
            OnLateSetupComplete();
        }
    }

    internal sealed class TestTab : UITab
    {
        internal int SelectedCount;
        internal int DeselectedCount;

        protected internal override void OnTabSelected()
        {
            SelectedCount++;
        }

        protected internal override void OnTabDeselected()
        {
            DeselectedCount++;
        }
    }

    internal sealed class TestDragFeature : DragFeature<TestDragFeature>
    {
        internal bool AllowPick = true;

        protected internal override bool CanPickFrom(DropZoneFeature<TestDragFeature> zone)
        {
            return AllowPick && base.CanPickFrom(zone);
        }

        internal void InitializeForTests()
        {
            AssignComponents();
        }
    }

    internal sealed class TestSlotFeature : SlotFeature<TestDragFeature>
    {
        internal bool SwapAllowed = true;

        protected override bool AllowSwap
        {
            get => SwapAllowed;
            set => SwapAllowed = value;
        }

        internal void InitializeForTests()
        {
            AssignComponents();
        }
    }

    internal sealed class TestTooltip : UITooltipBase<string>
    {
        internal string RenderedContext;

        public override void OnRender(string withContext)
        {
            RenderedContext = withContext;
        }
    }

    internal sealed class TestTooltipFeature : UITooltipFeature<TestTooltip, string>
    {
        internal string Value = string.Empty;

        protected override string GetNewTooltipContext()
        {
            return Value;
        }
    }

    internal static class TestRectUtility
    {
        internal static RectTransform CreateRect(string name, Transform parent, Vector2 size, Vector2 anchoredPosition)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;
            return rectTransform;
        }

        internal static LimitObjectToParent AddParentLimiter(RectTransform rectTransform)
        {
            LimitObjectToParent limiter = rectTransform.gameObject.AddComponent<LimitObjectToParent>();
            return limiter;
        }

        internal static LimitObjectToViewport AddViewportLimiter(RectTransform rectTransform)
        {
            LimitObjectToViewport limiter = rectTransform.gameObject.AddComponent<LimitObjectToViewport>();
            return limiter;
        }
    }
}
