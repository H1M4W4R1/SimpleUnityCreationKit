using JetBrains.Annotations;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleUI.Components.Windows;
using UnityEngine;

namespace Systems.SimpleUI.Data
{
    /// <summary>
    ///     Database with all known User Interface Windows
    /// </summary>
    public sealed class WindowsDatabase : AddressableDatabase<WindowsDatabase, UIWindowBase, GameObject>
    {
        [NotNull] protected override string AddressableLabel => "SimpleUI.Windows";
        
    }
}