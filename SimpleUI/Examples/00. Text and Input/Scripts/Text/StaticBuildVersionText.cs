using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Text;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Text
{
    /// <summary>
    ///     Component used to display build version of the application
    /// </summary>
    public sealed class StaticBuildVersionText : UITextObject, IWithLocalContext<string>
    {
        /// <summary>
        ///     Gets the build version
        /// </summary>
        public bool TryGetContext(out string context)
        {
            context = Application.version;
            return true;
        }
    }
}