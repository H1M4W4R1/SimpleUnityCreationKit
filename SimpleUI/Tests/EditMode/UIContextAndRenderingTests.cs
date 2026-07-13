using NUnit.Framework;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Progress;
using Systems.SimpleUI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SimpleUI.Tests
{
    public sealed class UIContextAndRenderingTests
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
        public void ProvideContextForReturnsProviderContext()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestStringProvider provider = root.AddComponent<TestStringProvider>();
            provider.Value = "provided";

            TestTextObject textObject = SimpleUITestFixtures.CreateUIComponent<TestTextObject>("Text", root.transform);
            SimpleUITestFixtures.ValidateRecursively(root);

            string context = ((IWithContext) textObject).ProvideContextFor<string>();

            Assert.AreEqual("provided", context);
        }

        [Test]
        public void LocalContextOverridesParentProvider()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestStringProvider provider = root.AddComponent<TestStringProvider>();
            provider.Value = "provided";

            TestLocalTextObject textObject =
                SimpleUITestFixtures.CreateUIComponent<TestLocalTextObject>("Local Text", root.transform);
            textObject.Value = "local";
            SimpleUITestFixtures.ValidateRecursively(root);

            string context = ((IWithContext) textObject).ProvideContextFor<string>();

            Assert.AreEqual("local", context);
        }

        [Test]
        public void TextObjectRendersContextWhenRefreshIsRequested()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestLocalTextObject textObject =
                SimpleUITestFixtures.CreateUIComponent<TestLocalTextObject>("Rendered Text", root.transform);
            textObject.Value = "hello ui";
            SimpleUITestFixtures.ValidateRecursively(root);

            textObject.OnRender(((IWithContext) textObject).ProvideContextFor<string>());

            Assert.AreEqual("hello ui", textObject.RenderedText);
        }

        [Test]
        public void TextObjectCanRenderNullContextAsEmptyText()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestTextObject textObject = SimpleUITestFixtures.CreateUIComponent<TestTextObject>("Null Text", root.transform);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();

            textObject.OnRender(null);

            Assert.AreEqual(string.Empty, text.text);
        }

        [Test]
        public void UIObjectHideWithoutAnimationDeactivatesGameObject()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestVisibilityObject visibilityObject =
                SimpleUITestFixtures.CreateUIComponent<TestVisibilityObject>("Visibility", root.transform);

            visibilityObject.HideForTests();

            Assert.IsFalse(visibilityObject.gameObject.activeSelf);
            Assert.IsFalse(visibilityObject.IsVisible);

            visibilityObject.ShowForTests();

            Assert.IsTrue(visibilityObject.gameObject.activeSelf);
            Assert.IsTrue(visibilityObject.IsVisible);
        }

        [Test]
        public void ProgressObjectRendersClampedProgressToEveryImage()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            GameObject progressObject = new GameObject("Progress", typeof(RectTransform));
            progressObject.SetActive(false);
            progressObject.transform.SetParent(root.transform, false);

            GameObject firstFill = new GameObject("First Fill", typeof(RectTransform), typeof(Image));
            firstFill.transform.SetParent(progressObject.transform, false);
            UIProgressImage firstImage = firstFill.AddComponent<UIProgressImage>();

            GameObject secondFill = new GameObject("Second Fill", typeof(RectTransform), typeof(Image));
            secondFill.transform.SetParent(progressObject.transform, false);
            UIProgressImage secondImage = secondFill.AddComponent<UIProgressImage>();
            TestProgressObject progress = progressObject.AddComponent<TestProgressObject>();

            SimpleUITestFixtures.ValidateRecursively(root);

            progress.OnRender(1.5f);

            Assert.AreEqual(Image.Type.Filled, firstFill.GetComponent<Image>().type);
            Assert.AreEqual(1f, firstFill.GetComponent<Image>().fillAmount);
            Assert.AreEqual(1f, secondFill.GetComponent<Image>().fillAmount);
            Assert.IsTrue(firstImage);
            Assert.IsTrue(secondImage);

            progress.OnRender(-0.25f);

            Assert.AreEqual(0f, firstFill.GetComponent<Image>().fillAmount);
            Assert.AreEqual(0f, secondFill.GetComponent<Image>().fillAmount);
        }

        [Test]
        public void ProgressObjectMarksDirtyWhenProgressValueChanges()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            GameObject progressObject = new GameObject("Progress", typeof(RectTransform));
            progressObject.SetActive(false);
            progressObject.transform.SetParent(root.transform, false);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(progressObject.transform, false);
            fill.AddComponent<UIProgressImage>();
            TestProgressObject progress = progressObject.AddComponent<TestProgressObject>();

            SimpleUITestFixtures.ValidateRecursively(root);

            progress.Value = 0.25f;
            progress.OnRender(0.25f);
            ((IWithContext) progress).SetDirty(false);

            progress.ValidateContext();
            Assert.IsFalse(((IWithContext) progress).IsDirty);

            progress.Value = 0.75f;
            progress.ValidateContext();
            Assert.IsTrue(((IWithContext) progress).IsDirty);
        }
    }
}
