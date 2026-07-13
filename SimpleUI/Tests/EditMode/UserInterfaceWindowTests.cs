using NUnit.Framework;
using Systems.SimpleUI.Utility;
using UnityEngine;

namespace Systems.SimpleUI.Tests
{
    public sealed class UserInterfaceWindowTests
    {
        [SetUp]
        public void SetUp()
        {
            SimpleUITestFixtures.ResetScene();
            SimpleUITestFixtures.CreateWindowCanvas();
            SimpleUITestFixtures.CreatePopupCanvas();
        }

        [TearDown]
        public void TearDown()
        {
            SimpleUITestFixtures.ResetScene();
        }

        [Test]
        public void OpenWindowCreatesVisibleInstanceWithContextAndCallback()
        {
            TestWindow prefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Window Prefab");
            object context = new object();

            bool wasOpened = UserInterface.OpenWindow(prefab, context: context);

            Assert.IsTrue(wasOpened);
            Assert.AreEqual(1, UserInterface.OpenWindows.Count);
            Assert.AreEqual(0, UserInterface.ClosedWindows.Count);

            TestWindow instance = SimpleUITestFixtures.GetOpenWindow<TestWindow>();
            Assert.AreSame(context, instance.ExposedContext);
            Assert.IsTrue(instance.IsVisible);
            Assert.IsTrue(instance.gameObject.activeSelf);
            Assert.AreEqual(1, instance.OpenedCount);
        }

        [Test]
        public void OpenWindowHonorsCanBeOpenedUnlessForced()
        {
            TestWindow prefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Blocked Window Prefab");
            prefab.Openable = false;

            Assert.IsFalse(UserInterface.OpenWindow(prefab));
            Assert.AreEqual(0, UserInterface.OpenWindows.Count);

            Assert.IsTrue(UserInterface.OpenWindow(prefab, force: true));
            Assert.AreEqual(1, UserInterface.OpenWindows.Count);
        }

        [Test]
        public void OpenWindowPreventsDuplicateContextByDefault()
        {
            TestWindow prefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Duplicate Window Prefab");
            object context = new object();

            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: context));
            Assert.IsFalse(UserInterface.OpenWindow(prefab, context: context));

            Assert.AreEqual(1, UserInterface.OpenWindows.Count);
        }

        [Test]
        public void OpenWindowAllowsDifferentContextByDefault()
        {
            TestWindow prefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Different Context Window Prefab");

            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: "first"));
            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: "second"));

            Assert.AreEqual(2, UserInterface.OpenWindows.Count);
        }

        [Test]
        public void OpenWindowCanForbidMultipleDifferentContexts()
        {
            TestNoMultipleDifferentContextWindow prefab =
                SimpleUITestFixtures.CreateWindowPrefab<TestNoMultipleDifferentContextWindow>("Single Window Prefab");

            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: "first"));
            Assert.IsFalse(UserInterface.OpenWindow(prefab, context: "second"));

            Assert.AreEqual(1, UserInterface.OpenWindows.Count);
        }

        [Test]
        public void OpenWindowCanAllowMultipleSameContexts()
        {
            TestSameContextAllowedWindow prefab =
                SimpleUITestFixtures.CreateWindowPrefab<TestSameContextAllowedWindow>("Multi Same Context Prefab");
            object context = new object();

            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: context));
            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: context));

            Assert.AreEqual(2, UserInterface.OpenWindows.Count);
        }

        [Test]
        public void CloseWindowMovesInstanceToClosedCacheAndReusesIt()
        {
            TestWindow prefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Reusable Window Prefab");

            Assert.IsTrue(UserInterface.OpenWindow(prefab));
            TestWindow firstInstance = SimpleUITestFixtures.GetOpenWindow<TestWindow>();

            Assert.IsTrue(UserInterface.CloseWindow(firstInstance));
            Assert.AreEqual(0, UserInterface.OpenWindows.Count);
            Assert.AreEqual(1, UserInterface.ClosedWindows.Count);
            Assert.AreEqual(1, firstInstance.ClosedCount);

            Assert.IsTrue(UserInterface.OpenWindow(prefab));
            TestWindow secondInstance = SimpleUITestFixtures.GetOpenWindow<TestWindow>();

            Assert.AreSame(firstInstance, secondInstance);
            Assert.AreEqual(0, UserInterface.ClosedWindows.Count);
        }

        [Test]
        public void CloseWindowHonorsCanBeClosedUnlessForced()
        {
            TestWindow prefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Closable Window Prefab");
            Assert.IsTrue(UserInterface.OpenWindow(prefab));
            TestWindow instance = SimpleUITestFixtures.GetOpenWindow<TestWindow>();
            instance.Closable = false;

            Assert.IsFalse(UserInterface.CloseWindow(instance));
            Assert.AreEqual(1, UserInterface.OpenWindows.Count);

            Assert.IsTrue(UserInterface.CloseWindow(instance, force: true));
            Assert.AreEqual(0, UserInterface.OpenWindows.Count);
        }

        [Test]
        public void CloseWindowClosesAllDependents()
        {
            TestWindow parentPrefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Parent Window Prefab");
            TestSecondWindow childPrefab = SimpleUITestFixtures.CreateWindowPrefab<TestSecondWindow>("Child Window Prefab");

            Assert.IsTrue(UserInterface.OpenWindow(parentPrefab));
            TestWindow parent = SimpleUITestFixtures.GetOpenWindow<TestWindow>();

            Assert.IsTrue(UserInterface.OpenWindow(childPrefab, parent));
            TestSecondWindow child = SimpleUITestFixtures.GetOpenWindow<TestSecondWindow>();

            Assert.AreEqual(1, parent.DependentCount);
            Assert.IsTrue(UserInterface.CloseWindow(parent));

            Assert.AreEqual(0, UserInterface.OpenWindows.Count);
            Assert.AreEqual(2, UserInterface.ClosedWindows.Count);
            Assert.AreEqual(1, child.ClosedCount);
        }

        [Test]
        public void CanCloseWindowChecksNestedDependents()
        {
            TestWindow parentPrefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Parent Prefab");
            TestSecondWindow childPrefab = SimpleUITestFixtures.CreateWindowPrefab<TestSecondWindow>("Child Prefab");
            TestThirdWindow grandchildPrefab = SimpleUITestFixtures.CreateWindowPrefab<TestThirdWindow>("Grandchild Prefab");

            Assert.IsTrue(UserInterface.OpenWindow(parentPrefab));
            TestWindow parent = SimpleUITestFixtures.GetOpenWindow<TestWindow>();
            Assert.IsTrue(UserInterface.OpenWindow(childPrefab, parent));
            TestSecondWindow child = SimpleUITestFixtures.GetOpenWindow<TestSecondWindow>();
            Assert.IsTrue(UserInterface.OpenWindow(grandchildPrefab, child));
            TestThirdWindow grandchild = SimpleUITestFixtures.GetOpenWindow<TestThirdWindow>();

            grandchild.Closable = false;

            Assert.IsFalse(UserInterface.CanCloseWindow(parent));
            Assert.IsFalse(UserInterface.CloseWindow(parent));
            Assert.AreEqual(3, UserInterface.OpenWindows.Count);
        }

        [Test]
        public void CloseAllClosesEveryMatchingWindowWithoutSkippingAfterRemoval()
        {
            TestWindow prefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Close All Prefab");

            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: "first"));
            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: "second"));
            Assert.IsTrue(UserInterface.OpenWindow(prefab, context: "third"));

            int closedCount = UserInterface.CloseAll<TestWindow>();

            Assert.AreEqual(3, closedCount);
            Assert.AreEqual(0, UserInterface.OpenWindows.Count);
            Assert.AreEqual(3, UserInterface.ClosedWindows.Count);
        }

        [Test]
        public void CloseAll_WithForcePassesForceToEachWindow()
        {
            TestWindow prefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Forced Close All Prefab");
            Assert.IsTrue(UserInterface.OpenWindow(prefab));
            TestWindow window = SimpleUITestFixtures.GetOpenWindow<TestWindow>();
            window.Closable = false;

            Assert.AreEqual(1, UserInterface.CloseAll<TestWindow>(true));
            Assert.AreEqual(0, UserInterface.OpenWindows.Count);
        }

        [Test]
        public void FocusWindowMovesWindowToTopAndSortsWindowsAndPopupsSeparately()
        {
            TestWindow windowPrefab = SimpleUITestFixtures.CreateWindowPrefab<TestWindow>("Focus Window Prefab");
            TestSecondWindow secondWindowPrefab =
                SimpleUITestFixtures.CreateWindowPrefab<TestSecondWindow>("Second Focus Window Prefab");
            TestPopup popupPrefab = SimpleUITestFixtures.CreateWindowPrefab<TestPopup>("Focus Popup Prefab");

            Assert.IsTrue(UserInterface.OpenWindow(windowPrefab));
            TestWindow first = SimpleUITestFixtures.GetOpenWindow<TestWindow>();
            Assert.IsTrue(UserInterface.OpenWindow(secondWindowPrefab));
            TestSecondWindow second = SimpleUITestFixtures.GetOpenWindow<TestSecondWindow>();
            Assert.IsTrue(UserInterface.OpenPopup(popupPrefab));
            TestPopup popup = SimpleUITestFixtures.GetOpenWindow<TestPopup>();

            UserInterface.FocusWindow(first);

            Assert.AreSame(first, UserInterface.OpenWindows[UserInterface.OpenWindows.Count - 1]);
            Assert.AreEqual(UserInterface.UI_WINDOW_SORTING_ORDER, second.SortingOrder);
            Assert.AreEqual(UserInterface.UI_WINDOW_SORTING_ORDER + 1, first.SortingOrder);
            Assert.AreEqual(UserInterface.UI_POPUP_SORTING_ORDER, popup.GetComponent<Canvas>().sortingOrder);
        }

        [Test]
        public void PopupQueueOpensNextPopupAfterCurrentPopupIsClosed()
        {
            TestPopup firstPopupPrefab = SimpleUITestFixtures.CreateWindowPrefab<TestPopup>("First Popup Prefab");
            TestSecondPopup secondPopupPrefab =
                SimpleUITestFixtures.CreateWindowPrefab<TestSecondPopup>("Second Popup Prefab");

            Assert.IsTrue(UserInterface.OpenPopup(firstPopupPrefab));
            TestPopup firstPopup = SimpleUITestFixtures.GetOpenWindow<TestPopup>();

            Assert.IsTrue(UserInterface.OpenPopup(secondPopupPrefab));
            Assert.AreEqual(1, UserInterface.OpenWindows.Count);

            Assert.IsTrue(UserInterface.CloseWindow(firstPopup));

            Assert.AreEqual(1, UserInterface.OpenWindows.Count);
            TestSecondPopup secondPopup = SimpleUITestFixtures.GetOpenWindow<TestSecondPopup>();
            Assert.AreEqual(1, secondPopup.OpenedCount);
        }
    }
}
