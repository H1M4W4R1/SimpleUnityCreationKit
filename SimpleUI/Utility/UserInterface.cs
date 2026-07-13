using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Canvases;
using Systems.SimpleUI.Components.Windows;
using Systems.SimpleUI.Data;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Utility
{
    public static class UserInterface
    {
        /// <summary>
        ///     Order of regular windows, max 5K
        /// </summary>
        public const int UI_WINDOW_SORTING_ORDER = 15000;

        /// <summary>
        ///     Order of popups (always on top), max 5K
        /// </summary>
        public const int UI_POPUP_SORTING_ORDER = 20000;

        /// <summary>
        ///     Anything that should overlay windows (edges of screen etc.)
        /// </summary>
        public const int UI_OVERLAY_SORTING_ORDER = 25000;
        
        /// <summary>
        ///     Anything that should overlay everything (tooltips, mouse cursor)
        /// </summary>
        public const int UI_TOOLTIP_SORTING_ORDER = 30000;

#region Windows

        private static UIWindowCanvas _windowCanvas;

        /// <summary>
        ///     List of all open windows
        /// </summary>
        [NotNull] [ItemNotNull] internal static List<UIWindowBase> OpenWindows { get; } = new();

        /// <summary>
        ///     List of closed windows that are hidden from user view but cached for re-use
        /// </summary>
        [NotNull] [ItemNotNull] internal static List<UIWindowBase> ClosedWindows { get; } = new();

        /// <summary>
        ///     Opens a window
        /// </summary>
        /// <param name="parentWindow">Parent window to open this window as dependent of</param>
        /// <param name="force">Force to open window</param>
        /// <param name="context">Context to pass to window</param>
        /// <typeparam name="TWindowType">Window type to open</typeparam>
        /// <returns>True if window was opened, false if it was not</returns>
        public static bool OpenWindow<TWindowType>(
            [CanBeNull] UIWindowBase parentWindow = null,
            bool force = false,
            [CanBeNull] object context = null)
            where TWindowType : UIWindowBase, new()
        {
            TWindowType window = WindowsDatabase.GetExact<TWindowType>();
            Assert.IsNotNull(window, $"Window {typeof(TWindowType).Name} not found in database");
            return OpenWindow(window, parentWindow, force, context);
        }

        /// <summary>
        ///     Opens a window
        /// </summary>
        /// <param name="windowPrefab">Window prefab to open</param>
        /// <param name="parentWindow">Window to open this window as dependent of</param>
        /// <param name="force">Force to open window</param>
        /// <param name="context">Context to pass to window</param>
        /// <returns>True if window was opened, false if it was not</returns>
        public static bool OpenWindow(
            [NotNull] UIWindowBase windowPrefab,
            [CanBeNull] UIWindowBase parentWindow = null,
            bool force = false,
            [CanBeNull] object context = null)
        {
            // Check canvas setup
            if (!_windowCanvas) _windowCanvas = Object.FindAnyObjectByType<UIWindowCanvas>();
            if (!_popupCanvas) _popupCanvas = Object.FindAnyObjectByType<UIPopupCanvas>();

            // Assert canvas are valid
            Assert.IsNotNull(_windowCanvas, "WindowCanvas not found. Create Unity canvas with this component.");
            Assert.IsNotNull(_popupCanvas, "PopupCanvas not found. Create Unity canvas with this component.");

            // Check if window can be opened
            if (!windowPrefab.CanBeOpened && !force) return false;

            // Check if there is any instance with same context and forbid opening
            // This is to prevent opening multiple windows with same context
            if (!windowPrefab.AllowMultipleInstancesWithSameContext && !force)
            {
                for (int i = 0; i < OpenWindows.Count; i++)
                {
                    // Check if window is the same type with same context
                    UIWindowBase window = OpenWindows[i];
                    if (window.GetType() != windowPrefab.GetType()) continue;
                    if (window.WindowContext == context) return false;

                    // If window does not allow multiple instances with different context, forbid opening
                    // otherwise continue to next window
                    if (!windowPrefab.AllowMultipleInstancesWithDifferentContext) return false;
                }
            }

            // Find created disabled window instance
            UIWindowBase windowInstance = null;
            for (int closedWindowIndex = 0; closedWindowIndex < ClosedWindows.Count; closedWindowIndex++)
            {
                UIWindowBase window = ClosedWindows[closedWindowIndex];
                // Check if window is the same type
                if (window.GetType() != windowPrefab.GetType()) continue;
                windowInstance = window;

                // Remove from closed windows
                ClosedWindows.RemoveAt(closedWindowIndex);
                break;
            }

            // Create window instance if not found
            if (!windowInstance)
            {
                windowInstance = Object.Instantiate(windowPrefab, Vector3.zero, Quaternion.identity,
                    windowPrefab is UIPopupBase ? _popupCanvas.transform : _windowCanvas.transform);

                // We assume that RectTransform is not null at this moment
                windowInstance.RectTransformReference!.anchoredPosition = Vector2.zero;
            }

            // Set window context before show so first render sees current data
            windowInstance.WindowContext = context;

            // Set parent window before show so children can discover dependencies early
            if (parentWindow) parentWindow.Dependents.Add(windowInstance);

            // Add window to open windows and enable to ensure GameObject is active
            OpenWindows.Add(windowInstance);
            windowInstance.Show();

            // Call window opened event
            windowInstance.OnWindowOpened();
            
            // Set window position to center
            if(windowInstance.RectTransformReference)
                windowInstance.RectTransformReference.anchoredPosition = Vector2.zero;

            // Return true
            return true;
        }


        /// <summary>
        ///     Closes all open windows
        /// </summary>
        /// <param name="force">Force to close windows</param>
        /// <returns>Count of windows closed, -1 if any window could not be closed</returns>
        public static int CloseAll(bool force = false) => CloseAll<UIWindowBase>(force);

        /// <summary>
        ///     Closes all open windows
        /// </summary>
        /// <typeparam name="TWindowType">Type of window to close</typeparam>
        /// <param name="force">Force to close windows</param>
        /// <returns>Count of windows closed, -1 if any window could not be closed</returns>
        public static int CloseAll<TWindowType>(bool force = false)
            where TWindowType : UIWindowBase
        {
            int nWindowsClosed = 0;

            // Get all open windows
            for (int i = 0; i < OpenWindows.Count; i++)
            {
                if (OpenWindows[i] is not TWindowType) continue;
                if (!CanCloseWindow(OpenWindows[i]) && !force) return -1;
            }

            List<UIWindowBase> windowsToClose = new List<UIWindowBase>();
            for (int i = 0; i < OpenWindows.Count; i++)
            {
                if (OpenWindows[i] is not TWindowType) continue;
                windowsToClose.Add(OpenWindows[i]);
            }

            // Close all windows
            for (int i = 0; i < windowsToClose.Count; i++)
            {
                UIWindowBase window = windowsToClose[i];
                if (!OpenWindows.Contains(window)) continue;
                if (!CloseWindow(window, force)) continue;

                nWindowsClosed++;
            }

            return nWindowsClosed;
        }

        /// <summary>
        ///     Closes a window
        /// </summary>
        /// <param name="window">Window to close</param>
        /// <param name="force">Force close window</param>
        /// <returns>True if window was closed, false if it was not</returns>
        public static bool CloseWindow([NotNull] UIWindowBase window, bool force = false)
        {
            if (!CanCloseWindow(window) && !force) return false;

            List<UIWindowBase> dependentsCopy = new List<UIWindowBase>(window.Dependents);

            // Close all dependents
            for (int i = 0; i < dependentsCopy.Count; i++)
            {
                UIWindowBase dependentWindow = dependentsCopy[i];
                if (!OpenWindows.Contains(dependentWindow)) continue;
                CloseWindow(dependentWindow, force);
            }

            // Clear dependents list to remove stale references
            window.Dependents.Clear();

            // Remove this window from its parent's dependents list
            for (int i = 0; i < OpenWindows.Count; i++)
            {
                OpenWindows[i].Dependents.Remove(window);
            }

            // Close window (move to closed list)
            window.Hide();
            OpenWindows.Remove(window);
            ClosedWindows.Add(window);

            // Call window closed event after the window is no longer registered as open.
            window.OnWindowClosed();
            return true;
        }


        /// <summary>
        ///     Checks if a window can be closed (including dependents)
        /// </summary>
        public static bool CanCloseWindow([NotNull] UIWindowBase window)
        {
            // Check if window can be closed
            if (!window.CanBeClosed) return false;

            // Check if all dependents can be closed
            for (int i = 0; i < window.Dependents.Count; i++)
            {
                if (!CanCloseWindow(window.Dependents[i])) return false;
            }

            return true;
        }

#if UNITY_INCLUDE_TESTS
        internal static void ClearTestState()
        {
            OpenWindows.Clear();
            ClosedWindows.Clear();
            popupQueue.Clear();
            _windowCanvas = null;
            _popupCanvas = null;
        }
#endif

#endregion

#region Popups

        /// <summary>
        ///     Queue of popups to open
        /// </summary>
        private static readonly Queue<UIPopupBase> popupQueue = new();

        private static UIPopupCanvas _popupCanvas;

        /// <summary>
        ///     Opens a popup
        /// </summary>
        /// <typeparam name="TPopupType">Type of popup to open</typeparam>
        /// <returns>True if popup was opened (or queued), false if it was not</returns>
        public static bool OpenPopup<TPopupType>()
            where TPopupType : UIPopupBase, new()
        {
            // Get popup from database
            UIPopupBase popup = WindowsDatabase.GetExact<TPopupType>();
            Assert.IsNotNull(popup, $"Popup {typeof(TPopupType).Name} not found in database");

            return OpenPopup(popup);
        }

        /// <summary>
        ///     Opens a popup
        /// </summary>
        /// <param name="popup">Popup to open</param>
        /// <returns>True if popup was opened (or queued), false if it was not</returns>
        public static bool OpenPopup([NotNull] UIPopupBase popup)
        {
            // Check if any popup is open and forbid opening
            for (int nWindow = 0; nWindow < OpenWindows.Count; nWindow++)
            {
                // Skip if not popup
                if (OpenWindows[nWindow] is not UIPopupBase) continue;

                // If popup is open, add to queue
                popupQueue.Enqueue(popup);
                return true;
            }

            // No popups are open, show popup
            return OpenWindow(popup);
        }

        /// <summary>
        ///     Tries to open the next popup in the queue
        /// </summary>
        /// <returns>True if popup was opened, false if it was not</returns>
        public static bool TryOpenNextPopup()
        {
            if (popupQueue.Count == 0) return false;
            return OpenPopup(popupQueue.Dequeue());
        }

#endregion

#region Focusing Windows and Sorting

        /// <summary>
        ///     Sorts all open windows
        /// </summary>
        public static void SortAllWindows()
        {
            // Get popup and window index to sort windows sequentially based
            // on their type to make sure popups are always on top
            int popupIndex = UI_POPUP_SORTING_ORDER;
            int windowIndex = UI_WINDOW_SORTING_ORDER;

            // Sort windows automatically
            for (int i = 0; i < OpenWindows.Count; i++)
            {
                if (OpenWindows[i] is UIPopupBase)
                {
                    OpenWindows[i].SetSortingOrder(popupIndex);
                    popupIndex++;
                }
                else
                {
                    OpenWindows[i].SetSortingOrder(windowIndex);
                    windowIndex++;
                }
            }
        }

        /// <summary>
        ///     Focuses this window
        /// </summary>
        public static void FocusWindow([NotNull] UIWindowBase window)
        {
            // Prevent weird bug
            if (!OpenWindows.Contains(window)) return;
            
            // Set window in OpenWindows to last (quick swap)
            OpenWindows.Remove(window);
            OpenWindows.Add(window);

            // Sort windows
            SortAllWindows();
        }

#endregion
    }
}
