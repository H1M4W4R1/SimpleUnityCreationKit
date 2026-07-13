using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleUI.Components.Panels;
using Systems.SimpleUI.Utility;
using UnityEngine;

namespace Systems.SimpleUI.Components.Windows
{
    /// <summary>
    ///     Represents a user interface window
    /// </summary>
    [AutoAddressableObject("UI Windows", "SimpleUI.Windows")] [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIWindowBase : UIPanelBase
    {
        /// <summary>
        ///     If true, multiple instances of this window are allowed
        /// </summary>
        /// <remarks>
        ///     True overrides value of <see cref="AllowMultipleInstancesWithDifferentContext"/> to also
        ///     be true automatically
        /// </remarks>
        public virtual bool AllowMultipleInstancesWithSameContext => false;

        /// <summary>
        ///     If true, multiple instances of this window are allowed with different context
        /// </summary>
        public virtual bool AllowMultipleInstancesWithDifferentContext => true;

        /// <summary>
        ///     Context of this window
        /// </summary>
        protected internal object WindowContext { get; internal set; }


        /// <summary>
        ///     List of all windows that are dependent on this window
        /// </summary>
        [NotNull] [ItemNotNull] protected internal List<UIWindowBase> Dependents { get; } = new();

        /// <summary>
        ///     Focuses this window
        /// </summary>
        public void Focus() => UserInterface.FocusWindow(this);



#region Events

        /// <summary>
        ///     Event called when window is opened
        /// </summary>
        protected internal virtual void OnWindowOpened()
        {
        }

        /// <summary>
        ///     Event called when window is closed
        /// </summary>
        protected internal virtual void OnWindowClosed()
        {
        }

#endregion

#region Opening and closing windows

        /// <summary>
        ///     Check if this window can be closed
        /// </summary>
        public virtual bool CanBeClosed => true;

        /// <summary>
        ///     Check if this window can be opened, should be instance-independent
        /// </summary>
        public virtual bool CanBeOpened => true;

        /// <summary>
        ///     Closes this window
        /// </summary>
        /// <param name="force">Force close window</param>
        /// <returns>True if window was closed, false if it was not</returns>
        public virtual bool Close(bool force = false) => UserInterface.CloseWindow(this, force);

        /// <summary>
        ///     Closes all dependents of this window
        /// </summary>
        /// <param name="force">Force to close window</param>
        /// <returns>Count of windows closed, -1 if any window could not be closed</returns>
        public int CloseAllDependents(bool force = false) =>
            CloseAllDependents<UIWindowBase>(force);

        /// <summary>
        ///     Closes all dependents of this window of specified type
        /// </summary>
        /// <typeparam name="TWindowType">Type of window to close</typeparam>
        /// <param name="force">Force to close window</param>
        /// <returns>Count of windows closed, -1 if any window could not be closed</returns>
        public int CloseAllDependents<TWindowType>(bool force = false)
            where TWindowType : UIWindowBase
        {
            // Check if all dependents can be closed
            for (int i = 0; i < Dependents.Count; i++)
            {
                if (Dependents[i] is not TWindowType) continue;
                if (!UserInterface.CanCloseWindow(Dependents[i]) && !force) return -1;
            }

            // Copy list to avoid collection modification during iteration
            List<UIWindowBase> dependentsCopy = new(Dependents);

            // Close all dependents
            int nWindowsClosed = 0;
            for (int i = 0; i < dependentsCopy.Count; i++)
            {
                if (dependentsCopy[i] is not TWindowType) continue;
                UserInterface.CloseWindow(dependentsCopy[i], force);
                nWindowsClosed++;
            }

            return nWindowsClosed;
        }


        /// <summary>
        ///     Opens a dependent window for this window
        /// </summary>
        /// <typeparam name="TWindowType">Window to open</typeparam>
        /// <param name="force">Force to open window</param>
        /// <param name="context">Context to pass to window</param>
        /// <returns>True if window was opened, false if it was not</returns>
        public bool OpenDependentWindow<TWindowType>(bool force = false, [CanBeNull] object context = null)
            where TWindowType : UIWindowBase, new() => 
            UserInterface.OpenWindow<TWindowType>(this, force, context);

    

#endregion

    }
}