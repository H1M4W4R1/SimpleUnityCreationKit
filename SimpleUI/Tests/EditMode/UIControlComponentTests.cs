using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleUI.Tests
{
    public sealed class UIControlComponentTests
    {
        [SetUp]
        public void SetUp()
        {
            SimpleUITestFixtures.ResetScene();
        }

        [TearDown]
        public void TearDown()
        {
            SimpleUITestFixtures.ResetScene();
        }

        [Test]
        public void ButtonInvokesCallbackAndTracksInteractableState()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestButton button = SimpleUITestFixtures.CreateUIComponent<TestButton>("Button", root.transform);
            Button unityButton = button.GetComponent<Button>();

            Assert.IsTrue(button.IsInteractable);
            button.MakeNonInteractable();
            Assert.IsFalse(button.IsInteractable);
            button.MakeInteractable();
            Assert.IsTrue(button.IsInteractable);

            unityButton.onClick.Invoke();

            Assert.AreEqual(1, button.ClickCount);
        }

        [Test]
        public void SliderInvokesCallbackAndTracksInteractableState()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestSlider slider = SimpleUITestFixtures.CreateUIComponent<TestSlider>("Slider", root.transform);
            Slider unitySlider = slider.GetComponent<Slider>();

            slider.MakeNonInteractable();
            Assert.IsFalse(slider.IsInteractable);
            slider.MakeInteractable();
            Assert.IsTrue(slider.IsInteractable);

            unitySlider.value = 0.75f;

            Assert.AreEqual(1, slider.ChangeCount);
            Assert.AreEqual(0.75f, slider.LastValue);
        }

        [Test]
        public void ScrollbarInvokesCallbackAndTracksInteractableState()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestScrollbar scrollbar = SimpleUITestFixtures.CreateUIComponent<TestScrollbar>("Scrollbar", root.transform);
            Scrollbar unityScrollbar = scrollbar.GetComponent<Scrollbar>();

            scrollbar.MakeNonInteractable();
            Assert.IsFalse(scrollbar.IsInteractable);
            scrollbar.MakeInteractable();
            Assert.IsTrue(scrollbar.IsInteractable);

            unityScrollbar.value = 0.35f;

            Assert.AreEqual(1, scrollbar.ChangeCount);
            Assert.AreEqual(0.35f, scrollbar.LastValue);
        }

        [Test]
        public void InputFieldWiresAllSupportedCallbacks()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestInputField inputField = SimpleUITestFixtures.CreateUIComponent<TestInputField>("Input", root.transform);
            TMP_InputField unityInputField = inputField.GetComponent<TMP_InputField>();

            inputField.MakeNonInteractable();
            Assert.IsFalse(inputField.IsInteractable);
            inputField.MakeInteractable();
            Assert.IsTrue(inputField.IsInteractable);

            unityInputField.onSelect.Invoke("selected");
            unityInputField.onValueChanged.Invoke("changed");
            unityInputField.onSubmit.Invoke("submitted");
            unityInputField.onEndEdit.Invoke("ended");
            unityInputField.onDeselect.Invoke("deselected");
            unityInputField.onTextSelection.Invoke("selection", 1, 4);
            unityInputField.onEndTextSelection.Invoke("end selection", 2, 5);

            Assert.AreEqual("selected", inputField.SelectedText);
            Assert.AreEqual("changed", inputField.ChangedText);
            Assert.AreEqual("submitted", inputField.SubmittedText);
            Assert.AreEqual("ended", inputField.EndEditedText);
            Assert.AreEqual("deselected", inputField.DeselectedText);
            Assert.AreEqual("selection", inputField.SelectionText);
            Assert.AreEqual(1, inputField.SelectionFrom);
            Assert.AreEqual(4, inputField.SelectionTo);
            Assert.AreEqual("end selection", inputField.EndSelectionText);
        }

        [Test]
        public void ToggleGroupDiscoversChildTogglesAndSelectsByIndex()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            GameObject groupObject = new GameObject("Toggle Group", typeof(RectTransform), typeof(CanvasGroup),
                typeof(ToggleGroup));
            groupObject.transform.SetParent(root.transform, false);
            groupObject.GetComponent<ToggleGroup>().allowSwitchOff = true;
            TestToggleGroup group = groupObject.AddComponent<TestToggleGroup>();

            TestToggle firstToggle = CreateToggle("First Toggle", groupObject.transform);
            TestToggle secondToggle = CreateToggle("Second Toggle", groupObject.transform);
            SimpleUITestFixtures.ValidateRecursively(root);
            group.RefreshForTests();

            Assert.AreEqual(2, group.Toggles.Count);
            Assert.IsTrue(group.SelectToggle(1));
            Assert.IsFalse(firstToggle.IsToggled);
            Assert.IsTrue(secondToggle.IsToggled);
            Assert.AreEqual(1, group.FirstToggleIndex);
            Assert.AreEqual(1, group.LastChangedIndex);
            Assert.IsTrue(group.LastChangedValue);

            group.SetToggled(1, false);
            Assert.IsFalse(group.IsToggled(1));
            Assert.IsFalse(group.SelectToggle(99));
        }

        [Test]
        public void ToggleGroupRefreshesWhenToggleIsDestroyed()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            GameObject groupObject = new GameObject("Toggle Group", typeof(RectTransform), typeof(CanvasGroup),
                typeof(ToggleGroup));
            groupObject.transform.SetParent(root.transform, false);
            groupObject.GetComponent<ToggleGroup>().allowSwitchOff = true;
            TestToggleGroup group = groupObject.AddComponent<TestToggleGroup>();

            TestToggle firstToggle = CreateToggle("First Toggle", groupObject.transform);
            CreateToggle("Second Toggle", groupObject.transform);
            SimpleUITestFixtures.ValidateRecursively(root);
            group.RefreshForTests();

            Assert.AreEqual(2, group.Toggles.Count);

            Object.DestroyImmediate(firstToggle.gameObject);
            group.RefreshForTests();

            Assert.AreEqual(1, group.Toggles.Count);
        }

        private static TestToggle CreateToggle(string name, Transform parent)
        {
            GameObject toggleObject = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            toggleObject.transform.SetParent(parent, false);
            toggleObject.GetComponent<Toggle>().isOn = false;
            TestToggle toggle = toggleObject.AddComponent<TestToggle>();
            return toggle;
        }
    }
}
