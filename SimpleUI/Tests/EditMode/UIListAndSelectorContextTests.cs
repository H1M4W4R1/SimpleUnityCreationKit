using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleUI.Context.Lists;
using TMPro;
using UnityEngine;

namespace Systems.SimpleUI.Tests
{
    public sealed class UIListAndSelectorContextTests
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
        public void ListContextExposesCountValidityAndIndexedData()
        {
            List<string> data = new List<string>
            {
                "one",
                "two"
            };
            TestListContext context = new TestListContext(data);

            Assert.AreEqual(2, context.Count);
            Assert.IsTrue(context.IsValidIndex(0));
            Assert.IsTrue(context.IsValidIndex(1));
            Assert.IsFalse(context.IsValidIndex(-1));
            Assert.IsFalse(context.IsValidIndex(2));
            Assert.AreEqual("two", context[1]);
        }

        [Test]
        public void SelectableContextSupportsIndexObjectPreviousNextAndLoopingSelection()
        {
            List<string> data = new List<string>
            {
                "alpha",
                "beta",
                "gamma"
            };
            TestSelectableContext context = new TestSelectableContext(data, 1);

            Assert.AreEqual(1, context.SelectedIndex);
            Assert.AreEqual("beta", context.SelectedItem);
            Assert.IsTrue(context.HasPrevious);
            Assert.IsTrue(context.HasNext);

            Assert.IsTrue(context.TrySelectNext());
            Assert.AreEqual(2, context.SelectedIndex);
            Assert.IsFalse(context.TrySelectNext());
            Assert.IsTrue(context.TrySelectNext(loop: true));
            Assert.AreEqual(0, context.SelectedIndex);

            Assert.IsTrue(context.TrySelectPrevious(loop: true));
            Assert.AreEqual(2, context.SelectedIndex);

            Assert.IsTrue(context.TrySelectObject("alpha"));
            Assert.AreEqual(0, context.SelectedIndex);
            Assert.IsFalse(context.TrySelectObject("missing"));
            Assert.AreEqual(0, context.SelectedIndex);
        }

        [Test]
        public void SelectableContextReportsSelectionChanges()
        {
            List<string> data = new List<string>
            {
                "alpha",
                "beta"
            };
            TestSelectableContext context = new TestSelectableContext(data, 0);

            Assert.IsTrue(context.TrySelectIndex(1));

            Assert.AreEqual(0, context.LastOldIndex);
            Assert.AreEqual(1, context.LastNewIndex);
        }

        [Test]
        public void SelectorSelectsIndexAndObjectThroughLocalContext()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestSelector selector = SimpleUITestFixtures.CreateUIComponent<TestSelector>("Selector", root.transform);
            selector.ProvidedContext = new TestSelectableContext(new List<string>
            {
                "first",
                "second",
                "third"
            });

            Assert.IsTrue(selector.TrySelectIndex(1));
            Assert.AreEqual("second", selector.SelectedItem);
            Assert.AreEqual(-1, selector.LastFrom);
            Assert.AreEqual(1, selector.LastTo);

            Assert.IsTrue(selector.TrySelectObject("third"));
            Assert.AreEqual("third", selector.SelectedItem);
            Assert.AreEqual(1, selector.LastFrom);
            Assert.AreEqual(2, selector.LastTo);
            Assert.IsFalse(selector.TrySelectIndex(99));
        }

        [Test]
        public void SelectorValidateContextClampsSelectionWhenDataShrinks()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestSelector selector = SimpleUITestFixtures.CreateUIComponent<TestSelector>("Selector", root.transform);
            List<string> data = new List<string>
            {
                "first",
                "second",
                "third"
            };
            selector.ProvidedContext = new TestSelectableContext(data, 0);
            Assert.IsTrue(selector.TrySelectIndex(2));

            data.RemoveAt(2);
            selector.ValidateContext();

            Assert.AreEqual(1, selector.ProvidedContext.SelectedIndex);
            Assert.AreEqual("second", selector.SelectedItem);
        }

        [Test]
        public void DropdownBuildsOptionsAndKeepsContextSelectionInSync()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestDropdownSelector dropdown =
                SimpleUITestFixtures.CreateUIComponent<TestDropdownSelector>("Dropdown", root.transform);
            dropdown.ProvidedContext = new TestSelectableContext(new List<string>
            {
                "small",
                "medium",
                "large"
            }, 0);

            SimpleUITestFixtures.ValidateRecursively(root);
            dropdown.CompleteLateSetupForTests();

            TMP_Dropdown dropdownComponent = dropdown.GetComponent<TMP_Dropdown>();
            Assert.AreEqual(3, dropdownComponent.options.Count);
            Assert.AreEqual("Option small", dropdownComponent.options[0].text);

            Assert.IsTrue(dropdown.SelectOption(2));

            Assert.AreEqual(2, dropdown.ProvidedContext.SelectedIndex);
            Assert.AreEqual(2, dropdownComponent.value);
            Assert.IsFalse(dropdown.SelectOption(99));
        }
    }
}
