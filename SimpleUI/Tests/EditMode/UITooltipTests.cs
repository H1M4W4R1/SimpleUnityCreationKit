using NUnit.Framework;
using Systems.SimpleUI.Components.Animations;
using UnityEngine;

namespace Systems.SimpleUI.Tests
{
    public sealed class UITooltipTests
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
        public void ExitingPreviousFeatureDoesNotHideTooltipOwnedByNextFeature()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestTooltip tooltip = SimpleUITestFixtures.CreateUIComponent<TestTooltip>("Tooltip", root.transform);
            TestTooltipFeature firstFeature =
                SimpleUITestFixtures.CreateUIComponent<TestTooltipFeature>("First Feature", root.transform);
            TestTooltipFeature secondFeature =
                SimpleUITestFixtures.CreateUIComponent<TestTooltipFeature>("Second Feature", root.transform);
            firstFeature.SetTooltipForTests(tooltip);
            secondFeature.SetTooltipForTests(tooltip);
            firstFeature.Value = "A";
            secondFeature.Value = "B";

            firstFeature.OnPointerEnter(SimpleUITestFixtures.CreatePointerEvent(new Vector2(10f, 10f)));
            secondFeature.OnPointerEnter(SimpleUITestFixtures.CreatePointerEvent(new Vector2(20f, 20f)));
            firstFeature.OnPointerExit(SimpleUITestFixtures.CreatePointerEvent(new Vector2(20f, 20f)));

            Assert.IsTrue(tooltip.IsVisible);
            Assert.AreEqual("B", tooltip.CachedContext);
        }
        
    }
}
